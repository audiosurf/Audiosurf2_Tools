using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
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


    public SettingsViewModel()
    {
        _ = Task.Run(LoadSettings);
    }

    public async Task LoadSettings()
    {
        var cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        while (cfg == null)
        {
            await Task.Delay(100);
            cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        }
        TwitchCommandPrefix = cfg!.TwitchCommandPrefix;
        TwitchMaxQueueItemsUntilDuplicationsAllowed = cfg.TwitchMaxQueueItemsUntilDuplicationsAllowed;
        TwitchMaxRecentAgeBeforeDuplicationError = cfg.TwitchMaxRecentAgeBeforeDuplicateError;
        TwitchMaxQueueSize = cfg.TwitchMaxQueueSize;
        TwitchRequestCoolDown = cfg.TwitchRequestCoolDown;
        TwitchMaxSongLengthSeconds = cfg.TwitchMaxSongLengthSeconds;
    }

    public async Task SaveSettings()
    {
        var cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        cfg!.TwitchCommandPrefix = TwitchCommandPrefix;
        cfg.TwitchMaxQueueItemsUntilDuplicationsAllowed = TwitchMaxQueueItemsUntilDuplicationsAllowed;
        cfg.TwitchMaxRecentAgeBeforeDuplicateError = TwitchMaxRecentAgeBeforeDuplicationError;
        cfg.TwitchMaxQueueSize = TwitchMaxQueueSize;
        cfg.TwitchRequestCoolDown = TwitchRequestCoolDown;
        cfg.TwitchMaxSongLengthSeconds = TwitchMaxSongLengthSeconds;
        var text = JsonSerializer.Serialize(cfg);
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\Settings.json"), text);
        Globals.GlobalEntites["Settings"] = cfg; //Idk, just in case
    }
}