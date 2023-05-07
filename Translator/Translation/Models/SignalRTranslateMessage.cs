namespace Translation.Models
{
    public class SignalRTranslateMessage
    {
        public string ConnectionId { get; set; } = string.Empty;
        public bool CanTranslate { get; set; }
        public string UserEmail { get; set; } = string.Empty;
    }
}
