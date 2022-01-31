using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Audiosurf2_Tools.Models;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using Microsoft.Win32;
using Newtonsoft.Json;
using ReactiveUI;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using YoutubeExplode;

namespace Audiosurf2_Tools.ViewModels
{
    public class TwitchBotViewModel : ViewModelBase
    {
        private static HttpClient _httpClient = new();

        private TwitchConfig _twConfig = new();

        private string _statusText = "";
        private bool _isWaiting = false;
        private bool _isConnected = false;
        private ObservableCollection<string> _twitchResponses;
        private TwitchClient _twitchClient;
        private readonly YoutubeClient _youtubeClient;
        private string _twitchUsername = "";
        private string _twitchAccessToken = "";
        private string _as2InfoKey = "";
        private string _twitchChatChannel = "";
        private readonly ulong _steamId64 = 0;
        private ObservableCollection<RequestItem> _songRequests;
        public Window Parent { get; internal set; }

        public ObservableCollection<string> TwitchResponses
        {
            get => _twitchResponses;
            set => this.RaiseAndSetIfChanged(ref _twitchResponses, value);
        }

        public ObservableCollection<RequestItem> SongRequests
        {
            get => _songRequests;
            set => this.RaiseAndSetIfChanged(ref _songRequests, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        public bool IsWaiting
        {
            get => _isWaiting;
            set => this.RaiseAndSetIfChanged(ref _isWaiting, value);
        }

        public string TwitchUsername
        {
            get => _twitchUsername;
            set => this.RaiseAndSetIfChanged(ref _twitchUsername, value);
        }

        public string TwitchAccessToken
        {
            get => _twitchAccessToken;
            set => this.RaiseAndSetIfChanged(ref _twitchAccessToken, value);
        }

        public string As2InfoKey
        {
            get => _as2InfoKey;
            set => this.RaiseAndSetIfChanged(ref _as2InfoKey, value);
        }

        public string TwitchChatChannel
        {
            get => _twitchChatChannel;
            set => this.RaiseAndSetIfChanged(ref _twitchChatChannel, value);
        }

        public ReactiveCommand<Unit, Unit> ConnectBotCommand { get; set; }


        public TwitchBotViewModel()
        {
            _twitchResponses = new();
            _songRequests = new();
            _statusText = "";
            _isWaiting = false;
            _youtubeClient = new();

            var canConnect = this.WhenAny(
                x => x.TwitchUsername,
                x => x.TwitchChatChannel,
                x => x.TwitchAccessToken,
                x => x.As2InfoKey,
                (username, channel, token, as2InfoKey)
                    => (!string.IsNullOrWhiteSpace(username.Value)
                        && !string.IsNullOrWhiteSpace(channel.Value)
                        && !string.IsNullOrWhiteSpace(token.Value)
                        && !string.IsNullOrWhiteSpace(as2InfoKey.Value)));

            ConnectBotCommand = ReactiveCommand.CreateFromTask(TestAsync, canConnect);

            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            _twitchClient = new TwitchClient(customClient);


            //Get SteamID64 via registry only works for "Public" universe
            var reg = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam\\ActiveProcess", "ActiveUser", 0);
            if (reg == null || (int)reg == 0 || !ulong.TryParse(reg.ToString(), out var id3))
                return;
            _steamId64 = (1UL << 56) | (1UL << 52) | (1UL << 32) | id3;

            _ = Task.Run(_loadCreateConfig);
        }

        private void Client_OnConnectionError(object? sender, OnConnectionErrorArgs e)
        {
            TwitchResponses.Insert(0, $"ERROR: {e.Error.Message}");
            if (TwitchResponses.Count > 100)
                TwitchResponses.RemoveAt(TwitchResponses.Count - 1);
            _isConnected = false;
        }

        private void Client_OnDisconnected(object? sender, OnDisconnectedEventArgs e)
        {
            TwitchResponses.Insert(0, $"DISCONNECTED");
            if (TwitchResponses.Count > 100)
                TwitchResponses.RemoveAt(TwitchResponses.Count - 1);
            _isConnected = false;
        }

        private async Task _loadCreateConfig()
        {
            var appdataLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(Path.Combine(appdataLocation, "AS2Tools")))
                Directory.CreateDirectory(Path.Combine(appdataLocation, "AS2Tools"));
            if (File.Exists(Path.Combine(appdataLocation, "AS2Tools\\TwitchConfig.json")))
            {
                var json = await File.ReadAllTextAsync(Path.Combine(appdataLocation, "AS2Tools\\TwitchConfig.json"));
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _twConfig = JsonConvert.DeserializeObject<TwitchConfig>(json) ?? new TwitchConfig();
                    TwitchChatChannel = _twConfig.ChatChannel;
                    TwitchAccessToken = _twConfig.TwitchToken;
                    TwitchUsername = _twConfig.BotUsername;
                    As2InfoKey = _twConfig.AS2InfoKey;
                }
            }
            else
            {
                var f = File.Create(Path.Combine(appdataLocation, "AS2Tools\\TwitchConfig.json"));
                await f.DisposeAsync();
            }
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
                    _ = Task.Run(() => sendQueueRequestAsync(match.Groups[1].Value, e.Command.ChatMessage.Username));
                }
            }
        }

        private async Task sendQueueRequestAsync(string videoId, string username)
        {
            using (var msg = new HttpRequestMessage(HttpMethod.Post, "https://audiosurf2.info/twitch/queue_add"))
            {
                var values = new Dictionary<string, string> {
                    { "username", username },
                    { "youtubeId", videoId },
                    { "apiKey", As2InfoKey }
                };
                var content = new FormUrlEncodedContent(values);
                msg.Content = content;
                var resp = await _httpClient.SendAsync(msg);
                var responseString = await resp.Content.ReadAsStringAsync();

                if (responseString.Length < 200 && resp.IsSuccessStatusCode)
                {
                    _twitchClient.SendMessage(TwitchChatChannel, "@" + username + " " + responseString);
                    if (responseString.Contains("Already in queue!"))
                        return;
                    await refreshRequests();
                }
                else
                {
                    Console.WriteLine("response error " + responseString);
                }
            }
        }

        private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs e)
        {
            TwitchResponses.Insert(0, $"{e.ChatMessage.DisplayName} ({e.ChatMessage.UserId}): {e.ChatMessage.Message}");
            if (TwitchResponses.Count > 100) 
                TwitchResponses.RemoveAt(TwitchResponses.Count - 1);

        }

        private void Client_OnConnected(object? sender, OnConnectedArgs e)
        {
            TwitchResponses.Insert(0, "Connected to chat!");
        }

        private async Task refreshLoop()
        {
            while (_isConnected)
            {
                await refreshRequests();
                await Task.Delay(10000);
            }
        }

        private async Task refreshRequests()
        {
            var apiResp =
                await _httpClient.GetFromJsonAsync<List<RequestApiResponse>>(
                    "https://audiosurf2.info/api/queue/" + _steamId64);
            if (apiResp == null)
                return;
            SongRequests.Clear();
            foreach (var apiRequest in apiResp)
            {
                var itm = new RequestItem(apiRequest.Song, apiRequest.Channel, XmlConvert.ToTimeSpan(apiRequest.Duration));
                SongRequests.Add(itm);
            }
        }

        private async Task TestAsync()
        {
            if (_isConnected)
            {
                _twitchClient.Disconnect();
                _isConnected = false; 
                var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
                WebSocketClient customClient = new WebSocketClient(clientOptions);
                _twitchClient = new TwitchClient(customClient); ;
            }

            try
            {
                _twitchClient.OnConnected += Client_OnConnected;
                _twitchClient.OnMessageReceived += Client_OnMessageReceived;
                _twitchClient.OnChatCommandReceived += Client_OnCommandReceived;
                _twitchClient.OnDisconnected += Client_OnDisconnected;
                _twitchClient.OnConnectionError += Client_OnConnectionError;
                _twitchClient.Initialize(new ConnectionCredentials(TwitchUsername, TwitchAccessToken),
                    TwitchChatChannel, '!');
                _isConnected = _twitchClient.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (!_isConnected)
                {
                    var mb = MessageBoxManager
                        .GetMessageBoxStandardWindow("Unable to start the bot", "Make sure the usernames match and the access token is for the bot user!\n(If you don't use a separate bot account,\nbot username and channel name should be the same)");
                    await mb.ShowDialog(Parent);
                }
                else
                {
                    _ = Task.Run(refreshLoop);
                }
            }
        }

        private async Task DisconnectAsync()
        {
            if (_isConnected)
            {
                _twitchClient.Disconnect();
                _isConnected = false;
                var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
                WebSocketClient customClient = new WebSocketClient(clientOptions);
                _twitchClient = new TwitchClient(customClient);
            }
            TwitchResponses.Insert(0, $"Disconnected");
            await Task.Delay(10);
        }

        private async Task saveConfigAsync()
        {
            var appdataLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!Directory.Exists(Path.Combine(appdataLocation, "AS2Tools")))
                Directory.CreateDirectory(Path.Combine(appdataLocation, "AS2Tools"));
            _twConfig.ChatChannel = TwitchChatChannel;
            _twConfig.TwitchToken = TwitchAccessToken;
            _twConfig.BotUsername = TwitchUsername;
            _twConfig.AS2InfoKey = As2InfoKey;
            var str = JsonConvert.SerializeObject(_twConfig);
            await File.WriteAllTextAsync(Path.Combine(appdataLocation, "AS2Tools\\TwitchConfig.json"), str);
        }

        public async Task GetAccessTokenAsync()
        {
            var mb = MessageBoxManager
                .GetMessageBoxStandardWindow("Important Info", "You'll be redirected to an OAuth token generator.\n" +
                                                               "Make sure you're logged into Twitch with the account you want\n" +
                                                               "to use for this (the one to respond to commands)", ButtonEnum.OkCancel);
            var result = await mb.ShowDialog(Parent);
            if (result != ButtonResult.Ok)
                return;

            Process.Start(new ProcessStartInfo("https://twitchapps.com/tmi/")
            {
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public async Task GetAS2TokenAsync()
        {
            var mb = MessageBoxManager
                .GetMessageBoxStandardWindow("Important Info", "You'll be redirected to the Audiosurf2.info site.\n" +
                                                               "At the bottom you'll find the API key needed.\n" +
                                                               "You need to be logged in.", ButtonEnum.OkCancel);
            var result = await mb.ShowDialog(Parent);
            if (result != ButtonResult.Ok)
                return;

            Process.Start(new ProcessStartInfo("https://audiosurf2.info/user/settings")
            {
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
