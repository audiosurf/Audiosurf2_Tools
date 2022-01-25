using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Audiosurf2_Tools.Models;
using ReactiveUI;

namespace Audiosurf2_Tools.ViewModels;

public class MoreFoldersViewModel : ViewModelBase
{
    private ObservableCollection<MoreFolderItem> _moreFolders;
    private bool _isInitializing = true;

    public string GamePath { get; init; }

    public ObservableCollection<MoreFolderItem> MoreFolders
    {
        get => _moreFolders;
        set => this.RaiseAndSetIfChanged(ref _moreFolders, value);
    }

    public bool IsInitializing
    {
        get => _isInitializing;
        set => this.RaiseAndSetIfChanged(ref _isInitializing, value);
    }

    public MoreFoldersViewModel()
    {
        _moreFolders = new();
        _ = Task.Run(LoadMoreFoldersAsync);
    }

    public async Task LoadMoreFoldersAsync()
    {
        IsInitializing = true;
        await Task.Delay(1000);
        _moreFolders.Clear();
        var file = await File.ReadAllLinesAsync(Path.Combine(GamePath, "MoreFolders.txt"));
        var names = file.Where(x => x.StartsWith("name")).ToArray();
        var paths = file.Where(x => x.StartsWith("path")).ToArray();
        for (int i = 0; i < names.Length; i++)
        {
            await Task.Delay(200);
            MoreFolders.Add(new MoreFolderItem(names[i].Replace("name=", ""), paths[i].Replace("path=", ""),
                MoreFolders));
        }

        await Task.Delay(1000);
        IsInitializing = false;
    }

    public void AddFolder()
    {
        MoreFolders.Add(new ("", "", MoreFolders, true));
    }

    public async Task SaveMoreFoldersAsync()
    {
        var titles = MoreFolders.Select(x => x.Name).ToArray();
        var paths = MoreFolders.Select(x => x.Path).ToArray();
        File.Delete(Path.Combine(GamePath, "MoreFolders.txt"));
        using (var fw = File.CreateText(Path.Combine(GamePath, "MoreFolders.txt")))
        {
            for (int i = 0; i < titles.Length; i++)
            {
                await fw.WriteLineAsync($"name={titles[i]}");
                await fw.WriteLineAsync($"path={paths[i]}");
            }
        }

        await LoadMoreFoldersAsync();
    }
}