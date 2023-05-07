using Newtonsoft.Json;
using System.Collections.Generic;
using Translation.DataService.Models;

namespace Translation.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Organization
    {
        public string Name { get; set; } = "";
        public string Domain { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int? OrganizationContactId { get; set; }
        public int? AddressId { get; set; }
        public int? LicensingTypeId { get; set; }
        public int? BillingTypeId { get; set; }
        public bool LockToJabraDevices { get; set; }
        public bool DataConsentStatus { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
