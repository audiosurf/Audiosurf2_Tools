using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.ViewModels;

public class TwitchBotViewModel : ViewModelBase
{
    [Reactive] public bool IsOpen { get; set; } = true;

    [Reactive] public TwitchBotSetupViewModel TwitchBotSetupViewModel { get; set; }

    public TwitchBotViewModel()
    {
        TwitchBotSetupViewModel = new();
    }
}