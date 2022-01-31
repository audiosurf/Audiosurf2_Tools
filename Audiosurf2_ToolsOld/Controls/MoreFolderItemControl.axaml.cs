using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Audiosurf2_Tools.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.Controls;

public class MoreFolderItemControl : UserControl
{
    public MoreFolderItemControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<Window> ParentWindowProperty =
        AvaloniaProperty.Register<MoreFolderItemControl, Window>(nameof(ParentWindow));

    public Window ParentWindow
    {
        get => GetValue(ParentWindowProperty);
        set => SetValue(ParentWindowProperty, value);
    }

    public static readonly StyledProperty<MoreFolderItem> FolderItemProperty =
        AvaloniaProperty.Register<MoreFolderItemControl, MoreFolderItem>(nameof(FolderItem));

    public MoreFolderItem FolderItem
    {
        get => GetValue(FolderItemProperty);
        set => SetValue(FolderItemProperty, value);
    }

    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<MoreFolderItemControl, bool>(nameof(IsEditing), false);

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public void RemoveFolder()
    {
        FolderItem.RemoveThis();
    }

    public void EditFolder()
    {
        IsEditing = true;
    }

    public async Task BrowseForPathAsync()
    {
        var folderD = new OpenFolderDialog
        {
            Title = "Select the folder you'd like to add",
            Directory = "C:\\"
        };
        var path = await folderD.ShowAsync(ParentWindow);
        if (Directory.Exists(path))
        {
            FolderItem.Path = path;
        }
    }

    public void SaveItem()
    {
        IsEditing = false;
    }

    public void MoveUp()
    {
        var currentIndex = FolderItem.ParentCollection.IndexOf(FolderItem);
        if (currentIndex == 0)
            return;
        FolderItem.ParentCollection.Insert(currentIndex - 1, FolderItem);
        FolderItem.ParentCollection.RemoveAt(currentIndex + 1);

    }

    public void MoveDown()
    {
        var currentIndex = FolderItem.ParentCollection.IndexOf(FolderItem);
        if (currentIndex == (FolderItem.ParentCollection.Count - 1))
            return;
        FolderItem.ParentCollection.Insert(currentIndex + 2, FolderItem);
        FolderItem.ParentCollection.RemoveAt(currentIndex);
    }
}