using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Dapper;
using PlaylistsNET.Content;
using PlaylistsNET.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace Audiosurf2_Tools.ViewModels;

public class TwitchBotViewModel : ViewModelBase
{
    private TwitchClient _twitchClient { get; set; }
    [Reactive] private bool IsConnected { get; set; }
    [Reactive] public bool IsOpen { get; set; } = true;

    [Reactive] public ObservableCollection<string> ChatMessages { get; set; }
    [Reactive] public ObservableCollection<TwitchRequestItem> Requests { get; set; }

    [Reactive] public TwitchBotSetupViewModel TwitchBotSetupViewModel { get; set; }

    public TwitchBotViewModel()
    {
        ChatMessages = new();
        Requests = new();
        TwitchBotSetupViewModel = new();
        _ = Task.Run(loadTwitchSettingsAsync);
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if(!File.Exists(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u")))
            File.WriteAllText(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"), "#EXTM3U");
    }

    private async Task loadTwitchSettingsAsync()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!File.Exists(Path.Combine(appdata, "AS2Tools\\TwitchSettings.json")))
            return;
        var data = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchSettings.json"));
        var settings = JsonSerializer.Deserialize<TwitchSettings>(data);
        if (settings == null)
            return;
        TwitchBotSetupViewModel.ChatChannelResult = settings.ChatChannel;
        TwitchBotSetupViewModel.BotUsernameResult = settings.BotUsername;
        TwitchBotSetupViewModel.TwitchTokenResult = settings.TwitchToken;
        TwitchBotSetupViewModel.AS2LocationResult = settings.AS2Location;
        TwitchBotSetupViewModel.ChatChannelDone = true;
        TwitchBotSetupViewModel.BotUsernameDone = true;
        TwitchBotSetupViewModel.TwitchTokenDone = true;
        TwitchBotSetupViewModel.AS2LocationDone = true;
        if (File.Exists(Path.Combine(settings.AS2Location, "MoreFolders.txt")))
        {
            var all = await File.ReadAllLinesAsync(Path.Combine(settings.AS2Location, "MoreFolders.txt"));
            if (all.Any(x => x == "path=" + Path.Combine(appdata, "AS2Tools")))
                return;
            var allList = all.ToList();
            allList.Add("name=Twitch Requests");
            allList.Add("path=" + Path.Combine(appdata, "AS2Tools"));
            await File.WriteAllLinesAsync(Path.Combine(settings.AS2Location, "MoreFolders.txt"), allList);
        }
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
            _twitchClient.OnChatCommandReceived += Client_OnCommandReceived;
            _twitchClient.OnDisconnected += Client_OnDisconnected;
            _twitchClient.OnConnectionError += Client_OnConnectionError;
            _twitchClient.Initialize(
                new ConnectionCredentials(TwitchBotSetupViewModel.BotUsernameResult,
                    TwitchBotSetupViewModel.TwitchTokenResult),
                TwitchBotSetupViewModel.ChatChannelResult, '!');
            IsConnected = _twitchClient.Connect();
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
    
    private void Client_OnCommandReceived(object? sender, OnChatCommandReceivedArgs e)
    {
        if (e.Command.CommandText == "sr")
        {
            var reg =
                @"(?i)(?:youtube\.com\/\S*(?:(?:\/e(?:mbed))?\/|watch\?(?:\S*?&?v\=))|youtu\.be\/)([a-zA-Z0-9_-]{6,11})(?-i)";
            var match = Regex.Match(e.Command.ArgumentsAsString, reg);
            if (match.Success && match.Groups[1].Value.Length == 11)
            {
                _ = Task.Run(() => handleSongRequestAsync(match.Groups[1].Value, e.Command.ChatMessage.Username));
            }
        }
    }

    private async Task handleSongRequestAsync(string id, string username)
    {
        //more checks here
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var content = new M3uContent();
        var plsText =  await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"));
        var playlist = content.GetFromString(plsText);
        playlist.PlaylistEntries.Add(new M3uPlaylistEntry()
        {
            Path = "https://www.youtube.com/watch?v=" + id,
            CustomProperties = new () { {"EXTREQ", username}}
        });
        plsText = content.ToText(playlist);
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"), plsText);
        var song = await Consts.YoutubeClient.Videos.GetAsync(id);
        Requests.Add(new TwitchRequestItem(song.Title, song.Author.Title, song.Url, username, song.Duration ?? TimeSpan.Zero));
        _twitchClient.SendMessage(TwitchBotSetupViewModel.ChatChannelResult, $"@{username} added {song.Title} to the queue!");
    }

    private async Task requestCheckLoop()
    {
        Requests.Clear();
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var content = new M3uContent();
        var plsText =  await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"));
        var playlist = content.GetFromString(plsText);
        foreach (var vid in playlist.PlaylistEntries)
        {
            var video = await Consts.YoutubeClient.Videos.GetAsync(vid.Path);
            Requests.Add(new TwitchRequestItem(video.Title, 
                video.Author.Title, 
                video.Url, 
                vid.CustomProperties.ContainsKey("EXTREQ") ? vid.CustomProperties["EXTREQ"] : "n/a", 
                video.Duration ?? TimeSpan.Zero));
        }
        while (IsConnected)
        {
            await Task.Delay(5000);
            if (Requests.Count == 0)
                continue;
            
            var con = new SQLiteConnection("Data Source=" +
                                           Path.Combine(TwitchBotSetupViewModel.AS2LocationResult, "Audiosurf2_Data\\cache\\db\\AudiosurfMusicLibrary.aml"));
            try
            {
                var latestPlayed =
                    await con.QueryAsync<AS2DBYoutubeEntry>("SELECT * FROM scsongs ORDER BY lastplaytime DESC LIMIT 5");
                if (latestPlayed == null)
                    continue;
                var latest = latestPlayed.FirstOrDefault();
                var vidId = latest!.Name.Split(':')[1];
                var recentReq = Requests.FirstOrDefault();
                var id = VideoId.Parse(vidId);
                var recId = VideoId.Parse(recentReq!.Location);
                if (id == recId)
                {
                    Requests.RemoveAt(0);
                    plsText = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"));
                    playlist = content.GetFromString(plsText);
                    playlist.PlaylistEntries.RemoveAt(0);
                    plsText = content.ToText(playlist);
                    await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"), plsText);
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
            //AS DB read here
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