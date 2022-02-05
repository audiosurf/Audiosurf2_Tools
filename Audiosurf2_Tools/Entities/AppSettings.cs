namespace Audiosurf2_Tools.Entities;

public class AppSettings
{
    public string TwitchCommandPrefix { get; set; } = "!";
    public int TwitchMaxQueueItemsUntilDuplicationsAllowed { get; set; } = 100;
    public int TwitchMaxRecentAgeBeforeDuplicateError { get; set; } = 5;
    public int TwitchMaxQueueSize { get; set; } = 25;
    public int TwitchRequestCoolDown { get; set; } = 30;
    public int TwitchMaxSongLengthSeconds { get; set; } = 600;
}