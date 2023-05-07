namespace Translation.Models
{
    public class UserUsageLimit
    {
        public int UserId { get; set; }
        public int? TranslationMinutes { get; set; }
        public int? StorageBytes { get; set; }
        public int? TranslationTimeframe { get; set; }
        public int? StorageTimeframe { get; set; }
        public int? MaxSessionTime { get; set; }
    }
}
