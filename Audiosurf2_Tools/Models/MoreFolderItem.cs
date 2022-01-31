using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class MoreFolderItem : ReactiveObject
{
    [Reactive]
    public string Name { get; set; }
    [Reactive]
    public string Path { get; set; }

    public MoreFolderItem(string name, string path)
    {
        Name = name;
        Path = path;
    }
}