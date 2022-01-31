using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Audiosurf2_Tools.Models;
using Microsoft.Win32;
using ReactiveUI.Fody.Helpers;

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
        if (!File.Exists(Path.Combine(gameDir, "MoreFolders.txt")))
            return;

        MoreFolders.Clear();
        var lines = await File.ReadAllLinesAsync(Path.Combine(gameDir, "MoreFolders.txt"));
        var names = lines.Where(x => x.StartsWith("name=")).ToArray();
        var paths = lines.Where(x => x.StartsWith("path=")).ToArray();
        if (names.Length != paths.Length)
            return;
        for (int i = 0; i < names.Length; i++)
        {
            var name = names[i].Substring(5);
            var path = paths[i].Substring(5).Replace('/', '\\');
            MoreFolders.Add(new(name, path));
        }
    }
}