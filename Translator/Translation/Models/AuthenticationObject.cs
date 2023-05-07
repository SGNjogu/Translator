using Translation.Auth;

namespace Translation.Models
{
    public class AuthenticationObject
    {
        public UserContext UserContext { get; set; }
        public SpeechlyUserData SpeechlyUserData { get; set; }
    }
}
