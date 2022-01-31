using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI;

namespace Audiosurf2_Tools.Models;

public class MoreFolderItem : ReactiveObject
{
    private string _name;
    private string _path;
    private bool _isEditing;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Path
    {
        get => _path;
        set => this.RaiseAndSetIfChanged(ref _path, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => this.RaiseAndSetIfChanged(ref _isEditing, value);
    }

    public ObservableCollection<MoreFolderItem> ParentCollection { get; set; }

    public MoreFolderItem(string name, string path, ObservableCollection<MoreFolderItem> parentCollection, bool isEditing = false)
    {
        _name = name ?? throw new NoNullAllowedException();
        _path = path ?? throw new NoNullAllowedException();
        ParentCollection = parentCollection;
        IsEditing = isEditing;
    }

    public void RemoveThis()
    {
        ParentCollection.Remove(this);
    } 

    
}