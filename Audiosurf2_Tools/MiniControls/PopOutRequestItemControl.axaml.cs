using Audiosurf2_Tools.Controls;
using Audiosurf2_Tools.Models;
using Audiosurf2_Tools.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.MiniControls;

public class PopOutRequestItemControl : UserControl
{
    public PopOutRequestItemControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<TwitchRequestItem> RequestItemProperty =
        AvaloniaProperty.Register<PopOutRequestItemControl, TwitchRequestItem>(nameof(RequestItem));

    public TwitchRequestItem RequestItem
    {
        get => GetValue(RequestItemProperty);
        set => SetValue(RequestItemProperty, value);
    }
    
    public static readonly StyledProperty<TwitchPopOutControl> ParentControlProperty =
        AvaloniaProperty.Register<PopOutRequestItemControl, TwitchPopOutControl>(nameof(ParentControl));

    public TwitchPopOutControl ParentControl
    {
        get => GetValue(ParentControlProperty);
        set => SetValue(ParentControlProperty, value);
    }
    
    public static readonly StyledProperty<TwitchPopOutViewModel> ParentContextProperty =
        AvaloniaProperty.Register<PopOutRequestItemControl, TwitchPopOutViewModel>(nameof(ParentContext));

    public TwitchPopOutViewModel ParentContext
    {
        get => GetValue(ParentContextProperty);
        set => SetValue(ParentContextProperty, value);
    }
    
}