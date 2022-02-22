using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.MiniControls;

public partial class PlaylistItemControl : UserControl
{
    public PlaylistItemControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}