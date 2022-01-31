using System.Threading.Tasks;
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

    public async Task Test()
    {
        await TwitchBotSetupViewModel.TwitchAuthUtil.DoOAuthFlowAsync("https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=ff9dg7h1dibw47gvj9y2y5brqo0edt&redirect_uri=http%3A%2F%2Flocalhost%3A8888&scope=chat:read%20chat:edit");
    }
}