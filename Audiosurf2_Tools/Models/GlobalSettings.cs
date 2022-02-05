using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class Globals
{
    public static Dictionary<string, object> GlobalEntites { get; set; }

    static Globals()
    {
        GlobalEntites = new();
    }
    
    public static async Task InitSettingsAsync()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!Directory.Exists(Path.Combine(appdata, "AS2Tools")))
            Directory.CreateDirectory(Path.Combine(appdata, "AS2Tools"));
        if (!File.Exists(Path.Combine(appdata, "AS2Tools\\Settings.json")))
        {
            var newCfg = new AppSettings();
            var newCfgText = JsonSerializer.Serialize(newCfg);
            await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\Settings.json"), newCfgText);
        }

        var cfgText = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\Settings.json"));
        var cfg = JsonSerializer.Deserialize<AppSettings>(cfgText);
        
        if (!File.Exists(Path.Combine(appdata, "AS2Tools\\PopOutSettings.json")))
        {
            var newCfg = new PopOutSettings();
            var newCfgText = JsonSerializer.Serialize(newCfg);
            await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\PopOutSettings.json"), newCfgText);
        }
        var cfgText2 = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\PopOutSettings.json"));
        var cfg2 = JsonSerializer.Deserialize<PopOutSettings>(cfgText2);
        GlobalEntites.Add("Settings", cfg!);
        GlobalEntites.Add("PopOutSettings", cfg2!);
    }

    public static T? TryGetGlobal<T>(string key)
    {
        if (GlobalEntites.ContainsKey(key))
            if (GlobalEntites[key] is T itm)
                return itm;

        return default;
    }
}