using Newtonsoft.Json;
using SQLite;

namespace Translation.DataService.Models
{
    public class PlaybackUsage : BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("sessionNumber")]
        public string SessionNumber { get; set; } = "";

        [JsonProperty("playbackSeconds")]
        public double PlaybackSeconds { get; set; }

        [JsonIgnore]
        public bool SyncedToServer { get; set; } = false;
    }
}
