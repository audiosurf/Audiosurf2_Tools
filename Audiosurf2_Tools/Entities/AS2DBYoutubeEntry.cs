namespace Audiosurf2_Tools.Entities;

public class AS2DBYoutubeEntry
{
    public int SongId { get; set; }
    public string Name { get; set; }
    public string Artist { get; set; }
    public long Duration { get; set; }
    public string DownloadUrl { get; set; }
    public string ImageUrl { get; set; }
    public string LinkUrl { get; set; }
    public long PlayCount { get; set; }
    public long Rating { get; set; }
    public int LastPlayTime { get; set; }
}