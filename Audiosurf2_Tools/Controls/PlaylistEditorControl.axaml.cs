using Audiosurf2_Tools.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.Controls;

public class PlaylistEditorControl : UserControl
{
    public PlaylistEditorControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sender is not TextBox tx) 
            return;
        
        var vm = (PlaylistEditorViewModel) tx.DataContext!;
        vm.AddYoutubeCommand.Execute(tx.Text);
    }
}