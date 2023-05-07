using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Core.Events;
using Translation.Core.Interfaces;
using Translation.Core.Utils;

namespace Translation.Core.Services.TranslationService
{
    public class MicrosoftTextToSpeechProvider : IMicrosoftTextToSpeechProvider
    {
        private string _apiKey;
        private string _region;

        private Guid Guid { get; set; }

        public event Action<TranslationResult> TranscriptionResultReady;
        public event Action<TranslationResult> TranslationSpeechReady;

        private readonly IMicrosoftTextToTextTranslator _microsoftTextToTextTranslator;
        private readonly IMicrosoftStandardVoiceSynthesizer _microsoftStandardVoiceSynthesizer;

        public MicrosoftTextToSpeechProvider(IMicrosoftTextToTextTranslator microsoftTextToTextTranslator, IMicrosoftStandardVoiceSynthesizer microsoftStandardVoiceSynthesizer)
        {
            _microsoftTextToTextTranslator = microsoftTextToTextTranslator;
            _microsoftStandardVoiceSynthesizer = microsoftStandardVoiceSynthesizer;
        }

        public async Task Translate(string apiKey, string apiRegion, Language sourceLanguage, string textToTranslate, Language targetLanguage)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _region = apiRegion ?? throw new ArgumentNullException(nameof(apiRegion));

            if (sourceLanguage == null)
                throw new ArgumentNullException(nameof(sourceLanguage));
            if (targetLanguage == null)
                throw new ArgumentNullException(nameof(targetLanguage));

            //Text to Text
            var translatedText = await _microsoftTextToTextTranslator.TranslateTextToText(apiKey, apiRegion, sourceLanguage.Code, textToTranslate, targetLanguage.Code);
            var result = new TranslationResult
            {
                Guid = Guid.NewGuid(),
                OriginalText = textToTranslate,
                SourceLanguageCode = sourceLanguage.Code,
                TargetLanguageCode = targetLanguage.Code,
                TranslatedText = translatedText,
                OffsetInTicks = RandomLong()
            };

            //Synthesize Text to Speech

            if (targetLanguage.UseNeuralVoice)
            {
                try
                {
                    var synthesizer = SetupNeuralVoiceSynthesizer(targetLanguage.Code, targetLanguage.VoiceName);
                    synthesizer.OnAudioAvailable += Synthesizer_OnAudioAvailable;
                    synthesizer.OnError += Synthesizer_OnError;
                    await synthesizer.Synthesize(CancellationToken.None, result);
                }
                catch (Exception ex)
                {
                    //TODO log exception
                    Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                _microsoftStandardVoiceSynthesizer.TranslationSpeechReady += _microsoftStandardVoiceSynthesizer_TranslationSpeechReady;
                await _microsoftStandardVoiceSynthesizer.SynthesizeText(targetLanguage.Code, result.TranslatedText, _apiKey, apiRegion);
            }
        }

        private void _microsoftStandardVoiceSynthesizer_TranslationSpeechReady(TranslationResult result)
        {
            TranscriptionResultReady?.Invoke(result);
            TranslationSpeechReady?.Invoke(result);
        }

        private void Synthesizer_OnAudioAvailable(object sender, SynthesizerEventArgs<TranslationResult> e)
        {
            var result = e.EventData;
            TranscriptionResultReady?.Invoke(result);
            TranslationSpeechReady?.Invoke(result);
        }

        private MicrosoftNeuralVoiceSynthesizer SetupNeuralVoiceSynthesizer(string targetLanguageCode, string voiceName)
        {
            // TODO: Remove this when allocation of region specific endpoints is implemented
            // Make sure the region supports neural voice
            string tokenUrl = $"https://{_region}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
            string endpointUri = $"https://{_region}.tts.speech.microsoft.com/cognitiveservices/v1";

            var auth = new CognitiveServiceAuthentication(tokenUrl, _apiKey);

            var accessToken = auth.GetAccessToken();

            SynthesizerInputOptions inputOptions = new SynthesizerInputOptions()
            {
                RequestUri = new Uri(endpointUri),
                Guid = Guid,
                VoiceType = Gender.Female,
                Locale = targetLanguageCode,
                VoiceName = voiceName,
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken
            };

            return new MicrosoftNeuralVoiceSynthesizer(inputOptions);
        }

        private void Synthesizer_OnError(object sender, SynthesizerEventArgs<Exception> e)
        {
            Console.WriteLine($"Neural Synthesizer Error: {e.EventData}");
        }

        private long RandomLong()
        {
            Random random = new Random();
            byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}
