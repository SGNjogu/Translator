using System;

namespace Translation.Core.Events
{
    public class GoogleTextTranslationResponse : EventArgs
    {
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
        public long OffsetInTicks { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public delegate void GoogleTextTranslationOnTextAvailable(object sender, GoogleTextTranslationResponse googleTextTranslationResponse);
}
