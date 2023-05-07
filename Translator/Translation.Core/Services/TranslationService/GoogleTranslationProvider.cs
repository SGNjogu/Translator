using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Core.Events;
using Translation.Core.Interfaces;
using Translation.Core.Utils;
using Translation.Interface;

namespace Translation.Core.Services.TranslationService
{
    public class GoogleTranslationProvider : ITranslator
    {
        private IGoogleTextToTextTranslator TextToTextClient { get; set; }

        private IGoogleSpeechToTextClient ForwardGoogleSpeechToTextClient { get; set; }

        private IGoogleSpeechToTextClient ReversedGoogleSpeechToTextClient { get; set; }

        private MicrosoftNeuralVoiceSynthesizer ForwardNeuralSynthesizer { get; set; }

        private MicrosoftNeuralVoiceSynthesizer ReverseNeuralSynthesizer { get; set; }

        private InputDevice _inputDevice;

        private ConcurrentQueue<GoogleSpeechToTextResponse> SpeechToTextResponseQueue { get; set; }

        public Language SourceLanguage { get; private set; }

        public Language TargetLanguage { get; private set; }

        public string AzureKey { get; private set; }
        public string AzureRegion { get; private set; }

        public string GoogleJsonCredentials { get; private set; }

        public string OriginalText { get; set; }

        public Guid Guid { get; set; }

        public AudioDataInput _audioDataInput { get; set; }

        public bool IsMute { get; set; }

        // Tracking which TranslationConfig and Recognizer is in use
        public bool Reversed { get; set; }
        private readonly IAudioRecorder _audioRecorder;
        private readonly IAudioFileSaver _audioFileSaver;

        private ConcurrentQueue<AudioDataInput> _audioInputQueue { get; set; }

        private string _audioFilePath;

        // Events
        public event Action<TranslationResult> PartialResultReady;
        public event Action<TranslationResult> TranscriptionResultReady;
        public event Action<TranslationResult> TranslationSpeechReady;
        public event Action<TranslationResult> FinalResultReady;
        public event Action<TranslationCancelled> TranslationCancelled;
        public event AudioInputDataAvailable InputDataAvailable;

        public GoogleTranslationProvider
            (
            IAudioRecorder audioRecorder,
            IAudioFileSaver audioFileSaver,
            string googleJsonCredentials
            )
        {
            IsMute = false;
            Reversed = false;
            _audioRecorder = audioRecorder;
            _audioFileSaver = audioFileSaver;
            GoogleJsonCredentials = googleJsonCredentials ?? throw new ArgumentNullException($"Google Json Credentials is null");
        }

        private async Task InitializeClients(bool allowExplicitContent)
        {
            InitializeForwardClients(allowExplicitContent);
            InitializeReverseClients(allowExplicitContent);
            await InitializeTextToTextTranslatorClient(allowExplicitContent);
        }

        private void InitializeForwardClients(bool allowExplicitContent)
        {
            if (TargetLanguage.UseNeuralVoice)
            {
                ForwardGoogleSpeechToTextClient = new GoogleSpeechToTextClient(SourceLanguage.Code, _audioRecorder, _inputDevice, GoogleJsonCredentials);
                ForwardNeuralSynthesizer = SetupNeuralVoiceSynthesizer(TargetLanguage.Code, TargetLanguage.VoiceName);
            }
        }

        private void InitializeReverseClients(bool allowExplicitContent)
        {
            if (SourceLanguage.UseNeuralVoice)
            {
                ReversedGoogleSpeechToTextClient = new GoogleSpeechToTextClient(TargetLanguage.Code, _audioRecorder, _inputDevice, GoogleJsonCredentials);
                ReverseNeuralSynthesizer = SetupNeuralVoiceSynthesizer(SourceLanguage.Code, SourceLanguage.VoiceName);
            }
        }

        private async Task InitializeTextToTextTranslatorClient(bool allowExplicitContent)
        {
            TextToTextClient = new GoogleTextToTextTranslator(GoogleJsonCredentials);
            TextToTextClient.GoogleTextTranslationOnTextAvailable += OnTranslatedTextAvailable;
            await TextToTextClient.Initialize();
        }

        public void Mute()
        {
            IsMute = true;
        }

        public void UnMute()
        {
            IsMute = false;
        }

        public async Task Translate
            (
            Language sourceLanguage,
            Language targetLanguage,
            string audioFilePath,
            bool allowExplicitContent,
            AudioDevice audioDevice,
            string azureKey,
            string azureRegion
            )
        {
            try
            {
                Reversed = false;
                IsMute = false;
                SourceLanguage = sourceLanguage ?? throw new ArgumentNullException($"Source Language is null");
                TargetLanguage = targetLanguage ?? throw new ArgumentNullException($"Target Language is null");
                _audioFilePath = audioFilePath ?? throw new ArgumentNullException($"FilePath is null");
                AzureKey = azureKey ?? throw new ArgumentNullException($"Azure Key is null");
                AzureRegion = azureRegion ?? throw new ArgumentNullException($"Azure Region is null");
                _inputDevice = audioDevice.InputDevice;
                _audioDataInput = new AudioDataInput();
                _audioInputQueue = new ConcurrentQueue<AudioDataInput>();

                await InitializeClients(allowExplicitContent);
                await StartForwardSpeechToTextClient();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void OnInputDataAvailable(object sender, AudioInputDataAvailableArgs args)
        {
            _audioDataInput.Bytes = args.Buffer;
            _audioDataInput.ByteCount = args.Count;
            _audioInputQueue.Enqueue(_audioDataInput);
            WriteToFile();
        }

        void WriteToFile()
        {
            while (_audioInputQueue.Any())
            {
                AudioDataInput dequed;
                _audioInputQueue.TryDequeue(out dequed);
                if (dequed != null)
                    _audioFileSaver.WriteToFile(_audioFilePath, dequed);
            }
        }

        private async void OnSpeechToTextTextAvailable(GoogleSpeechToTextResponse speechToTextResponse)
        {
            if (SpeechToTextResponseQueue == null)
            {
                SpeechToTextResponseQueue = new ConcurrentQueue<GoogleSpeechToTextResponse>();
            }

            SpeechToTextResponseQueue.Enqueue(speechToTextResponse);

            Debug.WriteLine($"Final Response Recieved: {speechToTextResponse}");

            await TranslateTextResponse(speechToTextResponse.SpeechText, speechToTextResponse.OffsetInTicks, speechToTextResponse.Duration);
        }

        //  We probably need an event for this
        private async Task TranslateTextResponse(string transcript, long offsetInTicks, TimeSpan duration)
        {
            try
            {
                while (SpeechToTextResponseQueue.Any())
                {
                    GoogleSpeechToTextResponse googleSpeechToTextResponse;
                    SpeechToTextResponseQueue.TryDequeue(out googleSpeechToTextResponse);

                    if (googleSpeechToTextResponse != null)
                    {
                        string sourceLanguageCode;
                        string targetLanguageCode;

                        if (!Reversed)
                        {
                            sourceLanguageCode = SourceLanguage.Code;
                            targetLanguageCode = TargetLanguage.Code;
                        }
                        else
                        {
                            sourceLanguageCode = TargetLanguage.Code;
                            targetLanguageCode = SourceLanguage.Code;
                        }

                        Debug.WriteLine($"Source Language Code: {sourceLanguageCode}, Target Language Code: {targetLanguageCode}");

                        await TextToTextClient.TranslateAsync(transcript, sourceLanguageCode, targetLanguageCode, offsetInTicks, duration);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void OnTranslatedTextAvailable(GoogleTextTranslationResponse googleTextTranslationResponse)
        {
            try
            {
                var result = new TranslationResult
                {
                    Guid = Guid,
                    OriginalText = googleTextTranslationResponse.OriginalText,
                    SourceLanguageCode = SourceLanguage.Code,
                    TargetLanguageCode = TargetLanguage.Code,
                    TranslatedText = googleTextTranslationResponse.TranslatedText,
                    OffsetInTicks = googleTextTranslationResponse.OffsetInTicks,
                    Duration = googleTextTranslationResponse.Duration
                };

                TranscriptionResultReady?.Invoke(result);

                Debug.WriteLine($"Original Text: {result.OriginalText}, Translated Text: {result.TranslatedText}");
                Debug.WriteLine($"Synthesizing Audio");
                try
                {
                    if (!Reversed && TargetLanguage.UseNeuralVoice)
                    {
                        await ForwardNeuralSynthesizer.Synthesize(CancellationToken.None, result);
                    }
                    else if (Reversed && SourceLanguage.UseNeuralVoice)
                    {
                        await ReverseNeuralSynthesizer.Synthesize(CancellationToken.None, result);
                    }
                }
                catch (Exception ex)
                {
                    //TODO log exception
                    Debug.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Synthesizer_OnError(object sender, SynthesizerEventArgs<Exception> e)
        {
            Console.WriteLine($"Neural Synthesizer Error: {e.EventData}");
        }

        private void Synthesizer_OnAudioAvailable(object sender, SynthesizerEventArgs<TranslationResult> e)
        {
            var result = new TranslationResult
            {
                Guid = Guid,
                SourceLanguageCode = SourceLanguage.Code,
                TargetLanguageCode = TargetLanguage.Code,
                AudioResult = e.EventData.AudioResult
            };

            TranslationSpeechReady?.Invoke(result);
            FinalResultReady?.Invoke(result);

            Debug.WriteLine($"OnAudioRecieved");
        }

        private MicrosoftNeuralVoiceSynthesizer SetupNeuralVoiceSynthesizer(string targetLanguageCode, string voiceName)
        {
            // TODO: Remove this when allocation of region specific endpoints is implemented
            // Make sure the region supports neural voice
            string tokenUrl = $"https://{AzureRegion}.api.cognitive.microsoft.com/sts/v1.0/issueToken";
            string endpointUri = $"https://{AzureRegion}.tts.speech.microsoft.com/cognitiveservices/v1";

            var auth = new CognitiveServiceAuthentication(tokenUrl, AzureKey);

            var accessToken = auth.GetAccessToken();

            SynthesizerInputOptions inputOptions = new SynthesizerInputOptions()
            {
                RequestUri = new Uri(endpointUri),
                Guid = Guid.NewGuid(),
                VoiceType = Gender.Female,
                Locale = targetLanguageCode,
                VoiceName = voiceName,
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken
            };

            return new MicrosoftNeuralVoiceSynthesizer(inputOptions);
        }

        public async Task StopAndResetSpeechRecognizer()
        {
            if (!Reversed)
                await ForwardGoogleSpeechToTextClient.StopTranslationAsync();
            else
                await ReversedGoogleSpeechToTextClient.StopTranslationAsync();
        }

        public async Task SwitchSpeakers()
        {
            try
            {
                if (!Reversed)
                {
                    await StopForwardSpeechToTextClient();
                    await StartReverseSpeechToTextClient();
                    Reversed = true;
                }
                else
                {
                    await StopReverseSpeechToTextClient();
                    await StartForwardSpeechToTextClient();
                    Reversed = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task StartForwardSpeechToTextClient()
        {
            ForwardNeuralSynthesizer.OnAudioAvailable += Synthesizer_OnAudioAvailable;
            ForwardNeuralSynthesizer.OnError += Synthesizer_OnError;
            ForwardGoogleSpeechToTextClient.InputDataAvailable += OnInputDataAvailable;
            ForwardGoogleSpeechToTextClient.GoogleSpeechToTextOnTextAvailable += OnSpeechToTextTextAvailable;
            await ForwardGoogleSpeechToTextClient.StartSpeechToTextAsync();
        }

        private async Task StartReverseSpeechToTextClient()
        {
            ReverseNeuralSynthesizer.OnAudioAvailable += Synthesizer_OnAudioAvailable;
            ReverseNeuralSynthesizer.OnError += Synthesizer_OnError;
            ReversedGoogleSpeechToTextClient.InputDataAvailable += OnInputDataAvailable;
            ReversedGoogleSpeechToTextClient.GoogleSpeechToTextOnTextAvailable += OnSpeechToTextTextAvailable;
            await ReversedGoogleSpeechToTextClient.StartSpeechToTextAsync();
        }

        private async Task StopForwardSpeechToTextClient()
        {
            ForwardNeuralSynthesizer.OnAudioAvailable -= Synthesizer_OnAudioAvailable;
            ForwardNeuralSynthesizer.OnError -= Synthesizer_OnError;
            ForwardGoogleSpeechToTextClient.InputDataAvailable -= OnInputDataAvailable;
            ForwardGoogleSpeechToTextClient.GoogleSpeechToTextOnTextAvailable -= OnSpeechToTextTextAvailable;
            await ForwardGoogleSpeechToTextClient.StopTranslationAsync();
        }

        private async Task StopReverseSpeechToTextClient()
        {
            ReverseNeuralSynthesizer.OnAudioAvailable -= Synthesizer_OnAudioAvailable;
            ReverseNeuralSynthesizer.OnError -= Synthesizer_OnError;
            ReversedGoogleSpeechToTextClient.InputDataAvailable -= OnInputDataAvailable;
            ReversedGoogleSpeechToTextClient.GoogleSpeechToTextOnTextAvailable -= OnSpeechToTextTextAvailable;
            await ReversedGoogleSpeechToTextClient.StopTranslationAsync();
        }

        public Task AutoDetectTranslate(Language sourceLanguage, Language targetLanguage, string audioFilePath, bool allowExplicitContent, AudioDevice audioDevice, string azureKey, string azureRegion, List<Language> candidateLanguages = null)
        {
            return null;
        }

        public Task StopAndResetAutoDetectSpeechRecognizer()
        {
            return null;
        }

        public void WriteToFile(byte[] bytes)
        {
            if (_audioFileSaver != null && _audioInputQueue != null)
            {
                _audioInputQueue.Enqueue(new AudioDataInput { ByteCount = bytes.Length, Bytes = bytes });
            }

            WriteToFile();
        }
    }
}
