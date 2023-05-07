using System;

namespace Translation.Events
{
    public class SessionTagChangedArgs : EventArgs
    {
        public string TagValue { get; set; }
    }

    public delegate void SessionTagChangedEvent(object sender, SessionTagChangedArgs args);
}
