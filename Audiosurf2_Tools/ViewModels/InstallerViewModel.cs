using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Audiosurf2_Tools.Entities;
using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Console = System.Console;

namespace Audiosurf2_Tools.ViewModels;

public class InstallerViewModel : ViewModelBase
{
    [Reactive] public bool IsOpen { get; set; }

    [Reactive] public string GameLocation { get; set; }
    [Reactive] public string StatusText { get; set; } = "nothing";
    [Reactive] public int ProgressValue { get; set; }
    [Reactive] public ReactiveCommand<Unit, Unit> InstallCommand { get; set; }

    [Reactive] public bool IsGameInstalled { get; set; }
    [Reactive] public bool IsPatchInstalled { get; set; }

    [Reactive] public string UnityVersion { get; set; }
    [Reactive] public string BetaChannel { get; set; }

    [Reactive] public string PatchVersion { get; set; }
    [Reactive] public string PatchChannel { get; set; }

    public InstallerViewModel()
    {
        var watCmd = ReactiveCommand.CreateFromTask<string>(FindGameAndPatchVersionAsync);
        this.WhenAnyValue(x => x.GameLocation)
            .InvokeCommand(watCmd);
        var canInstall = this.WhenAny(x => x.GameLocation, (thing) => !string.IsNullOrWhiteSpace(thing.Value));
        InstallCommand = ReactiveCommand.CreateFromTask(InstallPatchAsync, canInstall);
    }

    public async Task AutoFindAsync()
    {
        GameLocation = await ToolUtils.GetGameDirectoryAsync();
    }
    
    public async Task BrowserLocation(Window parent)
    {
        var openFol = new OpenFolderDialog()
        {
            Directory = "C:\\",
            Title = "Select the Audiosurf 2 install location"
        };
        var result = await openFol.ShowAsync(parent);
        if (!string.IsNullOrWhiteSpace(result))
        {
            if (Directory.Exists(result))
            {
                var dirInfo = new DirectoryInfo(result);
                if (dirInfo.GetFiles().Any(x => x.Name.Contains("Audiosurf2.exe")))
                {
                    IsGameInstalled = false;
                    IsPatchInstalled = false;
                    GameLocation = result;
                }
            }
        }
    }

    public async Task FindGameAndPatchVersionAsync(string x)
    {
        if (string.IsNullOrWhiteSpace(GameLocation))
            return;
        IsGameInstalled = true;

        var as2Ver = FileVersionInfo.GetVersionInfo(Path.Combine(GameLocation, "Audiosurf2.exe"));

        if (as2Ver.FileVersion?.StartsWith("2017") == true)
        {
            UnityVersion = as2Ver.FileVersion;
            BetaChannel = "none";
        }
        else if (as2Ver.FileVersion?.StartsWith("5.5") == true)
        {
            UnityVersion = as2Ver.FileVersion;
            BetaChannel = "beforejune2018_xp";
        }
        else if (as2Ver.FileVersion?.StartsWith("5.2") == true)
        {
            UnityVersion = as2Ver.FileVersion;
            BetaChannel = "before_videosurf";
        }

        if (Directory.Exists(Path.Combine(GameLocation, "Audiosurf2_Data\\patchupdater")))
        {
            IsPatchInstalled = true;
            var version = await File.ReadAllTextAsync(Path.Combine(GameLocation,
                "Audiosurf2_Data\\patchupdater\\installedversion.txt"));
            if (version.Contains('.'))
            {
                PatchVersion = version;
                PatchChannel = "latest";
            }
            else
            {
                PatchVersion = version;
                PatchChannel = "beta patch";
            }
        }
    }

    public async Task InstallPatchAsync()
    {
        try
        {
            ProgressValue = 0;
            StatusText = "Downloading Patch .zip";
            var zipStream = await Consts.HttpClient.GetStreamAsync("https://aiae.ovh/newpatch/audiosurf2_newpatch.zip");
            ProgressValue = 60;
            StatusText = "Extracting files...";
            var zip = new ZipArchive(zipStream);
            zip.ExtractToDirectory(GameLocation, true);
            ProgressValue = 100;
            StatusText = "Done";
        }
        catch (Exception e)
        {
            StatusText = "Something went wrong: " + e;
        }
    }
}