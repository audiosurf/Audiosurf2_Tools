using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class GlobalSettings : ReactiveObject
{
    public static char TwitchCommandPrefix { get; set; } = '!';
    public static int TwitchMaxSongsBeforeDuplicateError { get; set; } = 5;
    public static int TwitchMaxRecentAgeBeforeDuplicateError { get; set; } = 5;
    public static int TwitchMaxQueueSize { get; set; } = 25;

    public static async Task InitSettingsAsync()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!File.Exists(Path.Combine(appdata, "AS2Tools\\Settings.json")))
        {
            var newCfg = new AppSettings();
            var newCfgText = JsonSerializer.Serialize(newCfg);
            await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\Settings.json"), newCfgText);
        }

        var cfgText = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\Settings.json"));
        var cfg = JsonSerializer.Deserialize<AppSettings>(cfgText);
        TwitchCommandPrefix = cfg!.TwitchCommandPrefix;
        TwitchMaxSongsBeforeDuplicateError = cfg!.TwitchMaxSongsBeforeDuplicateError;
        TwitchMaxRecentAgeBeforeDuplicateError = cfg!.TwitchMaxRecentAgeBeforeDuplicateError;
        TwitchMaxQueueSize = cfg!.TwitchMaxQueueSize;
    }
}