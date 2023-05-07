namespace Translation.DataService.Models
{
    public class UserQuestions : BaseModel
    {
        public string LanguageCode { get; set; }
        public string Question { get; set; }
        public int Index { get; set; }
    }
}
