using System.Collections.ObjectModel;
using Audiosurf2_Tools.Models;
using Audiosurf2_Tools.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Controls;

public class TwitchPopOutControl : UserControl
{
    public TwitchPopOutControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<Collection<TwitchRequestItem>> ParentCollectionProperty =
        AvaloniaProperty.Register<TwitchPopOutControl, Collection<TwitchRequestItem>>(nameof(ParentCollection));

    public Collection<TwitchRequestItem> ParentCollection
    {
        get => GetValue(ParentCollectionProperty);
        set => SetValue(ParentCollectionProperty, value);
    }
}