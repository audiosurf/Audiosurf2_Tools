using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Microsoft.Win32;

namespace Audiosurf2_Tools.ViewModels
{
    public class InstallerViewModel : ViewModelBase
    {
        private HttpClient _client = new();
        private string _gameLocation = "";
        private int _progressValue = 0;
        private string _statusText = "";

        public string GameLocation
        {
            get => _gameLocation;
            set => this.RaiseAndSetIfChanged(ref _gameLocation, value);
        }

        public int ProgressValue
        {
            get => _progressValue;
            set => this.RaiseAndSetIfChanged(ref _progressValue, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        public ReactiveCommand<Unit, Unit> AutoFindCommand { get; }
        public ReactiveCommand<Window, Unit> BrowseCommand { get; }
        public ReactiveCommand<Unit, Unit> InstallCommand { get; }

        public InstallerViewModel()
        {
            AutoFindCommand = ReactiveCommand.CreateFromTask(autoFindGameFolderAsync);
            BrowseCommand = ReactiveCommand.CreateFromTask((Window param) => browseGameFolderAsync(param));
            InstallCommand = ReactiveCommand.CreateFromTask(installPatchAsync);
        }

        private async Task autoFindGameFolderAsync()
        {
            if (Directory.Exists("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Audiosurf 2"))
            {
                GameLocation = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Audiosurf 2";
            }
            else
            {
                try
                {
                    //GameID: 235800
                    object steamPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam", "InstallPath", null) ?? throw new KeyNotFoundException("Steam installation not found");
                    var vdfText = await File.ReadAllLinesAsync(Path.Combine(steamPath.ToString() ?? throw new InvalidOperationException("Invalid Steam install path"), "config\\libraryfolders.vdf"));
                    var gameLineText = vdfText.FirstOrDefault(x => x.Contains("\"235800\"")) ?? throw new DirectoryNotFoundException("Game isn't listed as installed in Steam");
                    var lineIndex = Array.IndexOf(vdfText, gameLineText);
                    for (int i = lineIndex - 1; i >= 0; i--)
                    {
                        if (vdfText[i].Contains("\"path\""))
                        {
                            vdfText[i] = vdfText[i].Replace("\"path\"", "");
                            var libraryPath = vdfText[i][(vdfText[i].IndexOf('\"') + 1)..vdfText[i].LastIndexOf('\"')];
                            libraryPath = libraryPath.Replace("\\\\", "\\");
                            GameLocation = Path.Combine(libraryPath, "steamapps\\common\\Audiosurf 2");
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(GameLocation))
                        throw new DirectoryNotFoundException("Unable to find Audiosurf 2 directory");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    //MessageBox here with error or something
                }
            }
        }

        private async Task browseGameFolderAsync(Window parent)
        {
            var dialog = new OpenFolderDialog();
            dialog.Directory = "C:\\";
            dialog.Title = "Select where Audiosurf 2 is installed (The one with Audiosurf2.exe)";
            var selected = await dialog.ShowAsync(parent);
            if (!string.IsNullOrWhiteSpace(selected) && Directory.Exists(selected))
            {
                var dir = new DirectoryInfo(selected);
                if (dir.GetFiles().Any(x => x.Name == "Audiosurf2.exe"))
                    GameLocation = selected;
            }
        }

        private async Task installPatchAsync()
        {
            if (string.IsNullOrWhiteSpace(GameLocation) || !Directory.Exists(GameLocation))
            {
                StatusText = "Invalid Directory";
                return;
            }

            var dir = new DirectoryInfo(GameLocation);
            if (dir.GetFiles().All(x => x.Name != "Audiosurf2.exe"))
            {
                StatusText = "Game not found in directory";
                return;
            }

            StatusText = "Downloading files...";
            ProgressValue = 5;
            using (var msg = new HttpRequestMessage(HttpMethod.Get, ""))
            {
                var data = await _client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);
                ProgressValue = 20;
                StatusText = "Reading data...";
                using (var zip = new ZipArchive(await data.Content.ReadAsStreamAsync()))
                {
                    ProgressValue = 60;
                    StatusText = "Extracting contents...";

                    #if !DEBUG
                    zip.ExtractToDirectory(GameLocation);              
                    #endif
                }
            }
            ProgressValue = 100;
            StatusText = "Done! Have fun c:";
            //await Task.Delay(2500);
            //GC.Collect();
        }
    }
}
