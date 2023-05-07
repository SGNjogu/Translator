using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Core.Interfaces;

namespace Translation.Core.Services.TranslationService
{
    public partial class MicrosoftTranslationProvider : ITranslator
    {
        public SpeechTranslationConfig AutoDetectTranslationConfig { get; private set; }
        public TranslationRecognizer AutoDetectRecognizer { get; private set; }
        private List<Language> _candidateLanguages;

        public async Task AutoDetectTranslate
            (
            Language sourceLanguage,
            Language targetLanguage,
            string audioFilePath,
            bool allowExplicitContent,
            AudioDevice audioDevice,
            string azureKey,
            string azureRegion,
            List<Language> candidateLanguages = null
            )
        {
            try
            {
                Reversed = false;
                IsMute = false;
                SourceLanguage = sourceLanguage ?? throw new ArgumentNullException($"Source Language is null");
                if (candidateLanguages == null)
                    throw new ArgumentNullException($"Candidate Languages is null");
                else
                    _candidateLanguages = candidateLanguages;
                _audioFilePath = audioFilePath ?? throw new ArgumentNullException($"FilePath is null");
                AzureKey = azureKey ?? throw new ArgumentNullException($"Azure Key is null");
                AzureRegion = azureRegion ?? throw new ArgumentNullException($"Azure Region is null");
                _audioInputQueue = new ConcurrentQueue<AudioDataInput>();
                InitializeAutoDetectDevice(allowExplicitContent);
                await AutoDetectRecognizer.StartContinuousRecognitionAsync();
                _audioRecorder.DataAvailable += Recorder_OnDataAvailable;
                await _audioRecorder.StartRecording(audioDevice.InputDevice);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void InitializeAutoDetectDevice(bool allowExplicitContent)
        {
            CreateAudioStream();
            InitializeAutoDetectRecognizer(allowExplicitContent);
        }

        private void InitializeAutoDetectRecognizer(bool allowExplicitContent)
        {
            if (allowExplicitContent)
                ProfanityOption = ProfanityOption.Raw;
            else
                ProfanityOption = ProfanityOption.Masked;

            // Currently the v2 endpoint is required. In a future SDK release you won't need to set it.
            var endpointString = $"wss://{AzureRegion}.stt.speech.microsoft.com/speech/universal/v2";
            var endpointUrl = new Uri(endpointString);
            AutoDetectTranslationConfig = SpeechTranslationConfig.FromEndpoint(endpointUrl, AzureKey);

            // Source language is required, but currently ignored. 
            if (string.IsNullOrEmpty(SourceLanguage.Code))
                throw new NullReferenceException($"Target Language code is null: {SourceLanguage}");

            if (_candidateLanguages == null)
                throw new ArgumentNullException($"Candidate Languages is null");

            AutoDetectTranslationConfig.SpeechRecognitionLanguage = SourceLanguage.Code;
            AutoDetectTranslationConfig.AddTargetLanguage(SourceLanguage.Code);

            List<string> languageCodes = new List<string>();

            languageCodes.Add(SourceLanguage.Code);

            foreach (var language in _candidateLanguages)
            {
                AutoDetectTranslationConfig.AddTargetLanguage(language.Code);
                languageCodes.Add(language.Code);
            }

            AutoDetectTranslationConfig.SetProfanity(ProfanityOption);

            AutoDetectTranslationConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
            AutoDetectTranslationConfig.RequestWordLevelTimestamps();
            AutoDetectTranslationConfig.SetProperty(PropertyId.SpeechServiceConnection_LanguageIdMode, "Continuous");
            AutoDetectSourceLanguageConfig autoDetectSourceLanguageConfig = AutoDetectSourceLanguageConfig.FromLanguages(languageCodes.ToArray());
            //var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT);
            //AudioConfig audioConfig = AudioConfig.FromStreamInput(_forwardAudioInputStream, audioProcessingOptions);
            AudioConfig audioConfig = AudioConfig.FromStreamInput(_forwardAudioInputStream);
            AutoDetectRecognizer = new TranslationRecognizer(AutoDetectTranslationConfig, autoDetectSourceLanguageConfig, audioConfig);

            AutoDetectRecognizer.Recognizing += OnAutoDetectRecognizing;
            AutoDetectRecognizer.Recognized += OnAutoDetectRecognized;
            AutoDetectRecognizer.Canceled += OnAutoDetectCanceled;
        }

        private async void OnAutoDetectRecognized(object sender, TranslationRecognitionEventArgs e)
        {
            string sourceLanguageCode = string.Empty;
            string targetLanguageCode = string.Empty;

            if (e.Result.Reason == ResultReason.TranslatedSpeech)
            {
                var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);

                sourceLanguageCode = lidResult;
            }

            if (e.Result.Text.Length > 0)
            {
                var result = new TranslationResult
                {
                    Guid = Guid,
                    OriginalText = e.Result.Text,
                    SourceLanguageCode = sourceLanguageCode,
                    Duration = e.Result.Duration,
                    OffsetInTicks = e.Result.OffsetInTicks
                };

                if (e.Result.Translations == null || e.Result.Translations.Count == 0)
                {
                    result.TranslatedText = e.Result.Text;
                    result.TargetLanguageCode = sourceLanguageCode;
                }
                else
                {
                    foreach (var t in e.Result.Translations)
                    {
                        result.TranslatedText = t.Value;
                        result.TargetLanguageCode = t.Key;
                    }
                }

                Language targetLanguage = null;

                if (SourceLanguage.Code == result.TargetLanguageCode)
                {
                    result.IsPerson1 = false;
                    targetLanguage = SourceLanguage;
                }
                else
                {
                    result.IsPerson1 = true;
                    targetLanguage = _candidateLanguages.FirstOrDefault(s => s.Code == result.TargetLanguageCode);
                }

                TranscriptionResultReady?.Invoke(result);

                try
                {
                    if (SourceLanguage.UseNeuralVoice && targetLanguage != null && targetLanguage.UseNeuralVoice)
                    {
                        AutoDetectNeuralSynthesizer = SetupNeuralVoiceSynthesizer(targetLanguage.Code, targetLanguage.VoiceName);
                        AutoDetectNeuralSynthesizer.OnAudioAvailable += Synthesizer_OnAudioAvailable;
                        AutoDetectNeuralSynthesizer.OnError += Synthesizer_OnError;

                        await AutoDetectNeuralSynthesizer.Synthesize(CancellationToken.None, result);
                    }
                    else
                    {
                        try
                        {
                            var autoSynthesizer = SetupStandardVoiceSynthesizer(result.TargetLanguageCode);
                            var synthesisResult = await autoSynthesizer.SpeakTextAsync(result.TranslatedText);

                            if (synthesisResult.Reason == ResultReason.SynthesizingAudioCompleted)
                            {
                                var audioResult = new TranslationResult
                                {
                                    Guid = Guid,
                                    SourceLanguageCode = sourceLanguageCode,
                                    TargetLanguageCode = result.TargetLanguageCode,
                                    AudioResult = synthesisResult.AudioData
                                };

                                TranslationSpeechReady?.Invoke(audioResult);
                                FinalResultReady?.Invoke(audioResult);
                            }
                            else if (synthesisResult.Reason == ResultReason.Canceled)
                            {
                                Debug.WriteLine($"Standard Synthesizer Error: {synthesisResult.Reason}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Standard Synthesizer Error: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO log exception
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void OnAutoDetectRecognizing(object sender, TranslationRecognitionEventArgs e)
        {
            var lidResult = e.Result.Properties.GetProperty(PropertyId.SpeechServiceConnection_AutoDetectSourceLanguageResult);
            var sourceLanguageCode = lidResult;

            if (e.Result.Reason == ResultReason.TranslatingSpeech)
            {
                var partialResult = new TranslationResult
                {
                    Guid = Guid,
                    OriginalText = e.Result.Text,
                    SourceLanguageCode = sourceLanguageCode,
                    Duration = e.Result.Duration,
                    OffsetInTicks = e.Result.OffsetInTicks
                };

                if (e.Result.Translations == null || e.Result.Translations.Count == 0)
                {
                    partialResult.TranslatedText = e.Result.Text;
                    partialResult.TargetLanguageCode = sourceLanguageCode;
                }
                else
                {
                    foreach (var t in e.Result.Translations)
                    {
                        partialResult.TranslatedText = t.Value;
                        partialResult.TargetLanguageCode = t.Key;
                    }
                }

                if (SourceLanguage.Code == sourceLanguageCode)
                {
                    partialResult.IsPerson1 = true;
                }
                else
                {
                    partialResult.IsPerson1 = false;
                }

                PartialResultReady?.Invoke(partialResult);
            }
        }

        private void OnAutoDetectCanceled(object sender, TranslationRecognitionCanceledEventArgs e)
        {
            var translationCancelled = new TranslationCancelled
            {
                Reason = e.Reason.ToString(),
                ErrorCode = e.ErrorCode.ToString(),
                ErrorDetails = e.ErrorDetails
            };
            TranslationCancelled?.Invoke(translationCancelled);
        }

        public async Task StopAndResetAutoDetectSpeechRecognizer()
        {
            try
            {
                if (_audioFileSaver != null)
                    await _audioFileSaver.SaveFile();

                if (_audioRecorder != null)
                    _audioRecorder.DataAvailable -= Recorder_OnDataAvailable;

                if (AutoDetectRecognizer != null)
                {
                    AutoDetectRecognizer.Recognizing -= OnAutoDetectRecognizing;
                    AutoDetectRecognizer.Recognized -= OnAutoDetectRecognized;
                    AutoDetectRecognizer.Canceled -= OnAutoDetectCanceled;
                    AutoDetectNeuralSynthesizer.OnAudioAvailable -= Synthesizer_OnAudioAvailable;
                }

                if (_audioRecorder != null)
                    _audioRecorder.StopRecording();

                // Close and dispose both streams

                if (_forwardAudioInputStream != null)
                {
                    _forwardAudioInputStream.Close();
                    _forwardAudioInputStream.Dispose();
                    _forwardAudioInputStream = null;
                }

                if (_reverseAudioInputStream != null)
                {
                    _reverseAudioInputStream.Close();
                    _reverseAudioInputStream.Dispose();
                    _reverseAudioInputStream = null;
                }

                // Stop continous recognition for auto-detect recognizer
                if (AutoDetectRecognizer != null)
                {
                    await AutoDetectRecognizer.StopContinuousRecognitionAsync();
                }

                // Reset config
                AutoDetectTranslationConfig = null;

                // Reset recognizer
                AutoDetectRecognizer = null;
                IsMute = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private SpeechSynthesizer SetupStandardVoiceSynthesizer(string targetLanguageCode)
        {
            var config = SpeechConfig.FromSubscription(AzureKey, AzureRegion);
            config.SpeechSynthesisLanguage = targetLanguageCode;
            config.SetProfanity(ProfanityOption.Raw);

            return new SpeechSynthesizer(config, null);
        }
    }
}
