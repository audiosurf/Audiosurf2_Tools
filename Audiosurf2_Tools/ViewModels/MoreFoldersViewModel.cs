using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Audiosurf2_Tools.Models;
using ReactiveUI.Fody.Helpers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Audiosurf2_Tools.ViewModels;

public class MoreFoldersViewModel : ViewModelBase
{
    [Reactive] public bool IsOpen { get; set; } = false;
    [Reactive] public ObservableCollection<MoreFolderItem> MoreFolders { get; set; }

    public MoreFoldersViewModel()
    {
        MoreFolders = new();
        _ = Task.Run(_loadMoreFolderItems);
    }

    private async Task _loadMoreFolderItems()
    {
        var gameDir = await ToolUtils.GetGameDirectoryAsync();
        if (string.IsNullOrWhiteSpace(gameDir))
            return;
        if (!File.Exists(Path.Combine(gameDir, "MoreFolders.json")))
            return;

        MoreFolders.Clear();
        var lines = await File.ReadAllTextAsync(Path.Combine(gameDir, "MoreFolders.json"));
        var obj = JsonSerializer.Deserialize<List<MoreFolderItem>>(lines);
        if (obj == null)
            return;
        MoreFolders = new (obj);
    }
}