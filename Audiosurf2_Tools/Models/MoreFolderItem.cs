using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class MoreFolderItem : ReactiveObject
{
    [Reactive] public string Name { get; set; }
    [Reactive] public string Path { get; set; }
    [Reactive] public int Position { get; set; }

    public MoreFolderItem()
    {
    }

    public MoreFolderItem(string name, string path, int position = -1)
    {
        Name = name;
        Path = path;
        Position = position;
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