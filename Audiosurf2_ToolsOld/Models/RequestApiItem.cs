using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Audiosurf2_Tools.Models
{
    public class RequestApiResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("song_id")]
        public string SongId { get; set; }

        [JsonProperty("song")]
        public string Song { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("requested_by")]
        public string RequestedBy { get; set; }
    }
}
