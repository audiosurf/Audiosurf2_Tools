using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Audiosurf2_Tools.MiniControls;

public class BotSetupStepExtra : UserControl
{
    public BotSetupStepExtra()
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
        AvaloniaProperty.Register<BotSetupStepExtra, bool>(nameof(IsDone));

    public bool IsDone
    {
        get => GetValue(IsDoneProperty);
        set => SetValue(IsDoneProperty, value);
    }
    
    public static readonly StyledProperty<string> HelpTextProperty =
        AvaloniaProperty.Register<BotSetupStepExtra, string>(nameof(HelpText));

    public string HelpText
    {
        get => GetValue(HelpTextProperty);
        set => SetValue(HelpTextProperty, value);
    }
    
    public static readonly StyledProperty<string> InputValueProperty =
        AvaloniaProperty.Register<BotSetupStepExtra, string>(nameof(InputValue));

    public string InputValue
    {
        get => GetValue(InputValueProperty);
        set => SetValue(InputValueProperty, value);
    }
    
    public static readonly StyledProperty<string> ResultValueProperty =
        AvaloniaProperty.Register<BotSetupStepExtra, string>(nameof(ResultValue));

    public string ResultValue
    {
        get => GetValue(ResultValueProperty);
        set => SetValue(ResultValueProperty, value);
    }
    
    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<BotSetupStepExtra, bool>(nameof(IsEditing));

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> ConfirmCommandProperty =
        AvaloniaProperty.Register<BotSetupStepExtra, ICommand>(nameof(ConfirmCommand));

    public ICommand ConfirmCommand
    {
        get => GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }
    
    public static readonly StyledProperty<ICommand> ExtraCommandProperty =
        AvaloniaProperty.Register<BotSetupStepExtra, ICommand>(nameof(ExtraCommand));

    public ICommand ExtraCommand
    {
        get => GetValue(ExtraCommandProperty);
        set => SetValue(ExtraCommandProperty, value);
    }

    public void OpenHelpText()
    {
        var wnd = new Window()
        {
            SizeToContent = SizeToContent.Height,
            Width = 300,
            SystemDecorations = SystemDecorations.None,
            Content = new TextBlock()
            {
                Text = HelpText,
                Margin = new Thickness(15),
                MaxWidth = 250,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            },
            Position = this.PointToScreen(this.Bounds.Center)
        };
        wnd.Tapped += (sender, args) => ((Window) sender!).Close(); 
        wnd.LostFocus += (sender, args) => ((Window) sender!).Close();
        wnd.Show();
    }
    
}