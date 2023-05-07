using Newtonsoft.Json;
using SQLite;
using System;
using System.Diagnostics;
using Translation.DataService.Helpers;

namespace Translation.DataService.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    [Table("Session")]
    public class Session : BaseModel
    {
        [PrimaryKey, AutoIncrement]
        [JsonIgnore]
        public int ID { get; set; }

        private string _sessionName = "--";
        [JsonProperty("sessionName")]
        public string SessionName
        {
            get { return _sessionName; }
            set
            {
                _sessionName = value;
                OnPropertyChanged();
            }
        }

        private long _startTime;
        [JsonProperty("startTime")]
        public long StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged();
                Created = GetDateCreated(StartTime);
            }
        }

        private long _endTime;
        [JsonProperty("endTime")]
        public long EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("customerId")]
        public int CustomerId { get; set; }

        [JsonProperty("ingramMicroCustomerId")]
        public string IngramMicroCustomerId { get; set; }

        [JsonProperty("organizationId")]
        public int OrganizationId { get; set; }

        [JsonProperty("userId")]
        public int UserId { get; set; }

        private string _sourceLangISO;
        [JsonProperty("sourceLangISO")]
        public string SourceLangISO
        {
            get { return _sourceLangISO; }
            set
            {
                _sourceLangISO = value;
                OnPropertyChanged();
            }
        }

        private string _targetLangIso;
        [JsonProperty("targetLangIso")]
        public string TargetLangIso
        {
            get { return _targetLangIso; }
            set
            {
                _targetLangIso = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("softwareVersion")]
        public string SoftwareVersion { get; set; }

        [JsonProperty("clientType")]
        public string ClientType { get; set; }

        [JsonProperty("licenseKeyUsed")]
        public string LicenseKeyUsed { get; set; }

        [JsonProperty("duration")]
        private string _duration;
        public string Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("customTags")]
        public string CustomTags { get; set; } = string.Empty;

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

        // History specific attributes
        private string _recordDate;
        [JsonIgnore]
        public string RecordDate
        {
            get { return _recordDate; }
            set
            {
                _recordDate = value;
                OnPropertyChanged();
            }
        }

        private string _rawStartTime;
        [JsonIgnore]
        public string RawStartTime
        {
            get { return _rawStartTime; }
            set
            {
                _rawStartTime = value;
                OnPropertyChanged();
            }
        }

        private string _rawEndTime;
        [JsonIgnore]
        public string RawEndTime
        {
            get { return _rawEndTime; }
            set
            {
                _rawEndTime = value;
                OnPropertyChanged();
            }
        }

        private string _sourceLanguage;
        [JsonIgnore]
        public string SourceLanguage
        {
            get { return _sourceLanguage; }
            set
            {
                _sourceLanguage = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(SourceLanguage))
                    DisplaySourceLanguage = SourceLanguage.Substring(0, SourceLanguage.IndexOf(" "));
            }
        }

        private string _targeLanguage;
        [JsonIgnore]
        public string TargeLanguage
        {
            get { return _targeLanguage; }
            set
            {
                _targeLanguage = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(TargeLanguage))
                    DisplayTargetLanguage = TargeLanguage.Substring(0, TargeLanguage.IndexOf(" "));
            }
        }

        private string _sessionDuration;
        [JsonIgnore]
        public string SessionDuration
        {
            get { return _sessionDuration; }
            set
            {
                _sessionDuration = value;
                OnPropertyChanged();
            }
        }

        private string _displayDuration;
        [JsonIgnore]
        public string DisplayDuration
        {
            get { return _displayDuration; }
            set
            {
                _displayDuration = value;
                OnPropertyChanged();
            }
        }

        private string _displayDate;
        [JsonIgnore]
        public string DisplayDate
        {
            get { return _displayDate; }
            set
            {
                _displayDate = value;
                OnPropertyChanged();
            }
        }

        private string _displaySourceLanguage;
        [JsonIgnore]
        public string DisplaySourceLanguage
        {
            get { return _displaySourceLanguage; }
            set
            {
                _displaySourceLanguage = value;
                OnPropertyChanged();
            }
        }

        private string _displayTargetLanguage;
        [JsonIgnore]
        public string DisplayTargetLanguage
        {
            get { return _displayTargetLanguage; }
            set
            {
                _displayTargetLanguage = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("billableSeconds")]
        public double BillableSeconds { get; set; }

        [JsonProperty("sessionNumber")]
        public string SessionNumber { get; set; } = "";

        private DateTime _created;
        [JsonIgnore]
        [Ignore]
        public DateTime Created
        {
            get { return _created; }
            set
            {
                _created = value;
                OnPropertyChanged();
            }
        }

        private DateTime GetDateCreated(long startTime)
        {
            try
            {
                return DateTimeUtility.ReturnDateTimeFromlongSeconds(startTime);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return new DateTime(2020, 0, 0);
        }

        public Session()
        {

        }
    }
}
