using Newtonsoft.Json;
using SQLite;
using System;

namespace Translation.DataService.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    [Table("Transcription")]
    public class Transcription : BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [JsonProperty("sessionId")]
        public int SessionId { get; set; }

        [JsonProperty("chatUser")]
        public string ChatUser { get; set; }

        [JsonProperty("originalText")]
        public string OriginalText { get; set; }

        [JsonProperty("translatedText")]
        public string TranslatedText { get; set; }

        [JsonProperty("chatTime")]
        public DateTime ChatTime { get; set; }

        [JsonProperty("sentiment")]
        public string Sentiment { get; set; }

        private bool _syncedToServer;
        [JsonIgnore]
        public bool SyncedToServer
        {
            get { return _syncedToServer; }
            set
            {
                _syncedToServer = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("translationSeconds")]
        public double TranslationSeconds { get; set; }
    }
}
