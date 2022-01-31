using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.Views
{
    public partial class PlaylistEditorWindow : Window
    {
        public Window PreviousWindow { get; internal init; }

        public PlaylistEditorWindow()
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
    }
}
