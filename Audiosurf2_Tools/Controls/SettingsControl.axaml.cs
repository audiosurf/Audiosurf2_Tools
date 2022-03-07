using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

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
    
    public void OpenHelpText()
    {
        var wnd = new Window()
        {
            SizeToContent = SizeToContent.Height,
            Width = 500,
            SystemDecorations = SystemDecorations.None,
            Content = new TextBlock()
            {
                Text = "This is a very experimental feature and you need a specific setup for this!\n\n" +
                       "The location you set here needs to have direct access to the location your viewers " +
                       "upload files to/request files from with the command: \n\"!sr fileName.extension\"\n\n" +
                       "The recommended way would work via a mounted Network Drive, \n" +
                       "a Nextcloud or Seafile mounted via WebDAV would work well.\n" +
                       "Using sync clients like Dropbox might need 2 attempts to request a song.",
                Margin = new Thickness(15),
                MaxWidth = 480,
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