using Newtonsoft.Json;
using SQLite;

namespace Translation.DataService.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    [Table("OrganizationSettings")]
    public class OrganizationSettings : BaseModel
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [JsonProperty("organizationId")]
        public int OrganizationId { get; set; }

        [JsonProperty("translationMinutesLimit")]
        public int? TranslationMinutesLimit { get; set; }

        [JsonProperty("allowExplicitContent")]
        public bool AllowExplicitContent { get; set; } = false;

        [JsonProperty("copyPasteEnabled")]
        public bool CopyPasteEnabled { get; set; } = false;

        [JsonProperty("exportEnabled")]
        public bool ExportEnabled { get; set; } = false;

        [JsonProperty("historyPlaybackEnabled")]
        public bool HistoryPlaybackEnabled { get; set; } = true;

        [JsonProperty("historyAudioPlaybackEnabled")]
        public bool HistoryAudioPlaybackEnabled { get; set; } = true;

        [JsonProperty("autoUpdateDesktopApp")]
        public bool AutoUpdateDesktopApp { get; set; }

        [JsonProperty("autoUpdateIOTApp")]
        public bool AutoUpdateIOTApp { get; set; }

        [JsonProperty("languageId")]
        public int LanguageId { get; set; }

        [JsonProperty("languageCode")]
        public string LanguageCode { get; set; }

        [JsonProperty("enableSessionTags")]
        public bool EnableSessionTags { get; set; }
    }
}
