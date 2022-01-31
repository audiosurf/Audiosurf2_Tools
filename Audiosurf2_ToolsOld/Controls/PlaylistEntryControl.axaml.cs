using Audiosurf2_Tools.Models;
using Audiosurf2_Tools.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Audiosurf2_Tools.Controls
{
    public partial class PlaylistEntryControl : UserControl
    {
        public PlaylistEntryControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<IPlaylistItem> PlaylistItemProperty =
            AvaloniaProperty.Register<MoreFolderItemControl, IPlaylistItem>(nameof(PlaylistItem));

        public IPlaylistItem PlaylistItem
        {
            get => GetValue(PlaylistItemProperty);
            set => SetValue(PlaylistItemProperty, value);
        }

        public static readonly StyledProperty<PlaylistEditorViewModel> ParentVMProperty =
            AvaloniaProperty.Register<MoreFolderItemControl, PlaylistEditorViewModel>(nameof(ParentVM));

        public PlaylistEditorViewModel ParentVM
        {
            get => GetValue(ParentVMProperty);
            set => SetValue(ParentVMProperty, value);
        }

        public void MoveUp() => ParentVM.MoveUp(PlaylistItem);
        public void MoveDown() => ParentVM.MoveDown(PlaylistItem);
        public void RemoveItem() => ParentVM.RemoveItem(PlaylistItem);
    }
}
