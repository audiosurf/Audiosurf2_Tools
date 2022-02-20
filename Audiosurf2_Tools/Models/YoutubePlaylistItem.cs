using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;

namespace Audiosurf2_Tools.Models;

public class YoutubePlaylistItem : BasePlaylistItem
{
    public YoutubePlaylistItem(string url, Collection<BasePlaylistItem> parent) : base(url, parent)
    {
        Path = url;
    }

    public override async Task LoadInfoAsync()
    {
        var vid = VideoId.TryParse(Path);
        if (vid == null)
            return;
        var video = await Consts.YoutubeClient.Videos.GetAsync(vid.Value);
        Title = video.Title;
        Artist = video.Author.Title;
        Duration = video.Duration.GetValueOrDefault();
        Path = video.Url;
        _ = Task.Run(() => FillImageAsync(video.Thumbnails.MinBy(x => x.Resolution.Area)!.Url));
        IsLoaded = true;
    }

    private async Task FillImageAsync(string url)
    {
        var coverStream = await Consts.HttpClient.GetByteArrayAsync(url);
        Dispatcher.UIThread.Post(() =>
        {
            CoverImage = new Bitmap(new MemoryStream(coverStream) { Position = 0 });
        });
    }
}