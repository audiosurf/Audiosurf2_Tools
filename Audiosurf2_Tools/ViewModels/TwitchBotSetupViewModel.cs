using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using ReactiveUI.Fody.Helpers;
using Timer = System.Timers.Timer;

namespace Audiosurf2_Tools.ViewModels;

public class TwitchBotSetupViewModel : ViewModelBase
{
    [Reactive] public string ChatChannel { get; set; } = "";
    [Reactive] public bool DoneChatChannel { get; set; } = true;
    [Reactive] public double ChatChannelOp { get; set; } = 1;
    [Reactive] public string BotUsername { get; set; } = "";
    [Reactive] public bool DoneBotUsername { get; set; } = true;
    [Reactive] public double BotUsernameOp { get; set; } = 0.5;
    [Reactive] public string AS2InfoKey { get; set; } = "";
    [Reactive] public bool DoneAS2InfoKey { get; set; } = true; 
    [Reactive] public double AS2InfoKeyOp { get; set; } = 0.5;
    [Reactive] public string TwitchToken { get; set; } = "";
    [Reactive] public bool DoneTwitchToken { get; set; } = true;
    [Reactive] public double TwitchTokenOp { get; set; } = 0.5;

    [Reactive] public string InputWatermark { get; set; }
    [Reactive] public string InputValue { get; set; } = "";
    [Reactive] public string ExternalUrl { get; set; } = "";
    [Reactive] public bool UseExtendedInput { get; set; } = false;

    [Reactive] public TwitchAuthUtil TwitchAuthUtil { get; set; } = new();

    public TwitchBotSetupViewModel()
    {
        InputWatermark = "Enter the streamers channel name (lowercase)";
    }

    public void EnterInput()
    {
        if (string.IsNullOrWhiteSpace(InputValue))
            return;
        if (string.IsNullOrWhiteSpace(ChatChannel))
        {
            UseExtendedInput = false;
            ChatChannel = InputValue;
            InputValue = "";
            InputWatermark = "Enter the bots twitch username (lowercase)";
            DoneChatChannel = false;
            BotUsernameOp = 1;
            return;
        }
        if (string.IsNullOrWhiteSpace(BotUsername))
        {
            UseExtendedInput = true;
            BotUsername = InputValue;
            InputValue = "";
            InputWatermark = "Enter your AS2.info API key";
            ExternalUrl = "https://audiosurf2.info/user/settings";
            DoneBotUsername = false;
            AS2InfoKeyOp = 1;
            return;
        }
        if (string.IsNullOrWhiteSpace(AS2InfoKey))
        {
            UseExtendedInput = true;
            AS2InfoKey = InputValue;
            InputValue = "";
            InputWatermark = "Enter your Twitch OAuth token";
            ExternalUrl = "https://id.twitch.tv/oauth2/authorize?client_id=ff9dg7h1dibw47gvj9y2y5brqo0edt&redirect_uri=http%3A%2F%2Flocalhost%3A8888&response_type=code&scope=chat:read%20chat:edit";
            DoneAS2InfoKey = false;
            TwitchTokenOp = 1;
            return;
        }
        if (string.IsNullOrWhiteSpace(TwitchToken))
        {
            UseExtendedInput = true;
            TwitchToken = InputValue;
            InputValue = "";
            InputWatermark = "";
            DoneTwitchToken = false;
            TwitchAuthUtil.TryStop();
        }
    }

    public async Task OpenExternal()
    {
        if (!ExternalUrl.Contains("twitch"))
        {
            Process.Start(new ProcessStartInfo(ExternalUrl)
            { 
                UseShellExecute = true, 
                Verb = "open" 
            }); 
        }
        else
        {
            try
            {
                await TwitchAuthUtil.DoOAuthFlowAsync(ExternalUrl);
                if (!string.IsNullOrWhiteSpace(TwitchAuthUtil.CurrentAuth.AccessToken))
                {
                    InputValue = TwitchAuthUtil.CurrentAuth.AccessToken;
                    EnterInput();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public void StartOver()
    {
        InputWatermark = "Enter the streamers channel name (lowercase)";
        InputValue = "";
        ExternalUrl = "";
        TwitchAuthUtil = new();
        UseExtendedInput = false;
        ChatChannel = "";
        ChatChannelOp = 1;
        DoneChatChannel = true;
        BotUsername = "";
        BotUsernameOp = 0.5;
        DoneBotUsername = true;
        AS2InfoKey = "";
        AS2InfoKeyOp = 0.5;
        DoneAS2InfoKey = true;
        TwitchToken = "";
        TwitchTokenOp = 0.5;
        DoneTwitchToken = true;
    }

}