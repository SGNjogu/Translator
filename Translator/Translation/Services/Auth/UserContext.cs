using Newtonsoft.Json;

namespace Translation.Auth
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class UserContext
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("organizationId")]
        public int OrganizationId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("jobTitle")]
        public string JobTitle { get; set; }

        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("userType")]
        public string UserType { get; set; }

        [JsonProperty("userIntId")]
        public int UserIntID { get; set; }

        public string AccessToken { get; set; }

        public string TokenExpiry { get; set; }

        public string TenantId { get; set; }

        public string Domain { get; set; }

        public string Organization { get; set; }
        public bool DataConsentStatus { get; set; }
        public bool IsLoggedIn { get; set; } = false;
        public string ResellerName { get; set; }
        public string ResellerEmail { get; set; }
    }
}
