using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using Audiosurf2_Tools.Models;
using Audiosurf2_Tools.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Audiosurf2_Tools.Controls;

public class TwitchBotControl : UserControl
{
    public TwitchBotControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}