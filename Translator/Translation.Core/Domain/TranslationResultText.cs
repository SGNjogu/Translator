using System;

namespace Translation.Core.Domain
{
    public class TranslationResult
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public bool IsPerson1 { get; set; }
        public string LanguageName { get; set; }
        public string SourceLanguageCode { get; set; }
        public string TargetLanguageCode { get; set; }
        public string OriginalText { get; set; }
        public string TranslatedText { get; set; }
        public string DateString { get; set; }
        public string Person { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsCopyPasteEnabled { get; set; }
        public long OffsetInTicks { get; set; }
        public string Sentiment { get; set; }
        public string SentimentEmoji { get; set; }
        public byte[] AudioResult { get; set; }
    }
}
