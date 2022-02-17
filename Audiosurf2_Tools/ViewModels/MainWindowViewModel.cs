using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Reactive] public InstallerViewModel InstallerViewModel { get; set; }
    [Reactive] public MoreFoldersViewModel MoreFoldersViewModel { get; set; }
    [Reactive] public PlaylistEditorViewModel PlaylistEditorViewModel { get; set; }
    [Reactive] public TwitchBotViewModel TwitchBotViewModel { get; set; }
    [Reactive] public SettingsViewModel SettingsViewModel { get; set; }
    [Reactive] public bool OpenSidebar { get; set; } = true;
    [Reactive] public bool IsLoading { get; set; } = true;
    [Reactive] public ISolidColorBrush InstallerHighlight { get; set; }
    [Reactive] public ISolidColorBrush MoreFoldersHighlight { get; set; }
    [Reactive] public ISolidColorBrush PlaylistEditorHighlight { get; set; }
    [Reactive] public ISolidColorBrush TwitchBotHighlight { get; set; }
    [Reactive] public ISolidColorBrush SettingsHighlight { get; set; }

    public MainWindowViewModel()
    {
        InstallerViewModel = new();
        MoreFoldersViewModel = new();
        PlaylistEditorViewModel = new();
        TwitchBotViewModel = new();
        SettingsViewModel = new();
        InstallerHighlight = Brushes.Transparent;
        MoreFoldersHighlight = Brushes.Transparent;
        PlaylistEditorHighlight = Brushes.Transparent;
        TwitchBotHighlight = Brushes.Transparent;
        SettingsHighlight = Brushes.Transparent;
        OpenTwitchBot();
        _ = Task.Run(LoadCreateSettingsAsync);
    }

    public async Task LoadCreateSettingsAsync()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!Directory.Exists(Path.Combine(appdata, "AS2Tools")))
            Directory.CreateDirectory(Path.Combine(appdata, "AS2Tools"));

        try
        {
            await InitSettingsAsync();
            await LoadSettingsVMAsync();
            await LoadInstallerVMAsync();
            await LoadMoreFoldersVMAsync();
            await LoadTwitchSettingsVMAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        IsLoading = false;
    }


    public async Task InitSettingsAsync()
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

        if (!File.Exists(Path.Combine(appdata, "AS2Tools\\PopOutSettings.json")))
        {
            var newCfg = new PopOutSettings();
            var newCfgText = JsonSerializer.Serialize(newCfg);
            await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\PopOutSettings.json"), newCfgText);
        }

        var popOutCfgString = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\PopOutSettings.json"));
        var popOutCfg = JsonSerializer.Deserialize<PopOutSettings>(popOutCfgString);
        Globals.GlobalEntites.Add("Settings", cfg!);
        Globals.GlobalEntites.Add("PopOutSettings", popOutCfg!);
    }

    public async Task LoadInstallerVMAsync()
    {
        var dir = await ToolUtils.GetGameDirectoryAsync();
        Dispatcher.UIThread.Post(() => InstallerViewModel.GameLocation = dir);
    }

    public async Task LoadMoreFoldersVMAsync()
    {
        var gameDir = await ToolUtils.GetGameDirectoryAsync();
        if (string.IsNullOrWhiteSpace(gameDir))
            return;
        if (!File.Exists(Path.Combine(gameDir, "MoreFolders.json")))
            return;

        Dispatcher.UIThread.Post(() => MoreFoldersViewModel.MoreFolders.Clear());
        var lines = await File.ReadAllTextAsync(Path.Combine(gameDir, "MoreFolders.json"));
        var obj = JsonSerializer.Deserialize<List<MoreFolderItem>>(lines);
        if (obj == null)
            return;
        foreach (var item in obj)
        {
            Dispatcher.UIThread.Post(() =>
            {
                item.Parent = MoreFoldersViewModel.MoreFolders;
                item.SomethingChangedEvent += MoreFoldersViewModel.HighlightSaveButton;
                MoreFoldersViewModel.MoreFolders.Add(item);
            });
        }

        Dispatcher.UIThread.Post(() => MoreFoldersViewModel.IsInitialized = true);
    }

    public async Task LoadSettingsVMAsync()
    {
        var cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        while (cfg == null)
        {
            await Task.Delay(100);
            cfg = Globals.TryGetGlobal<AppSettings>("Settings");
        }

        Dispatcher.UIThread.Post(() =>
        {
            SettingsViewModel.TwitchCommandPrefix = cfg!.TwitchCommandPrefix;
            SettingsViewModel.TwitchMaxQueueItemsUntilDuplicationsAllowed =
                cfg.TwitchMaxQueueItemsUntilDuplicationsAllowed;
            SettingsViewModel.TwitchMaxRecentAgeBeforeDuplicationError = cfg.TwitchMaxRecentAgeBeforeDuplicateError;
            SettingsViewModel.TwitchMaxQueueSize = cfg.TwitchMaxQueueSize;
            SettingsViewModel.TwitchRequestCoolDown = cfg.TwitchRequestCoolDown;
            SettingsViewModel.TwitchMaxSongLengthSeconds = cfg.TwitchMaxSongLengthSeconds;
            SettingsViewModel.TwitchQueueMaxLengthEnabled = cfg.TwitchQueueMaxLengthEnabled;
            SettingsViewModel.TwitchQueueMaxLength = cfg.TwitchQueueMaxLength;
            SettingsViewModel.TwitchQueueCutOffTimeEnabled = cfg.TwitchQueueCutOffTimeEnabled;
            SettingsViewModel.TwitchQueueCutOffTimeDate = cfg.TwitchQueueCutOffTime.Date;
            SettingsViewModel.TwitchQueueCutOffTimeTime = cfg.TwitchQueueCutOffTime.TimeOfDay;
            SettingsViewModel.TwitchEnableLocalRequests = cfg.TwitchEnableLocalRequests;
            SettingsViewModel.TwitchLocalRequestPath = cfg.TwitchLocalRequestPath;
        });
    }

    public async Task LoadTwitchSettingsVMAsync()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!File.Exists(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u")))
            await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u"), "#EXTM3U");
        if (!File.Exists(Path.Combine(appdata, "AS2Tools\\TwitchSettings.json")))
            return;
        var data = await File.ReadAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchSettings.json"));
        var settings = JsonSerializer.Deserialize<TwitchSettings>(data);
        if (settings != null)
        {
            TwitchBotViewModel.TwitchBotSetupViewModel.ChatChannelResult = settings.ChatChannel;
            TwitchBotViewModel.TwitchBotSetupViewModel.BotUsernameResult = settings.BotUsername;
            TwitchBotViewModel.TwitchBotSetupViewModel.TwitchTokenResult = settings.TwitchToken;
            TwitchBotViewModel.TwitchBotSetupViewModel.AS2LocationResult = settings.AS2Location;
            TwitchBotViewModel.TwitchBotSetupViewModel.ChatChannelDone = !string.IsNullOrWhiteSpace(settings.ChatChannel);
            TwitchBotViewModel.TwitchBotSetupViewModel.BotUsernameDone = !string.IsNullOrWhiteSpace(settings.BotUsername);
            TwitchBotViewModel.TwitchBotSetupViewModel.TwitchTokenDone = !string.IsNullOrWhiteSpace(settings.TwitchToken);
            TwitchBotViewModel.TwitchBotSetupViewModel.AS2LocationDone = !string.IsNullOrWhiteSpace(settings.AS2Location);
            Globals.GlobalEntites.Add("TwitchSettings", settings);
        }

        if (File.Exists(Path.Combine(appdata, "AS2Tools\\TwitchRequests.m3u")))
            await Dispatcher.UIThread.InvokeAsync(TwitchBotViewModel.ReloadRequestsPlaylist);

        if (File.Exists(Path.Combine(settings!.AS2Location, "MoreFolders.json")))
        {
            var lines = await File.ReadAllTextAsync(Path.Combine(settings.AS2Location, "MoreFolders.json"));
            var obj = JsonSerializer.Deserialize<List<RawMoreFolderItem>>(lines);
            if (obj == null || obj.Any(x => x.Path == Path.Combine(appdata, "AS2Tools")))
                return;

            obj.Add(new RawMoreFolderItem
            {
                Name = "Twitch Bot Requests",
                Path = Path.Combine(appdata, "AS2Tools"),
                Position = 9
            });
            lines = JsonSerializer.Serialize(obj);
            await File.WriteAllTextAsync(Path.Combine(settings.AS2Location, "MoreFolders.json"), lines);
        }
    }

    public void OpenCloseSidebar()
        => OpenSidebar = !OpenSidebar;

    public void OpenInstaller()
    {
        (InstallerHighlight, MoreFoldersHighlight, PlaylistEditorHighlight, TwitchBotHighlight, SettingsHighlight) =
            (SolidColorBrush.Parse("#33ffffff"), Brushes.Transparent, Brushes.Transparent, Brushes.Transparent,
                Brushes.Transparent);
        (InstallerViewModel.IsOpen, MoreFoldersViewModel.IsOpen, PlaylistEditorViewModel.IsOpen,
                TwitchBotViewModel.IsOpen, SettingsViewModel.IsOpen) =
            (true, false, false, false, false);
    }

    public void OpenMoreFolders()
    {
        (InstallerHighlight, MoreFoldersHighlight, PlaylistEditorHighlight, TwitchBotHighlight, SettingsHighlight) =
            (Brushes.Transparent, SolidColorBrush.Parse("#33ffffff"), Brushes.Transparent, Brushes.Transparent,
                Brushes.Transparent);
        (InstallerViewModel.IsOpen, MoreFoldersViewModel.IsOpen, PlaylistEditorViewModel.IsOpen,
                TwitchBotViewModel.IsOpen, SettingsViewModel.IsOpen) =
            (false, true, false, false, false);
    }

    public void OpenPlaylistEditor()
    {
        (InstallerHighlight, MoreFoldersHighlight, PlaylistEditorHighlight, TwitchBotHighlight, SettingsHighlight) =
            (Brushes.Transparent, Brushes.Transparent, SolidColorBrush.Parse("#33ffffff"), Brushes.Transparent,
                Brushes.Transparent);
        (InstallerViewModel.IsOpen, MoreFoldersViewModel.IsOpen, PlaylistEditorViewModel.IsOpen,
                TwitchBotViewModel.IsOpen, SettingsViewModel.IsOpen) =
            (false, false, true, false, false);
    }

    public void OpenTwitchBot()
    {
        (InstallerHighlight, MoreFoldersHighlight, PlaylistEditorHighlight, TwitchBotHighlight, SettingsHighlight) =
            (Brushes.Transparent, Brushes.Transparent, Brushes.Transparent, SolidColorBrush.Parse("#33ffffff"),
                Brushes.Transparent);
        (InstallerViewModel.IsOpen, MoreFoldersViewModel.IsOpen, PlaylistEditorViewModel.IsOpen,
                TwitchBotViewModel.IsOpen, SettingsViewModel.IsOpen) =
            (false, false, false, true, false);
    }

    public void OpenSettings()
    {
        (InstallerHighlight, MoreFoldersHighlight, PlaylistEditorHighlight, TwitchBotHighlight, SettingsHighlight) =
            (Brushes.Transparent, Brushes.Transparent, Brushes.Transparent, Brushes.Transparent,
                SolidColorBrush.Parse("#33ffffff"));
        (InstallerViewModel.IsOpen, MoreFoldersViewModel.IsOpen, PlaylistEditorViewModel.IsOpen,
                TwitchBotViewModel.IsOpen, SettingsViewModel.IsOpen) =
            (false, false, false, false, true);
    }
}