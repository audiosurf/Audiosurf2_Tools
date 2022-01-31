using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.Views
{
    public partial class TwitchBotWindow : Window
    {
        public Window PreviousWindow { get; set; }
        public TwitchBotWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override bool HandleClosing()
        {
            PreviousWindow.Show();
            return base.HandleClosing();
        }

        private void InputElement_OnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("a");
            var original = sender as ListBox;
            var listB = new ListBox()
            {
                Padding = new Thickness(10),
                DataContext = DataContext,
                ItemTemplate = original!.ItemTemplate,
                Items = original!.Items
            };
            var newW = new Window
            {
                Content = listB,
                Title = "Requests PopOut"
            };
            newW.Show();
        }
    }
}
