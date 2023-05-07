using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Core.Events;

namespace Translation.Core.Interfaces
{
    public interface ITranslator
    {
        string OriginalText { get; set; }
        bool Reversed { get; set; }
        Language SourceLanguage { get; }
        Language TargetLanguage { get; }

        event Action<TranslationResult> PartialResultReady;
        event Action<TranslationResult> TranscriptionResultReady;
        event Action<TranslationResult> TranslationSpeechReady;
        event Action<TranslationResult> FinalResultReady;
        event Action<TranslationCancelled> TranslationCancelled;
        event AudioInputDataAvailable InputDataAvailable;

        void WriteToFile(byte[] bytes);

        Task Translate
            (
            Language sourceLanguage,
            Language targetLanguage,
            string audioFilePath,
            bool allowExplicitContent,
            AudioDevice audioDevice,
            string azureKey,
            string azureRegion
            );

        Task AutoDetectTranslate
            (
            Language sourceLanguage,
            Language targetLanguage,
            string audioFilePath,
            bool allowExplicitContent,
            AudioDevice audioDevice,
            string azureKey,
            string azureRegion,
            List<Language> candidateLanguages = null
            );

        Task StopAndResetSpeechRecognizer();
        Task StopAndResetAutoDetectSpeechRecognizer();
        Task SwitchSpeakers();
        void Mute();
        void UnMute();
    }
}