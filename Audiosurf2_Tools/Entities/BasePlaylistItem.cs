using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Entities;

public class BasePlaylistItem : ReactiveObject
{
    private Collection<BasePlaylistItem> parentCollection { get; set; }
    
    [Reactive] public string Title { get; set; }
    [Reactive] public string Artist{ get; set; }
    [Reactive] public TimeSpan Duration { get; set; }
    [Reactive] public Bitmap CoverImage { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public bool IsLoaded { get; set; }

    public BasePlaylistItem() //XAML
    {
    }
    
    public BasePlaylistItem(string path, Collection<BasePlaylistItem> parent)
    {
        Path = path;
        parentCollection = parent;

    }

    public virtual Task LoadInfoAsync()
    {
        return Task.CompletedTask;
    }

    public void MoveUp()
    {
        var index = parentCollection.IndexOf(this);
        if (index == 0)
            return;
        var temp = parentCollection[index];
        RemoveThis();
        parentCollection.Insert(index - 1, temp);
    }

    public void MoveDown()
    {
        var index = parentCollection.IndexOf(this);
        if (index == parentCollection.Count - 1)
            return;
        var temp = parentCollection[index];
        parentCollection.Insert(index + 2, temp);
        RemoveThis();
    }

    public void RemoveThis()
    {
        parentCollection.Remove(this);
    }
}