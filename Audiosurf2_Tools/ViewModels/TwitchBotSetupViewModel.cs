using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Timer = System.Timers.Timer;

namespace Audiosurf2_Tools.ViewModels;

public class TwitchBotSetupViewModel : ViewModelBase
{
    //https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=ff9dg7h1dibw47gvj9y2y5brqo0edt&redirect_uri=http%3A%2F%2Flocalhost%3A8888&scope=chat:read%20chat:edit
    //https://audiosurf2.info/user/settings
    [Reactive] public bool IsSetupOpen { get; set; }
    
    [Reactive] public string ChatChannelInput { get; set; }
    [Reactive] public string ChatChannelResult { get; set; }
    [Reactive] public bool ChatChannelDone{ get; set; }
    [Reactive] public bool ChatChannelEditing { get; set; }
    [Reactive] public string BotUsernameInput { get; set; }
    [Reactive] public string BotUsernameResult { get; set; }
    [Reactive] public bool BotUsernameDone{ get; set; }
    [Reactive] public bool BotUsernameEditing { get; set; }
    [Reactive] public string TwitchTokenInput { get; set; }
    [Reactive] public string TwitchTokenResult { get; set; }
    [Reactive] public bool TwitchTokenDone{ get; set; }
    [Reactive] public bool TwitchTokenEditing { get; set; }
    [Reactive] public string AS2LocationInput { get; set; }
    [Reactive] public string AS2LocationResult { get; set; }
    [Reactive] public bool AS2LocationDone{ get; set; }
    [Reactive] public bool AS2LocationEditing { get; set; }
    
    [Reactive] public TwitchAuthUtil TwitchAuthUtil { get; set; } = new();

    public TwitchBotSetupViewModel()
    {
        ChatChannelEditing = true;
        this.WhenAny(x => x.ChatChannelDone, 
            x => x.BotUsernameDone,
            x => x.TwitchTokenDone, 
            x => x.AS2LocationDone,
            (x, y, z, a) 
                => IsSetupOpen = !(x.Value && y.Value && z.Value && a.Value)) //idk it just works™
            .Subscribe((a) => IsSetupOpen = a);
    }

    public void EnterChannelName()
    {
        if (!string.IsNullOrWhiteSpace(ChatChannelInput))
        {
            ChatChannelResult = ChatChannelInput;
            ChatChannelDone = true;
            ChatChannelEditing = false;
            BotUsernameEditing = true;
        }
    }
    
    public void EnterBotUsername()
    {
        if (!string.IsNullOrWhiteSpace(BotUsernameInput))
        {
            BotUsernameResult = BotUsernameInput;
            BotUsernameDone = true;
            BotUsernameEditing = false;
            TwitchTokenEditing = true;
        }
    }

    public async Task EnterTwitchToken()
    {
        await Task.Delay(1);
        if (!string.IsNullOrWhiteSpace(TwitchTokenInput))
        {
            //verify or something
            if (!TwitchTokenInput.StartsWith("oauth:"))
                TwitchTokenInput = "oauth:" + TwitchTokenInput;
            TwitchTokenResult = TwitchTokenInput;
            TwitchTokenDone = true;
            TwitchTokenEditing = false;
            AS2LocationEditing = true;
            _ = Task.Run(async () =>
            {
                var dir = await ToolUtils.GetGameDirectoryAsync();
                AS2LocationInput = dir;
            });
        }
    }
    
    public async Task AutoTwitchToken()
    {
        await TwitchAuthUtil.DoOAuthFlowAsync(
            "https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=ff9dg7h1dibw47gvj9y2y5brqo0edt&redirect_uri=http%3A%2F%2Flocalhost%3A8888&scope=chat:read%20chat:edit");
        if (string.IsNullOrWhiteSpace(TwitchAuthUtil.OAuthToken))
        {
            TwitchAuthUtil = new();
            return;
        }

        TwitchTokenInput = TwitchAuthUtil.OAuthToken;
        await EnterTwitchToken();
    }
    
    public async Task EnterAS2Location()
    {
        await Task.Delay(1);
        if (!string.IsNullOrWhiteSpace(AS2LocationInput))
        {
            AS2LocationResult = AS2LocationInput;
            AS2LocationDone = true;
            AS2LocationEditing = false;
            _ = Task.Run(saveConfig);
        }
    }
    
    public async Task BrowseAS2Location()
    {
        var openFolder = new OpenFolderDialog()
        {
            Directory = "C:\\"
        };
        var dir = await openFolder.ShowAsync(((ClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).MainWindow);
        AS2LocationInput = dir ?? "";
    }

    private async Task saveConfig()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settings = new TwitchSettings()
        {
            ChatChannel = ChatChannelResult,
            BotUsername = BotUsernameResult,
            TwitchToken = TwitchTokenResult,
            AS2Location = AS2LocationResult
        };
        var text = JsonSerializer.Serialize(settings);
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchSettings.json"), text);
    }

}