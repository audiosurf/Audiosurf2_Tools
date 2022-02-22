using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.Controls;

public class SettingsControl : UserControl
{
    public SettingsControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}