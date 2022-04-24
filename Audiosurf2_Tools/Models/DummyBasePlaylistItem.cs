using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class DummyBasePlaylistItem : BasePlaylistItem
{
    public DummyBasePlaylistItem(string path, Collection<BasePlaylistItem> parent) : base(path, parent)
    {
        Path = path;
        IsLoaded = true;
#if LINUX
        Title = Path.Split('/').Last();
#else
        Title = Path.Split('\\').Last();
#endif
        Artist = "n/a";
    }
}