using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Audiosurf2_Tools.Models;
using Avalonia.Media;
using ReactiveUI.Fody.Helpers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Audiosurf2_Tools.ViewModels;

public class MoreFoldersViewModel : ViewModelBase
{
    //BTN Backcolor #33ffffff
    [Reactive] public bool IsOpen { get; set; }
    [Reactive] public ObservableCollection<MoreFolderItem> MoreFolders { get; set; }

    [Reactive] public ISolidColorBrush SaveBtnBackground { get; set; } = SolidColorBrush.Parse("#33ffffff");

    public bool IsInitialized { get; set; } = false;

    public MoreFoldersViewModel()
    {
        MoreFolders = new();
        MoreFolders.CollectionChanged += (sender, args) =>
        {
            if (IsInitialized)
                HighlightSaveButton();
        }; 
    }

    public void HighlightSaveButton()
    {
        SaveBtnBackground = SolidColorBrush.Parse("#226622");
    }

    public void AddMoreFolderItem()
    {
        var itm = new MoreFolderItem(MoreFolders, "", "", -1)
        {
            IsEditing = true
        };
        itm.SomethingChangedEvent += HighlightSaveButton;
        MoreFolders.Add(itm);
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
            item.SomethingChangedEvent += HighlightSaveButton;
            MoreFolders.Add(item);
        }
        SaveBtnBackground = SolidColorBrush.Parse("#33ffffff");
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
        SaveBtnBackground = SolidColorBrush.Parse("#33ffffff");
    }
}