using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.ViewModels;

public class PlaylistEditorViewModel : ViewModelBase
{
    [Reactive] public bool IsOpen { get; set; } = false;
}