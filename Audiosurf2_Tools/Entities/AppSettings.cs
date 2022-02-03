namespace Audiosurf2_Tools.Entities;

public class AppSettings
{
    public char TwitchCommandPrefix { get; set; } = '!';
    public int TwitchMaxSongsBeforeDuplicateError { get; set; } = 5;
    public int TwitchMaxRecentAgeBeforeDuplicateError { get; set; } = 5;
    public int TwitchMaxQueueSize { get; set; } = 25;
}