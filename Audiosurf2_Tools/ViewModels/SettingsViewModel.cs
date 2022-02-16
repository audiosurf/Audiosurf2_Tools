using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    [Reactive] public bool IsOpen { get; set; }
    [Reactive] public string TwitchCommandPrefix { get; set; } = "!";
    [Reactive] public int TwitchMaxQueueItemsUntilDuplicationsAllowed { get; set; } = 100;
    [Reactive] public int TwitchMaxRecentAgeBeforeDuplicationError { get; set; } = 5;
    [Reactive] public int TwitchMaxQueueSize { get; set; } = 25;
    [Reactive] public int TwitchRequestCoolDown { get; set; } = 30;
    [Reactive] public int TwitchMaxSongLengthSeconds { get; set; } = 600;
    
    [Reactive] public bool TwitchQueueMaxLengthEnabled { get; set; } = false;
    [Reactive] public TimeSpan TwitchQueueMaxLength { get; set; } = TimeSpan.FromHours(2); 
    [Reactive] public bool TwitchQueueCutOffTimeEnabled { get; set; } = false;
    [Reactive] public DateTimeOffset TwitchQueueCutOffTimeDate { get; set; } = DateTime.Today; 
    [Reactive] public TimeSpan TwitchQueueCutOffTimeTime { get; set; } = TimeSpan.Zero;

    [Reactive] public bool TwitchEnableLocalRequests { get; set; } = false;
    [Reactive] public string TwitchLocalRequestPath { get; set; } = "";
    

    public async Task SaveSettings()
    {
        var cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        cfg!.TwitchCommandPrefix = TwitchCommandPrefix;
        cfg.TwitchMaxQueueItemsUntilDuplicationsAllowed = TwitchMaxQueueItemsUntilDuplicationsAllowed;
        cfg.TwitchMaxRecentAgeBeforeDuplicateError = TwitchMaxRecentAgeBeforeDuplicationError;
        cfg.TwitchMaxQueueSize = TwitchMaxQueueSize;
        cfg.TwitchRequestCoolDown = TwitchRequestCoolDown;
        cfg.TwitchMaxSongLengthSeconds = TwitchMaxSongLengthSeconds;
        cfg.TwitchQueueMaxLengthEnabled = TwitchQueueMaxLengthEnabled;
        cfg.TwitchQueueMaxLength = TwitchQueueMaxLength;
        cfg.TwitchQueueCutOffTimeEnabled = TwitchQueueCutOffTimeEnabled;
        cfg.TwitchQueueCutOffTime = TwitchQueueCutOffTimeDate + TwitchQueueCutOffTimeTime;
        cfg.TwitchEnableLocalRequests = TwitchEnableLocalRequests;
        cfg.TwitchLocalRequestPath = TwitchLocalRequestPath;
        var text = JsonSerializer.Serialize(cfg);
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\Settings.json"), text);
        Globals.GlobalEntites["Settings"] = cfg; //Idk, just in case
    }

    public async Task SetLocalRequestsPathAsync(Window parent)
    {
        var openFolder = new OpenFolderDialog()
        {
            Title = "Select The Folder Where Your Local Requests Are...",
            Directory = "C:\\"
        };
        var result = await openFolder.ShowAsync(parent);
        if (string.IsNullOrWhiteSpace(result) || !Directory.Exists(result))
            return;

        TwitchLocalRequestPath = result;
    }
}