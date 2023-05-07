using System.Collections.Generic;
using Translation.Models;

namespace Translation.Messages
{
    public class SessionMetaDataMessage
    {
        public string SessionName { get; set; }
        public List<SessionTag> SessionTags { get; set; }
        public List<string> CustomTags { get; set; }
    }
}
