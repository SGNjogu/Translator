using System;
using System.Threading.Tasks;
using Translation.Core.Domain;

namespace Translation.Core.Interfaces
{
    public interface IMicrosoftTextToSpeechProvider
    {
        event Action<TranslationResult> TranscriptionResultReady;
        event Action<TranslationResult> TranslationSpeechReady;

        Task Translate(string apiKey, string apiRegion, Language sourceLanguage, string textToTranslate, Language targetLanguage);
    }
}