using Microsoft.CognitiveServices.Speech;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Translation.Core.Domain;
using Translation.Core.Interfaces;

namespace Translation.Core.Services.TranslationService
{
    public class MicrosoftStandardVoiceSynthesizer : IMicrosoftStandardVoiceSynthesizer
    {
        public event Action<TranslationResult> TranslationSpeechReady;
        private string _apiKey;
        private string _region;

        private SpeechSynthesizer SetupStandardVoiceSynthesizer(string targetLanguageCode)
        {
            var config = SpeechConfig.FromSubscription(_apiKey, _region);
            config.SpeechSynthesisLanguage = targetLanguageCode;
            config.SetProfanity(ProfanityOption.Raw);

            return new SpeechSynthesizer(config, null);
        }

        public async Task<bool> SynthesizeText(string targetLanguageCode, string textToSynthesize, string apiKey, string apiRegion)
        {
            if (!string.IsNullOrEmpty(apiKey)) _apiKey = apiKey;
            if (!string.IsNullOrEmpty(apiRegion)) _region = apiRegion;

            try
            {
                var autoSynthesizer = SetupStandardVoiceSynthesizer(targetLanguageCode);
                var synthesisResult = await autoSynthesizer.SpeakTextAsync(textToSynthesize);

                if (synthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    var audioResult = synthesisResult.AudioData;
                    TranslationSpeechReady?.Invoke(new TranslationResult { AudioResult = audioResult });
                    return true;
                }
                else if (synthesisResult.Reason == ResultReason.Canceled)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Standard Synthesizer Error: {ex.Message}");
            }
            return false;
        }
    }
}