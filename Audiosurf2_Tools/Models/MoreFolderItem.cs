using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class MoreFolderItem : ReactiveObject
{
    public delegate void SomethingChangedEventHandler();
    public event SomethingChangedEventHandler SomethingChangedEvent;
    public Collection<MoreFolderItem> Parent { get; set; }
    [Reactive] public string Name { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public int Position { get; set; }
    [Reactive] public bool IsEditing { get; set; }

    public MoreFolderItem()
    {
    }

    public MoreFolderItem(Collection<MoreFolderItem> parent,string name, string path, int position = -1)
    {
        Parent = parent;
        Name = name;
        Path = path;
        Position = position;
    }

    public void EditToggle()
    {
        IsEditing = !IsEditing;
        SomethingChangedEvent?.Invoke();
    }

    public void MoveUp()
    {
        var index = Parent.IndexOf(this);
        if (index == 0)
            return;
        var temp = Parent[index];
        Remove();
        Parent.Insert(index - 1, temp);
    }

    public void MoveDown()
    {
        var index = Parent.IndexOf(this);
        if (index == Parent.Count - 1)
            return;
        var temp = Parent[index];
        Parent.Insert(index + 2, temp);
        Remove();
    }

    public void Remove()
    {
        Parent.Remove(this);
    }

    public RawMoreFolderItem ConvertToRaw(MoreFolderItem item)
    {
        return new RawMoreFolderItem
        {
            Name = item.Name,
            Path = item.Path,
            Position = item.Position
        };
    }
}

public class RawMoreFolderItem
{
    public string Name { get; set; }
    public string Path { get; set; }
    public int Position { get; set; }

}