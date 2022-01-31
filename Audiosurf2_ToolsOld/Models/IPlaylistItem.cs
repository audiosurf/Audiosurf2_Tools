using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Audiosurf2_Tools.Models;

public interface IPlaylistItem
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public TimeSpan Duration { get; set; }
    public string Location { get; set; }
    public Bitmap CoverImage { get; set; }
    bool Loaded { get; set; }

    public Task<bool> LoadInfoAsync();
}