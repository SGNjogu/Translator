using Newtonsoft.Json;
using System.Collections.Generic;

namespace Translation.DataService.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class BulkTranscriptions : BaseModel
    {
        [JsonProperty("sessionId")]
        public int SessionId { get; set; }

        [JsonProperty("transcriptions")]
        public List<Transcription> Transcriptions { get; set; }
    }
}
