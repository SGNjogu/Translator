using System;

namespace Translation.Core.Domain
{
    public class TranslationCancelled
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        public string Reason { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorDetails { get; set; }
    }
}
