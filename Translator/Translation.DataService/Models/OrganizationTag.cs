using Newtonsoft.Json;
using SQLite;

namespace Translation.DataService.Models
{
    public class OrganizationTag
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [JsonProperty("id")]
        public int OrganizationTagId { get; set; }
        public int OrganizationId { get; set; }
        public string TagName { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsShownOnApp { get; set; }
    }
}
