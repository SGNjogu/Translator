namespace Translation.DataService.Models
{
    public class UsageLimit
    {
        public int? UserTranslationMinutes { get; set; }
        public int? UserStorageBytes { get; set; }
        public int? UserTranslationTimeframe { get; set; }
        public int? UserStorageTimeframe { get; set; }
        public int? UserMaxSessionTime { get; set; }
        public string OrganizationLicensingType { get; set; }
        public string OrganizationBillingType { get; set; }
        public bool OrganizationTranslationLimitExceeded { get; set; } = false;
        public bool OrganizationStorageLimitExceeded { get; set; } = false;
    }
}
