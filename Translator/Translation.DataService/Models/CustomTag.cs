using Newtonsoft.Json;
using SQLite;

namespace Translation.DataService.Models
{
    public class CustomTag : BaseModel
    {
        [JsonIgnore]
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [JsonProperty("tagName")]
        public string TagName { get; set; }
    }
}
