using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Converters;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;
using ReactiveUI.Fody.Helpers;
using ThemeEditor.Controls.ColorPicker;
using Thickness = Avalonia.Thickness;

namespace Audiosurf2_Tools.ViewModels;

public class TwitchPopOutViewModel : ViewModelBase
{
    [Reactive] public double TitleFontSize { get; set; }
    [Reactive] public double ChannelFontSize { get; set; }
    [Reactive] public double RequesterFontSize { get; set; }
    [Reactive] public IBrush? FontColor { get; set; }
    [Reactive] public IBrush? BackgroundColor { get; set; }

    public TwitchPopOutViewModel()
    {
        var cfg = Globals.TryGetGlobal<PopOutSettings>("PopOutSettings");
        if (cfg == null)
            return;
        TitleFontSize = cfg.TitleFontSize;
        ChannelFontSize = cfg.ChannelFontSize;
        RequesterFontSize = cfg.RequesterFontSize;
        FontColor = SolidColorBrush.Parse(cfg.FontColor);
        BackgroundColor = SolidColorBrush.Parse(cfg.BackgroundColor);
    }

    public void ChangeFontSize(string target)
    {
        var niceTarget = target.Split('F')[0];
        var mainWnd = ((ClassicDesktopStyleApplicationLifetime) Application.Current!.ApplicationLifetime!).MainWindow;
        if (mainWnd == null)
            return;

        var stack = new StackPanel
        {
            Margin = new Thickness(20),
            Orientation = Orientation.Horizontal,
            Spacing = 15
        };
        var textBlock = new TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            MaxWidth = 50,
            Width = 50
        };
        var slider = new Slider
        {
            Minimum = 0.0,
            Maximum = 100.0,
            Width = 300
        };
        slider.Bind(RangeBase.ValueProperty, new Binding(target, BindingMode.TwoWay));
        textBlock.Bind(TextBlock.TextProperty, new Binding(target, BindingMode.TwoWay));
        stack.Children.Add(textBlock);
        stack.Children.Add(slider);
        var wnd = new Window
        {
            Title = "Change Size Of " + niceTarget,
            Content = stack,
            DataContext = this,
            SizeToContent = SizeToContent.WidthAndHeight
        };
        wnd.Show(mainWnd);
    }

    public void ChangeColor(string target)
    {
        var niceTarget = target.Split('F')[0];
        var mainWnd = ((ClassicDesktopStyleApplicationLifetime) Application.Current!.ApplicationLifetime!).MainWindow;
        if (mainWnd == null)
            return;

        var control = new ColorPicker
        {
            Margin = new Thickness(25) 
        };
        var binding = new Binding(target, BindingMode.TwoWay)
        {
            Converter = new ColorToBrushConverter2()
        };
        control.Bind(ColorPicker.ColorProperty, binding);
        var wnd = new Window
        {
            Title = "Change Size Of " + niceTarget,
            Content = control,
            DataContext = this,
            SizeToContent = SizeToContent.WidthAndHeight
        };
        wnd.Show(mainWnd);
    }

    public async Task SavePopUpConfigAsync(Window window)
    {
        var cfg = new PopOutSettings
        {
            TitleFontSize = TitleFontSize,
            ChannelFontSize = ChannelFontSize,
            RequesterFontSize = RequesterFontSize,
            FontColor = FontColor?.ToString()!,
            BackgroundColor = BackgroundColor?.ToString()!,
            Height = window.Height,
            Width = window.Width
        };
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var newCfgText = JsonSerializer.Serialize(cfg);
        await File.WriteAllTextAsync(Path.Combine(appdata, "AS2Tools\\PopOutSettings.json"), newCfgText);
        Globals.GlobalEntites["PopOutSettings"] = cfg;
    }
}