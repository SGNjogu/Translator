using System.Collections.Generic;

namespace Translation.Models
{
    public class SpeechlyUserData
    {
        public bool HasB2CAccount { get; set; } = false;
        public bool HasSpeeclyAccount { get; set; } = false;
        public bool HasValidSpeechlyCoreLicense { get; set; } = false;
        public bool HasValidSpeechlyConferenceLicense { get; set; } = false;
        public bool HasAdminRole { get; set; } = false;
        public bool CanLogIn { get; set; } = false;
        public SpeechlyUser User { get; set; } = null;
        public string ErrorMessage { get; set; } = "";
        public List<string> WarningMessages { get; set; }
    }
}
