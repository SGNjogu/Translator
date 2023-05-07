using System;

namespace Translation.Core.Events
{
    public class GoogleSpeechToTextResponse : EventArgs
    {
        public string SpeechText { get; set; }
        public long OffsetInTicks { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public delegate void GoogleSpeechToTextOnTextAvailable(object sender, GoogleSpeechToTextResponse googleSpeechToTextEventArgs);
}
