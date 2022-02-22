using System.Threading.Tasks;
using Audiosurf2_Tools.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.MiniControls;

public class MoreFoldersItemControl : UserControl
{
    public MoreFoldersItemControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public async Task ChangePathAsync()
    {
        var openFolder = new OpenFolderDialog()
        {
            Directory = "C:\\", 
            Title = "Select A Location That You Want To Add"
        };
        var wind = ((ClassicDesktopStyleApplicationLifetime) Application.Current!.ApplicationLifetime!).MainWindow;
        if(wind == null)
            return; //wat
        var path = await openFolder.ShowAsync(wind);
        if (string.IsNullOrWhiteSpace(path))
            return;
        ((MoreFolderItem) DataContext!).Path = path;
    }
}