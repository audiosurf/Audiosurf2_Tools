using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ATL;
using Audiosurf2_Tools.Entities;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class LocalPlaylistItem : BasePlaylistItem
{
    public LocalPlaylistItem(string path, Collection<BasePlaylistItem> parent) : base(path, parent)
    {
        Path = path;
        var info = new Track(path);
        Title = info.Title;
        Artist = info.Artist;
        Duration = TimeSpan.FromMilliseconds(info.DurationMs);
        if (info.EmbeddedPictures.Count != 0)
        {
            var pic = info.EmbeddedPictures.First();
            CoverImage = new Bitmap(new MemoryStream(pic.PictureData){ Position = 0 });
        }

        IsLoaded = true;
    }
}