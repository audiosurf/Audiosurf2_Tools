using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ATL;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Avalonia.Threading;
using Dapper;
using PlaylistsNET.Content;
using PlaylistsNET.Models;
using ReactiveUI.Fody.Helpers;
using Svg.FilterEffects;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace Audiosurf2_Tools.ViewModels;

public class TwitchBotViewModel : ViewModelBase
{
    private TwitchClient _twitchClient { get; set; }
    [Reactive] private bool IsConnected { get; set; }
    [Reactive] public bool RequestsOpen { get; set; } = true;
    [Reactive] public bool IsOpen { get; set; }

    [Reactive] public ObservableCollection<string> ChatMessages { get; set; }
    [Reactive] public ObservableCollection<TwitchRequestItem> Requests { get; set; }
    [Reactive] public TimeSpan RequestsLength { get; set; }
    [Reactive] public ObservableCollection<TwitchRequestItem> PastRequests { get; set; }

    private ConcurrentDictionary<string, DateTimeOffset> requestTimes { get; set; }
    [Reactive] public TwitchBotSetupViewModel TwitchBotSetupViewModel { get; set; }

    public TwitchBotViewModel()
    {
        RequestsLength = TimeSpan.Zero;
        ChatMessages = new();
        Requests = new();
        Requests.CollectionChanged += RequestsOnCollectionChanged;
        PastRequests = new ObservableCollection<TwitchRequestItem>();
        PastRequests.CollectionChanged += PastRequestsOnCollectionChanged;
        requestTimes = new();
        TwitchBotSetupViewModel = new();
    }

    private void PastRequestsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Add)
        {
            _ = Task.Run(RebuildPastRequestsM3U);
        }
    }

    private void RequestsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is not (NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Add)) 
            return;
        
        var tmSpn = TimeSpan.Zero;
        var times = Requests.Select(x => x.Duration);
        tmSpn = times.Aggregate(tmSpn, (current, duration) => current + duration);
        RequestsLength = tmSpn;
        _ = Task.Run(RebuildRequestsM3U);
    }

    public async Task ConnectAsync()
    {
        _twitchClient?.Disconnect();

        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        _twitchClient = new TwitchClient(customClient);
        try
        {
            _twitchClient.OnConnected += Client_OnConnected;
            _twitchClient.OnMessageReceived += Client_OnMessageReceived;
            _twitchClient.OnMessageReceived += Client_OnCommandReceived;
            _twitchClient.OnDisconnected += Client_OnDisconnected;
            _twitchClient.OnConnectionError += Client_OnConnectionError;
            _twitchClient.Initialize(
                new ConnectionCredentials(TwitchBotSetupViewModel.BotUsernameResult,
                    TwitchBotSetupViewModel.TwitchTokenResult),
                TwitchBotSetupViewModel.ChatChannelResult);
            IsConnected = await Task.Run(_twitchClient.Connect);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (IsConnected)
            {
                _ = Task.Run(requestCheckLoop);
            }
        }
    }

    public void Disconnect()
    {
        _twitchClient?.Disconnect();
        _twitchClient = default!;
    }

    public void StartOver()
    {
        TwitchBotSetupViewModel = new();
    }

    private void Client_OnConnectionError(object? sender, OnConnectionErrorArgs e)
    {
        ChatMessages.Insert(0, $"ERROR: {e.Error.Message}");
        if (ChatMessages.Count > 100)
            ChatMessages.RemoveAt(ChatMessages.Count - 1);
        IsConnected = false;
    }

    private void Client_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
    {
        ChatMessages.Insert(0, $"DISCONNECTED");
        if (ChatMessages.Count > 100)
            ChatMessages.RemoveAt(ChatMessages.Count - 1);
        IsConnected = false;
    }

    private void Client_OnCommandReceived(object? sender, OnMessageReceivedArgs e)
    {
        var cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        var prefix = cfg!.TwitchCommandPrefix;
        if (e.ChatMessage.Message.ToLower().StartsWith(prefix + "sr "))
        {
            if(!InitialCanRequestChecks(e.ChatMessage.Username))
                return;
            var length = (prefix + "sr ").Length;
            var reg =
                @"(?i)(?:youtube\.com\/\S*(?:(?:\/e(?:mbed))?\/|watch\?(?:\S*?&?v\=))|youtu\.be\/)([a-zA-Z0-9_-]{6,11})(?-i)";

            var match = Regex.Match(e.ChatMessage.Message.Substring(length), reg);
            if (match.Success && match.Groups[1].Value.Length == 11)
            {
                _ = Task.Run(() => handleSongRequestAsync(match.Groups[1].Value, e.ChatMessage.Username));
            }

            else if (cfg.TwitchEnableLocalRequests)
            {
                if (!File.Exists(Path.Combine(cfg.TwitchLocalRequestPath, e.ChatMessage.Message.Substring(length))))
                {
                    _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                        $"@{e.ChatMessage.Username} File not found, did you type the name wrong? (you also need to type the extension like .mp3 or .flac)");
                    return;
                }
                _ = Task.Run(() =>
                    handleSongRequestAsync(
                        Path.Combine(cfg.TwitchLocalRequestPath, e.ChatMessage.Message.Substring(length)),
                        e.ChatMessage.Username, false));
            }
        }
    }

    public bool InitialCanRequestChecks(string username)
    {
        var cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        
        if (!RequestsOpen)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                    $"@{username} Requests are currently disabled!"); 
            });
            return false;
        }

        var durationUpcoming = Requests.Sum(x => x.Duration.Ticks);
        var durationPast = PastRequests.Sum(x => x.Duration.Ticks);
        var fullDuration = new TimeSpan(durationUpcoming + durationPast);
        if (cfg!.TwitchQueueMaxLengthEnabled && cfg.TwitchQueueMaxLength < fullDuration)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                    $"@{username} The queue has reached the maximum length, no more requests fit in!");
            });
            return false;
        }
        
        if (requestTimes.ContainsKey(username))
        {
            if ((DateTimeOffset.Now - requestTimes[username]).TotalSeconds < cfg.TwitchRequestCoolDown)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                        $"@{username} You're on cooldown! Wait {(cfg.TwitchRequestCoolDown - (DateTimeOffset.Now - requestTimes[username]).TotalSeconds).ToString("##")} more seconds to request again!");
                });
                return false;
            }
        }

        if (cfg.TwitchQueueCutOffTimeEnabled)
        {
            if ((DateTimeOffset.Now + RequestsLength) > cfg.TwitchQueueCutOffTime)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                        $"@{username} Queue has reached cut-off time, no more requests fit in!");
                });
                return false;
            }
        }

        if (Requests.Count >= cfg.TwitchMaxQueueSize)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                    $"@{username} Queue is full, please try again later!");
            });
            return false;
        }

        return true;
    }

    public async Task ReloadRequestsPlaylist()
    {
        Requests.Clear();
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var content = new M3uContent();
        var plsText = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"));
        var playlist = content.GetFromString(plsText);
        var cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        foreach (var vid in playlist.PlaylistEntries)
        {
            if (vid.Path.Contains("https://"))
            {
                var video = await Consts.YoutubeClient.Videos.GetAsync(vid.Path);
                Requests.Add(new TwitchRequestItem(Requests,
                    video.Title,
                    video.Author.Title,
                    video.Url,
                    vid.CustomProperties.ContainsKey("EXTREQ") ? vid.CustomProperties["EXTREQ"] : "n/a",
                    video.Duration ?? TimeSpan.Zero));
            }
            else if (cfg!.TwitchEnableLocalRequests && File.Exists(vid.Path))
            {
                var video = new Track(vid.Path);
                Requests.Add(new TwitchRequestItem(Requests,
                    video.Title,
                    video.Artist,
                    video.Path,
                    vid.CustomProperties.ContainsKey("EXTREQ") ? vid.CustomProperties["EXTREQ"] : "n/a",
                    TimeSpan.FromSeconds(video.Duration)));
            }
        }
    }

    public async Task RebuildRequestsM3U()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var content = new M3uContent();
        var plsText = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"));
        var playlist = content.GetFromString(plsText);
        playlist.PlaylistEntries.Clear();
        foreach (var vid in Requests)
        {
            playlist.PlaylistEntries.Add(new M3uPlaylistEntry()
            {
                Path = vid.Location,
                CustomProperties = new() {{"EXTREQ", vid.Requester}}
            });
        }

        plsText = content.ToText(playlist);
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"), plsText);
    }

    public async Task RebuildPastRequestsM3U()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var plsLocation = Path.Combine(appdata, "AS2Tools");
        var titleDate = DateTime.Today.ToShortDateString().Replace('/', '.').Replace('\\', '.');
        if (!File.Exists(Path.Combine(plsLocation, $"{titleDate}.m3u")))
        {
            await File.WriteAllTextAsync(
                Path.Combine(plsLocation,
                    $"{titleDate}.m3u"),
                "#EXTM3U");
        }

        var content = new M3uContent();
        var plsText = await File.ReadAllTextAsync(Path.Combine(appdata, $"AS2Tools\\{titleDate}.m3u"));
        var playlist = content.GetFromString(plsText);
        playlist.PlaylistEntries.Clear();
        foreach (var vid in PastRequests)
        {
            playlist.PlaylistEntries.Add(new M3uPlaylistEntry()
            {
                Path = vid.Location,
                CustomProperties = new() {{"EXTREQ", vid.Requester}}
            });
        }

        plsText = content.ToText(playlist);
        await File.WriteAllTextAsync(Path.Combine(appdata, $"AS2Tools\\{titleDate}.m3u"), plsText);
    }

    private async Task handleSongRequestAsync(string id, string username, bool isYoutube = true)
    {
        //more checks here
        var cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        var mostRecent = PastRequests.Take(cfg!.TwitchMaxRecentAgeBeforeDuplicateError);
        if (mostRecent.Any(x => x.Location.Contains(id)))
        {
            _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                $"@{username} This was recently played!");
            return;
        }
        if (Requests.TakeLast(cfg!.TwitchMaxQueueItemsUntilDuplicationsAllowed).Any(x => x.Location.Contains(id)))
        {
            _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                $"@{username} This is already in the queue!");
            return;
        }
        if (isYoutube)
        {
            var song = await Consts.YoutubeClient.Videos.GetAsync(id);

            if (song.Duration == null)
            {
                _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                    $"@{username} Livestreams are not allowed!");
                return;
            }

            if (song.Duration?.TotalSeconds > cfg.TwitchMaxSongLengthSeconds)
            {
                _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                    $"@{username} Song too long, maximum allowed song length is {TimeSpan.FromSeconds(cfg.TwitchMaxSongLengthSeconds).TotalMinutes} Minutes!");
                return;
            }

            Requests.Add(new TwitchRequestItem(Requests, song.Title, song.Author.Title, song.Url, username,
                song.Duration ?? TimeSpan.Zero));
            _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                $"@{username} added {song.Title} to the queue!");
        }

        else
        {
            var song = new Track(id);

            if (song.Duration > cfg.TwitchMaxSongLengthSeconds)
            {
                _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                    $"@{username} Song too long, maximum allowed song length is {TimeSpan.FromSeconds(cfg.TwitchMaxSongLengthSeconds).TotalMinutes} Minutes!");
                return;
            }

            Requests.Add(new TwitchRequestItem(Requests, song.Title ?? id.Split('\\').Last(), song.Artist, id, username,
                TimeSpan.FromSeconds(song.Duration)));
            _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult,
                $"@{username} added {song.Title ?? id.Split('\\').Last()} to the queue!");
        }
        
        
        if (requestTimes.ContainsKey(username))
            requestTimes[username] = DateTimeOffset.Now;
        else
            requestTimes.TryAdd(username, DateTimeOffset.Now);
    }

    private async Task requestCheckLoop()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var plsLocation = Path.Combine(appdata, "AS2Tools");
        while (IsConnected)
        {
            await Task.Delay(5000);
            if (Requests.Count == 0)
                continue;

            await using var con = new SQLiteConnection("Data Source=" + Path.Combine(
                TwitchBotSetupViewModel.AS2LocationResult, "Audiosurf2_Data\\cache\\db\\AudiosurfMusicLibrary.aml"));
            try
            {
                var latestPlayedYoutubeEntry =
                    await con.QueryFirstOrDefaultAsync<AS2DBYoutubeEntry>(
                        "SELECT * FROM scsongs ORDER BY lastplaytime DESC LIMIT 1");
                var latestPlayedLocalEntryByLastPlayTime =
                    await con.QueryFirstOrDefaultAsync<AS2DBLocalEntry>(
                        "SELECT * FROM songs ORDER BY lastplaytime DESC LIMIT 1");
                var latestPlayedLocalEntryBySongId =
                    await con.QueryFirstOrDefaultAsync<AS2DBLocalEntry>(
                        "SELECT * FROM songs ORDER BY songid DESC LIMIT 1");
                if (latestPlayedYoutubeEntry == null)
                    throw new SqlNullValueException("CheckLoop: Latest song is null");

                var latestYoutubeId = latestPlayedYoutubeEntry.Name.Split(':')[1];
                var toRemove = Requests.FirstOrDefault(x => x.Location.Contains(latestYoutubeId));
                
                //Check local songs DB by lastPlayTime
                if (toRemove == null)
                {
                    toRemove = Requests.FirstOrDefault(x =>
                        x.Location.ToLower()
                            .Replace('/', '\\')
                            .Replace("\\\\", "\\") == latestPlayedLocalEntryByLastPlayTime.Path);
                }

                //By last added ID, maybe not use this
                //if (toRemove == null)
                //    toRemove = Requests.FirstOrDefault(x =>
                //        x.Location.ToLower()
                //            .Replace('/', '\\')
                //            .Replace("\\\\", "\\") == latestPlayedLocalEntryBySongId.Path);
                if (toRemove != null)
                {
                    if (!File.Exists(Path.Combine(plsLocation, "TwitchRequests.m3u"))) //Just in case
                        await File.WriteAllTextAsync(Path.Combine(plsLocation, "TwitchRequests.m3u"), "#EXTM3U");
                    PastRequests.Insert(0, toRemove);
                    Requests.Remove(toRemove);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                con.Close();
            }
        }
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        ChatMessages.Insert(0, $"{e.ChatMessage.DisplayName} ({e.ChatMessage.UserId}): {e.ChatMessage.Message}");
        if (ChatMessages.Count > 100)
            ChatMessages.RemoveAt(ChatMessages.Count - 1);
    }

    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        ChatMessages.Insert(0, "Connected to chat!");
    }
}