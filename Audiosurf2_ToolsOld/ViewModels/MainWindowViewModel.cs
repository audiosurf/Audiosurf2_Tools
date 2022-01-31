using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Audiosurf2_Tools.Views;
using Avalonia.Controls;
using JetBrains.Annotations;
using Microsoft.Win32;
using ReactiveUI;

namespace Audiosurf2_Tools.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private TwitchBotViewModel twitchBotViewModel;
        private string _gamePath = "";
        private bool _isInitializing = true;
        private bool _gamePatched = false;
        private Version _gamePatchVersion = new Version(0,0,0);
        private string _initStatusText = "Loading...";

        public bool IsInitializing
        {
            get => _isInitializing;
            set => this.RaiseAndSetIfChanged(ref _isInitializing, value);
        }

        public bool GamePatched
        {
            get => _gamePatched;
            set => this.RaiseAndSetIfChanged(ref _gamePatched, value);
        }

        public Version GamePatchVersion
        {
            get => _gamePatchVersion;
            set => this.RaiseAndSetIfChanged(ref _gamePatchVersion, value);
        }

        public string InitStatusText
        {
            get => _initStatusText;
            set => this.RaiseAndSetIfChanged(ref _initStatusText, value);
        }

        public MainWindowViewModel()
        {
            _ = Task.Run(checkPatchAsync);
        }

        public void OpenInstallerWindow(Window parent)
        {
            var installer = new InstallerWindow()
            {
                DataContext = new InstallerViewModel(),
                PreviousWindow = parent
            };
            parent.Hide();
            installer.Show();
        }
        
        public void OpenMoreFoldersWindow(Window parent)
        {
            var foldersWindow = new MoreFoldersWindow()
            {
                DataContext = new MoreFoldersViewModel
                {
                    GamePath = _gamePath
                },
                PreviousWindow = parent
            };
            parent.Hide();
            foldersWindow.Show();
        }

        public void OpenPlaylistEditorWindow(Window parent)
        {
            var foldersWindow = new PlaylistEditorWindow()
            {
                DataContext = new PlaylistEditorViewModel()
                {
                    Parent = parent
                },
                PreviousWindow = parent
            };
            parent.Hide();
            foldersWindow.Show();
        }

        public void OpenTwitchBotWindow(Window parent)
        {
            if (twitchBotViewModel == null)
                twitchBotViewModel = new();
            var foldersWindow = new TwitchBotWindow()
            {
                PreviousWindow = parent,
                DataContext = twitchBotViewModel
            };
            twitchBotViewModel.Parent = foldersWindow;
            parent.Hide();
            foldersWindow.Show();
        }

        private async Task<string> findGameDirectoryAsync()
        {
            string directory = "";
            if (Directory.Exists("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Audiosurf 2"))
            {
                directory = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Audiosurf 2";
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
                            directory = Path.Combine(libraryPath, "steamapps\\common\\Audiosurf 2");
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(directory))
                        throw new DirectoryNotFoundException("Unable to find Audiosurf 2 directory");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    //MessageBox here with error or something
                }
            }

            return directory;
        }

        private async Task checkPatchAsync()
        {
            //await Task.Delay(2500);
            //await changeTextAsync("Checking Patch Install...");
            //await Task.Delay(1500);
            _gamePath = await findGameDirectoryAsync();
            if (Directory.Exists(Path.Combine(_gamePath, "Audiosurf2_Data\\patchupdater")))
            {
                GamePatched = true;
                //VersionCheck here
                await changeTextAsync("Checking Patch Version...");
                //await Task.Delay(1500);
                var versionString = File.ReadAllTextAsync(Path.Combine(_gamePath, "Audiosurf2_Data\\patchupdater\\installedversion.txt"));
                GamePatchVersion = new Version("0.0.8");
            }
            await changeTextAsync("Ready :)");
            //await Task.Delay(1500);
            await changeTextAsync("");
            IsInitializing = false;
        }

        private async Task changeTextAsync(string newText)
        {
            var oldLength = InitStatusText.Length;
            for (int i = oldLength - 1; i >= 0; i--)
            {
                InitStatusText = InitStatusText[0..^1];
                await Task.Delay(10);
            }

            for (int i = 0; i < newText.Length; i++)
            {
                InitStatusText += newText[i];
                await Task.Delay(10);
            }
        }
    }
}