using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
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


    public async Task SaveSettingsAsync()
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

    public async Task ExportSettingsAsync(Window parent)
    {
        var saveFile = new SaveFileDialog()
        {
            DefaultExtension = "as2tconf",
            Title = "Select where you'd like to save the settings",
            InitialFileName = "AS2ToolsConfig",
            Filters = new() {new() {Name = "Config File", Extensions = new() {"as2tconf"}}}
        };
        var location = await saveFile.ShowAsync(parent);
        if (string.IsNullOrWhiteSpace(location))
            return;

        var combo = new SettingsCombo()
        {
            GeneralSettings = Globals.TryGetGlobal<AppSettings>("Settings")!,
            TwitchSettings = Globals.TryGetGlobal<TwitchSettings>("TwitchSettings")!,
            TwitchPopOutSettings = Globals.TryGetGlobal<PopOutSettings>("PopOutSettings")!
        };
        var text = JsonSerializer.Serialize(combo);
        await File.WriteAllTextAsync(location, text);
    }

    public async Task ImportSettingsAsync(Window parent)
    {
        var openFile = new OpenFileDialog()
        {
            Title = "Select where you'd like to save the settings",
            InitialFileName = "AS2ToolsConfig",
            AllowMultiple = false,
            Filters = new() {new() {Name = "Config File", Extensions = new() {"as2tconf"}}}
        };
        var location = await openFile.ShowAsync(parent);
        if (location == null || string.IsNullOrWhiteSpace(location?[0]) || !File.Exists(location?[0]))
            return;

        var tx = await File.ReadAllTextAsync(location[0]);
        var combo = JsonSerializer.Deserialize<SettingsCombo>(tx);
        if (combo == null)
            return;

        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\Settings.json"), JsonSerializer.Serialize(combo.GeneralSettings));
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\TwitchSettings.json"), JsonSerializer.Serialize(combo.TwitchSettings));
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\PopOutSettings.json"), JsonSerializer.Serialize(combo.TwitchPopOutSettings));
        var vm = (MainWindowViewModel) parent.DataContext!;
        Globals.GlobalEntites.Remove("Settings");
        Globals.GlobalEntites.Remove("TwitchSettings");
        Globals.GlobalEntites.Remove("PopOutSettings");
        await vm.LoadCreateSettingsAsync();
    }

    public async Task DeleteAllAndExitAsync(Window parent)
    {
        //add "are you sure?"
        bool yesDelete = false;
        var askWnd = new Window()
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            Title = "Are you sure?"
        };
        var mainStack = new StackPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = new(15)
        };
        var buttonStack = new StackPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Orientation = Orientation.Horizontal,
            Spacing = 5
        };
        mainStack.Children.Add(new TextBlock()
        {
            Text = "Are you sure you want to reset everything?\n(This also deletes all previous request playlists)"
        });
        mainStack.Children.Add(buttonStack);
        
        var okBtn = new Button()
        {
            Content = "Yes, delete everything!",
            Background = SolidColorBrush.Parse("#662222")
        };
        okBtn.Click += (sender, args) =>
        {
            yesDelete = true;
            askWnd.Close();
        };
        
        var cancelBtn = new Button()
        {
            Content = "Cancel"
        };
        cancelBtn.Click += (sender, args) =>
        {
            askWnd.Close();
        };
        buttonStack.Children.Add(okBtn);
        buttonStack.Children.Add(cancelBtn);
        askWnd.Content = mainStack;

        await askWnd.ShowDialog(parent);
        if (!yesDelete)
            return;
        
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        Directory.Delete(Path.Combine(appdata, "AS2Tools"));
        Environment.Exit(0);
    }

    public async Task SetLocalRequestsPathAsync(Window parent)
    {
        var openFolder = new OpenFolderDialog()
        {
            Title = "Select the folder where your local requests files are...",
            Directory = "C:\\"
        };
        var result = await openFolder.ShowAsync(parent);
        if (string.IsNullOrWhiteSpace(result) || !Directory.Exists(result))
            return;

        TwitchLocalRequestPath = result;
    }
}