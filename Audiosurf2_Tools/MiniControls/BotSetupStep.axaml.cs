using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.MiniControls;

public class BotSetupStep : UserControl
{
    public BotSetupStep()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<BotSetupStep, string>(nameof(Status));

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsDoneProperty =
        AvaloniaProperty.Register<BotSetupStep, bool>(nameof(IsDone));

    public bool IsDone
    {
        get => GetValue(IsDoneProperty);
        set => SetValue(IsDoneProperty, value);
    }
    
    public static readonly StyledProperty<string> HelpTextProperty =
        AvaloniaProperty.Register<BotSetupStep, string>(nameof(HelpText));

    public string HelpText
    {
        get => GetValue(HelpTextProperty);
        set => SetValue(HelpTextProperty, value);
    }
    
    public static readonly StyledProperty<string> InputValueProperty =
        AvaloniaProperty.Register<BotSetupStep, string>(nameof(InputValue));

    public string InputValue
    {
        get => GetValue(InputValueProperty);
        set => SetValue(InputValueProperty, value);
    }
    
    public static readonly StyledProperty<string> ResultValueProperty =
        AvaloniaProperty.Register<BotSetupStep, string>(nameof(ResultValue));

    public string ResultValue
    {
        get => GetValue(ResultValueProperty);
        set => SetValue(ResultValueProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<BotSetupStep, bool>(nameof(IsEditing));

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> ConfirmCommandProperty =
        AvaloniaProperty.Register<BotSetupStep, ICommand>(nameof(ConfirmCommand));

    public ICommand ConfirmCommand
    {
        get => GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public void OpenHelpText()
    {
        var wnd = new Window()
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            Content = new TextBlock()
            {
                Text = HelpText,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        };
        wnd.Show();
    }
    
}