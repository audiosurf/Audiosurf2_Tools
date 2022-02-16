using System;
using System.Runtime.Serialization;

namespace Audiosurf2_Tools.Entities;

public class AS2DBLocalEntry
{
    public int SongId { get; set; }
    public string Path { get; set; }
    public long Duration { get; set; }
    public string Name { get; set; }
    public string Artist { get; set; }
    public long PlayCount { get; set; }
    public long Rating { get; set; }
    public long LastPlayTime { get; set; }
    
    [IgnoreDataMember]
    public DateTimeOffset LastPlayTimeParsed => DateTimeOffset.FromUnixTimeSeconds(LastPlayTime);
}