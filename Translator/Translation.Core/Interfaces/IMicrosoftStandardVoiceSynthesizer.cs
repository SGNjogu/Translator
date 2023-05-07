using System;
using System.Threading.Tasks;

using Translation.Core.Domain;

namespace Translation.Core.Interfaces
{
    public interface IMicrosoftStandardVoiceSynthesizer
    {
        event Action<TranslationResult> TranslationSpeechReady;
        Task<bool> SynthesizeText(string targetLanguageCode, string textToSynthesize, string apiKey, string apiRegion);
    }
}