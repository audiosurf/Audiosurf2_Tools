using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using PlaylistsNET.Content;
using PlaylistsNET.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Audiosurf2_Tools.ViewModels;

public class PlaylistEditorViewModel : ViewModelBase
{
    [Reactive] public bool IsOpen { get; set; } = false;

    
    [Reactive] public string PlaylistPath { get; set; } = "";
    [Reactive] public string PlaylistName { get; set; } = "<New Playlist>";

    [Reactive] public string InputLink { get; set; }
    [Reactive] public ObservableCollection<BasePlaylistItem> PlaylistItems { get; set; }

    
    public int SelectedItemDummy { get => -1; set => this.RaisePropertyChanged(nameof(SelectedItemDummy)); }
    public int LocalAmount => PlaylistItems.Count(x => x is LocalPlaylistItem);
    public int YoutubeAmount => PlaylistItems.Count(x => x is YoutubePlaylistItem);
    public TimeSpan TotalDuration => TimeSpan.FromMilliseconds(PlaylistItems.Sum(x => x.Duration.TotalMilliseconds));


    [Reactive] public ReactiveCommand<string, Unit> AddYoutubeCommand { get; set; }

    public PlaylistEditorViewModel()
    {
        PlaylistItems = new();
        PlaylistItems.CollectionChanged += (sender, args) =>
        {
            this.RaisePropertyChanged(nameof(LocalAmount));
            this.RaisePropertyChanged(nameof(YoutubeAmount));
            this.RaisePropertyChanged(nameof(TotalDuration));
        };
        var canAdd = this.WhenAny(x => x.InputLink, 
            (thing) => Uri.TryCreate(thing.Value, UriKind.Absolute, out _));
        AddYoutubeCommand = ReactiveCommand.CreateFromTask<string>(AddYoutubeAsync, canAdd);
    }

    public async Task AddYoutubeAsync(string url)
    {
        var vid = VideoId.TryParse(url);
        var pls = PlaylistId.TryParse(url);
        if (vid != null)
        {
            var itm = new YoutubePlaylistItem(vid, PlaylistItems);
            _ = Task.Run(itm.LoadInfoAsync);
            PlaylistItems.Add(itm);
        }
        else if (pls != null)
        {
            var plsItems = await Consts.YoutubeClient.Playlists.GetVideosAsync(pls.GetValueOrDefault());
            foreach (var item in plsItems)
            {
                try
                {
                    var itm = new YoutubePlaylistItem(item.Url, PlaylistItems);
                    itm.Title = item.Title;
                    itm.Artist = item.Author.Title;
                    itm.Duration = item.Duration.GetValueOrDefault();
                    var covData = await Consts.HttpClient.GetByteArrayAsync(item.Thumbnails.GetWithHighestResolution().Url);
                    itm.CoverImage = new Bitmap(new MemoryStream(covData) { Position = 0 });
                    itm.IsLoaded = true;
                    PlaylistItems.Add(itm);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        InputLink = "";
    }

    public async Task BrowseLocalAsync(Window parent)
    {
        var filter = new[] {"mp3", "m4a", "flac", "ogg", ".aac", ".wma", ".alac", ".wav", ".mp4"};
        var openFile = new OpenFileDialog()
        {
            Title = "Select Local Songs to Add",
            AllowMultiple = true,
            Directory = "C:\\",
            Filters = new()
            {
                new()
                {
                    Extensions = filter.ToList(),
                    Name = "AS2 Supported Files"
                }
            }
        };
        var results = await openFile.ShowAsync(parent);
        if (results == null || results.Length == 0)
            return;
        foreach (var song in results)
        {
            PlaylistItems.Add(new LocalPlaylistItem(song, PlaylistItems));
        }
    }

    public void RemoveAll()
        => PlaylistItems.Clear();

    public void NewPlaylist()
    {
        PlaylistName = "<New Playlist>";
        PlaylistPath = "";
    }

    public async Task OpenPlaylistAsync(Window parent)
    {
        var openFile = new OpenFileDialog()
        {
            AllowMultiple = false,
            Title = "Open Playlist",
            Directory = "C:\\",
            Filters = new()
            {
                new()
                {
                    Extensions = new()
                    {
                        "m3u",
                        "m3u8"
                    },
                    Name = "M3U Playlist"
                }
            }
        };
        var result = await openFile.ShowAsync(parent);
        if (result == null || result?.Count() == 0 || string.IsNullOrWhiteSpace(result?[0]) || !File.Exists(result[0]))
            return;
        PlaylistPath = result[0];
        PlaylistName = result[0].Split('\\').Last();
        var plsTxt = await File.ReadAllTextAsync(result[0]);
        var content = new M3uContent();
        var pls = content.GetFromString(plsTxt);
        if (pls == null)
            return;

        RemoveAll();
        var reg =
            @"(?i)(?:youtube\.com\/\S*(?:(?:\/e(?:mbed))?\/|watch\?(?:\S*?&?v\=))|youtu\.be\/)([a-zA-Z0-9_-]{6,11})(?-i)";
        foreach (var entry in pls.PlaylistEntries)
        {
            var match = Regex.Match(entry.Path, reg);
            if (match.Success)
            {
                var itm = new YoutubePlaylistItem(entry.Path, PlaylistItems);
                _ = Task.Run(itm.LoadInfoAsync);
                PlaylistItems.Add(itm);
            }
            else if (File.Exists(entry.Path.Replace("file:///", "").Replace('/', '\\')))
            {
                try
                {
                    var itm = new LocalPlaylistItem(entry.Path.Replace("file:///", "").Replace('/', '\\'), PlaylistItems);
                    PlaylistItems.Add(itm);
                }
                catch (Exception e)
                {
                    PlaylistItems.Add(new DummyBasePlaylistItem(entry.Path, PlaylistItems));
                }
            }
            else
            {
                PlaylistItems.Add(new DummyBasePlaylistItem(entry.Path, PlaylistItems));
            }
        }
    }

    public async Task SavePlaylistAsync()
    {
        if (string.IsNullOrWhiteSpace(PlaylistPath))
        {
            var wnd = ((ClassicDesktopStyleApplicationLifetime) Application.Current!.ApplicationLifetime!).MainWindow;
            if (wnd == null)
                return; // wat
            await SavePlaylistAsAsync(wnd);
            return;
        }

        var content = new M3uContent();
        var pl = new M3uPlaylist();
        foreach (var item in PlaylistItems)
        {
            pl.PlaylistEntries.Add(new()
            {
                Path = item.Path
            });
        }

        await File.WriteAllTextAsync(PlaylistPath, content.ToText(pl));
    }

    public async Task SavePlaylistAsAsync(Window parent)
    {
        var openFile = new SaveFileDialog()
        {
            Title = "Save Playlist",
            InitialFileName = "Untitled.m3u",
            Directory = "C:\\",
            Filters = new()
            {
                new()
                {
                    Extensions = new()
                    {
                        "m3u",
                        "m3u8"
                    },
                    Name = "M3U Playlist"
                }
            }
        };
        var result = await openFile.ShowAsync(parent);
        if (result == null || string.IsNullOrWhiteSpace(result))
            return;
        PlaylistPath = result;
        PlaylistName = result.Split('\\').Last();
        await SavePlaylistAsync();
    }
}