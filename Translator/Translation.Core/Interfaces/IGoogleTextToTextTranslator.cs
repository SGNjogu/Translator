using System;
using System.Threading.Tasks;
using Translation.Core.Events;

namespace Translation.Core.Interfaces
{
    public interface IGoogleTextToTextTranslator
    {
        event Action<GoogleTextTranslationResponse> GoogleTextTranslationOnTextAvailable;
        Task Initialize();
        Task TranslateAsync
            (
            string originalText,
            string sourceLanguge,
            string targetLanguage,
            long OffsetInTicks,
            TimeSpan duration
            );
    }
}
