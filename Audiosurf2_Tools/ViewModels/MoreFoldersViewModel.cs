using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Audiosurf2_Tools.Models;
using ReactiveUI.Fody.Helpers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Audiosurf2_Tools.ViewModels;

public class MoreFoldersViewModel : ViewModelBase
{
    [Reactive] public bool IsOpen { get; set; }
    [Reactive] public ObservableCollection<MoreFolderItem> MoreFolders { get; set; }

    public MoreFoldersViewModel()
    {
        MoreFolders = new();
        _ = Task.Run(LoadMoreFolderItemsAsync);
    }

    public void AddMoreFolderItem()
    {
        MoreFolders.Add(new MoreFolderItem(MoreFolders, "", "", -1)
        {
            IsEditing = true
        });
    }

    public async Task LoadMoreFolderItemsAsync()
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
        foreach (var item in obj)
        {
            item.Parent = MoreFolders;
            MoreFolders.Add(item);
        }
    }

    public async Task SaveMoreFoldersAsync()
    {
        var gameDir = await ToolUtils.GetGameDirectoryAsync();
        if (string.IsNullOrWhiteSpace(gameDir))
            return;
        if (!File.Exists(Path.Combine(gameDir, "MoreFolders.json")))
            return;

        var rawMore = MoreFolders.Select(x => x.ConvertToRaw(x));
        var text = JsonSerializer.Serialize(rawMore);
        await File.WriteAllTextAsync(Path.Combine(gameDir, "MoreFolders.json"), text);
    }
}