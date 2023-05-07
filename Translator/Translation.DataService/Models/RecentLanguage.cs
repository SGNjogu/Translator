using SQLite;

namespace Translation.DataService.Models
{
    public class RecentLanguage
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string LanguageCode { get; set; }
    }
}
