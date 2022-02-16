using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Avalonia.Media.Imaging;
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
        var coverStream = await Consts.HttpClient.GetByteArrayAsync(video.Thumbnails.GetWithHighestResolution().Url);
        CoverImage = new Bitmap(new MemoryStream(coverStream) { Position = 0 });
        Path = video.Url;
        IsLoaded = true;
    }
}