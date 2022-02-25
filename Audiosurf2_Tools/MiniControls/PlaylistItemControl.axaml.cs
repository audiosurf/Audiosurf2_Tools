using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.MiniControls;

[PseudoClasses(":IsRemoving")]
public partial class PlaylistItemControl : UserControl
{
    public PlaylistItemControl()
    {
        InitializeComponent();
        UpdatePseudoClasses(IsRemoving);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<BasePlaylistItem> PlaylistItemProperty =
        AvaloniaProperty.Register<PlaylistItemControl, BasePlaylistItem>(nameof(PlaylistItem));

    public BasePlaylistItem PlaylistItem
    {
        get => GetValue(PlaylistItemProperty);
        set => SetValue(PlaylistItemProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsRemovingProperty =
        AvaloniaProperty.Register<PlaylistItemControl, bool>(nameof(IsRemoving));

    public bool IsRemoving
    {
        get => GetValue(IsRemovingProperty);
        set => SetValue(IsRemovingProperty, value);
    }
    
    private void UpdatePseudoClasses(bool isRemoving)
    {
        PseudoClasses.Set(":IsRemoving", isRemoving);
    }

    public async Task AnimateRemoveAsync()
    {
        IsRemoving = true;
        UpdatePseudoClasses(IsRemoving);
        PlaylistItem.RemoveThis();
        IsRemoving = false;
        UpdatePseudoClasses(IsRemoving);
    }
}