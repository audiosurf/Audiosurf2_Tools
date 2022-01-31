using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Audiosurf2_Tools.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using PlaylistsNET.Content;
using PlaylistsNET.Models;
using ReactiveUI;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Audiosurf2_Tools.ViewModels
{
    public class PlaylistEditorViewModel : ViewModelBase
    {
        private ObservableCollection<IPlaylistItem> _playlistItems;
        private string _currentPlaylistLocation = "";
        private string _currentPlaylistName = "n/a";
        private string _youTubeURL = "";
        private bool _canAddYoutubeCommand;


        public ObservableCollection<IPlaylistItem> PlaylistItems
        {
            get => _playlistItems;
            set => this.RaiseAndSetIfChanged(ref _playlistItems, value);
        }

        public int AmountLocal => PlaylistItems.Count(x => !x.Location.Contains("://"));
        public int AmountYouTube => PlaylistItems.Count(x => x.Location.Contains("://"));

        public string CurrentPlaylistLocation
        {
            get => _currentPlaylistLocation;
            set => this.RaiseAndSetIfChanged(ref _currentPlaylistLocation, value);
        }

        public string CurrentPlaylistName
        {
            get => _currentPlaylistName;
            set => this.RaiseAndSetIfChanged(ref _currentPlaylistName, value);
        }

        public string YouTubeURL
        {
            get => _youTubeURL;
            set => this.RaiseAndSetIfChanged(ref _youTubeURL, value);
        }

        public ReactiveCommand<Unit, Unit> AddYouTubeCommand { get; }

        public bool CanAddYoutubeCommand
        {
            get => _canAddYoutubeCommand;
            set => this.RaiseAndSetIfChanged(ref _canAddYoutubeCommand, value);
        }

        public Window Parent { get; init; }
        public PlaylistEditorViewModel()
        {
            _playlistItems = new();
            CurrentPlaylistLocation = "";
            CurrentPlaylistName = "n/a";

            var canAddYouTube = this.WhenAnyValue(x => x.YouTubeURL)
                .Subscribe(x => CanAddYoutubeCommand = ((VideoId.TryParse(x) != null || PlaylistId.TryParse(x) != null)));
            AddYouTubeCommand = ReactiveCommand.CreateFromTask(AddYouTubeURLAsync, this.WhenAnyValue(x => x.CanAddYoutubeCommand));
        }

        public async Task OpenPlaylistAsync()
        {
            RemoveAll();
            var f = new FileDialogFilter()
            {
                Extensions = new() {"m3u", "m3u8" },
                Name = "Playlist"
            };
            var fileDialog = new OpenFileDialog()
            {
                AllowMultiple = false,
                Title = "Select a playlist file!",
                Directory = "C:\\",
                Filters = new() { f }
            };
            var result = await fileDialog.ShowAsync(Parent);
            if (result == null || result?.Length == 0)
                return;
            var content = File.Open(result![0], FileMode.Open);
            var pl = new M3uContent().GetFromStream(content);
            CurrentPlaylistLocation = result[0];
            CurrentPlaylistName = result[0].Split("\\").Last();
            foreach (var entry in pl.PlaylistEntries)
            {
                try
                {
                    if (VideoId.TryParse(entry.Path) == null)
                    {
                        var plItem = new LocalPlaylistItem(entry.Path.Replace("file:///", ""));
                        await plItem.LoadInfoAsync();
                        PlaylistItems.Add(plItem);
                        this.RaisePropertyChanged("AmountLocal");
                    }
                    else
                    {
                        var plItem = new YouTubePlaylistItem(entry.Path.Replace("file:///", ""));
                        await plItem.LoadInfoAsync();
                        PlaylistItems.Add(plItem);
                        this.RaisePropertyChanged("AmountYouTube");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //throw;
                }
            }
        }

        public void QuitApp() => Environment.Exit(0);

        private async Task AddYouTubeURLAsync()
        {
            if (VideoId.TryParse(YouTubeURL) != null)
            {
                var plItem = new YouTubePlaylistItem(YouTubeURL);
                await plItem.LoadInfoAsync();
                PlaylistItems.Add(plItem);
                this.RaisePropertyChanged("AmountYouTube");
            }
            else
            {
                await foreach (var item in YouTubePlaylistItem.YTClient.Playlists.GetVideosAsync(YouTubeURL))
                {
                    var plItem = new YouTubePlaylistItem(item.Url)
                    {
                        Title = item.Title,
                        Artist = item.Author.Title,
                        Duration = item.Duration!.Value
                    };
                    var t = await YouTubePlaylistItem.HClient.GetByteArrayAsync(item.Thumbnails.OrderBy(t => t.Resolution.Area).FirstOrDefault()!.Url);
                    await using (var bt = new MemoryStream(t))
                    {
                        bt.Position = 0;
                        plItem.CoverImage = new Bitmap(bt);
                    }

                    plItem.Loaded = true;
                    PlaylistItems.Add(plItem);
                    this.RaisePropertyChanged("AmountYouTube");
                }
            }
            YouTubeURL = "";
        }

        public async Task AddLocalSongAsync()
        {
            var f = new FileDialogFilter()
            {
                Extensions = new() { "mp3", "mp4", "flac", "ogg", "wav", "m4a", "wma", "alac" },
                Name = "Media Files"
            };
            var fileDialog = new OpenFileDialog()
            {
                AllowMultiple = true,
                Title = "Select one or multiple audio files!",
                Directory = "C:\\",
                Filters = new() { f }
            };
            var result = await fileDialog.ShowAsync(Parent);
            if (result == null || result?.Length == 0)
                return;

            foreach (var song in result!)
            {
                try
                {
                    var plItem = new LocalPlaylistItem(song);
                    await plItem.LoadInfoAsync();
                    PlaylistItems.Add(plItem);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public async Task SaveAsAsync()
        {
            var fileDialog = new SaveFileDialog()
            {
                DefaultExtension = "m3u",
                Title = "Select where you'd like to save your playlist",
                Filters = new() { new() {Extensions = new(){ "m3u"}, Name = "Playlist"} }
            };
            var saveLocation = await fileDialog.ShowAsync(Parent);
            if (saveLocation == null)
                return;

            var playlist = new M3uPlaylist();
            foreach (var item in PlaylistItems)
            {
                playlist.PlaylistEntries.Add(new M3uPlaylistEntry()
                {
                    Path = item.Location
                });
            }

            var content = new M3uContent();
            var playlistText = content.ToText(playlist);
            await File.WriteAllTextAsync(saveLocation, playlistText);
            CurrentPlaylistLocation = saveLocation;
            CurrentPlaylistName = saveLocation.Split("\\").Last();
        }

        public async Task SaveAsync()
        {
            if (!File.Exists(CurrentPlaylistLocation))
            {
                await SaveAsAsync();
                return;
            }
            var playlist = new M3uPlaylist();
            foreach (var item in PlaylistItems)
            {
                playlist.PlaylistEntries.Add(new M3uPlaylistEntry()
                {
                    Path = item.Location
                });
            }

            var content = new M3uContent();
            var playlistText = content.ToText(playlist);
            await File.WriteAllTextAsync(CurrentPlaylistLocation, playlistText);
        }

        public void RemoveAll()
        {
            PlaylistItems.Clear();
            this.RaisePropertyChanged("AmountLocal");
            this.RaisePropertyChanged("AmountYouTube");
        }

        public void MoveUp(IPlaylistItem item)
        {
            var currentIndex = PlaylistItems.IndexOf(item);
            if (currentIndex == 0)
                return;
            PlaylistItems.Insert(currentIndex - 1, item);
            PlaylistItems.RemoveAt(currentIndex + 1);
        }

        public void MoveDown(IPlaylistItem item)
        {
            var currentIndex = PlaylistItems.IndexOf(item);
            if (currentIndex == (PlaylistItems.Count - 1))
                return;
            PlaylistItems.Insert(currentIndex + 2, item);
            PlaylistItems.RemoveAt(currentIndex);
        }

        public void RemoveItem(IPlaylistItem item)
        {
            PlaylistItems.Remove(item);
            this.RaisePropertyChanged("AmountLocal");
            this.RaisePropertyChanged("AmountYouTube");
        }
    }
}
