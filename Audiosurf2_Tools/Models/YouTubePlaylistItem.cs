using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace Audiosurf2_Tools.Models;

public class YouTubePlaylistItem : ReactiveObject, IPlaylistItem
{
    public static YoutubeClient YTClient = new YoutubeClient();
    public static HttpClient HClient = new HttpClient();

    private string _title = "";
    private string _artist = "";
    private TimeSpan _duration;
    private string _location = "";
    private Bitmap _coverImage;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string Artist
    {
        get => _artist;
        set => this.RaiseAndSetIfChanged(ref _artist, value);
    }

    public TimeSpan Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value);
    }

    public string Location
    {
        get => _location;
        set => this.RaiseAndSetIfChanged(ref _location, value);
    }

    public Bitmap CoverImage
    {
        get => _coverImage;
        set => this.RaiseAndSetIfChanged(ref _coverImage, value);
    }

    public bool Loaded { get; set; } = false;

    public YouTubePlaylistItem(string path)
    {
        Location = path;
    }

    public async Task<bool> LoadInfoAsync()
    {
        var vid = await YTClient.Videos.GetAsync(Location);
        Title = vid.Title;
        Artist = vid.Author.Title;
        Duration = vid.Duration!.Value;
        var t = await HClient.GetByteArrayAsync(vid.Thumbnails.OrderBy(t => t.Resolution.Area).FirstOrDefault()!.Url);
        using (var bt = new MemoryStream(t))
        {
            bt.Position = 0;
            CoverImage = new Bitmap(bt);
        }
        Loaded = true;
        return true;
    }
}