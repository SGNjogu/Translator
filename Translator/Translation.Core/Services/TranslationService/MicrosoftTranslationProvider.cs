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
using Translation.Core.Events;
using Translation.Core.Interfaces;
using Translation.Core.Utils;
using Translation.Interface;

namespace Translation.Core.Services.TranslationService
{
    public partial class MicrosoftTranslationProvider : ITranslator
    {
        public Language SourceLanguage { get; private set; }

        public Language TargetLanguage { get; private set; }

        public string AzureKey { get; private set; }
        public string AzureRegion { get; private set; }

        public string OriginalText { get; set; }

        public Guid Guid { get; set; }

        public bool IsMute { get; set; }

        // Translation config for ForwardRecognizer
        private SpeechTranslationConfig ForwardTranslationConfig { get; set; }
        // Translation config for ForwardRecognizer
        private SpeechTranslationConfig ReverseTranslationConfig { get; set; }

        // SourceLanguage To TargetLanguage
        private TranslationRecognizer ForwardRecognizer { get; set; }
        // TargetLanguage to SourceLanguage
        private TranslationRecognizer ReverseRecognizer { get; set; }

        private MicrosoftNeuralVoiceSynthesizer ForwardNeuralSynthesizer { get; set; }

        private MicrosoftNeuralVoiceSynthesizer ReverseNeuralSynthesizer { get; set; }

        private MicrosoftNeuralVoiceSynthesizer AutoDetectNeuralSynthesizer { get; set; }

        // Input stream read by forward recognizer
        private PushAudioInputStream _forwardAudioInputStream = null;

        // Input stream read by reverse recognizer
        private PushAudioInputStream _reverseAudioInputStream = null;

        // Tracking which TranslationConfig and Recognizer is in use
        public bool Reversed { get; set; }

        private ProfanityOption ProfanityOption { get; set; }
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

        public MicrosoftTranslationProvider(IAudioRecorder audioRecorder, IAudioFileSaver audioFileSaver)
        {
            IsMute = false;
            Reversed = false;
            _audioRecorder = audioRecorder;
            _audioFileSaver = audioFileSaver;
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
                _audioInputQueue = new ConcurrentQueue<AudioDataInput>();
                InitializeDevice(allowExplicitContent);
                await ForwardRecognizer.StartContinuousRecognitionAsync();
                await ReverseRecognizer.StartContinuousRecognitionAsync();
                _audioRecorder.DataAvailable += Recorder_OnDataAvailable;
                await _audioRecorder.StartRecording(audioDevice.InputDevice);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Recorder_OnDataAvailable(object sender, DataAvailableEventArgs args)
        {
            if (IsMute)
                return;

            // Write to the correct stream based on Reverse
            if (!Reversed)
            {
                if (_forwardAudioInputStream != null)
                {
                    _forwardAudioInputStream.Write(args.AudioDataInput.Bytes, args.AudioDataInput.ByteCount);
                }
            }
            else
            {
                if (_reverseAudioInputStream != null)
                {
                    _reverseAudioInputStream.Write(args.AudioDataInput.Bytes, args.AudioDataInput.ByteCount);
                }
            }

            _audioInputQueue.Enqueue(args.AudioDataInput);
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

        public void WriteToFile(byte[] bytes)
        {
            if (_audioFileSaver != null && _audioInputQueue != null)
            {
                _audioInputQueue.Enqueue(new AudioDataInput { ByteCount = bytes.Length, Bytes = bytes });
            }

            WriteToFile();
        }

        private void InitializeDevice(bool allowExplicitContent)
        {
            CreateAudioStream();
            InitializeReverseRecognizer(allowExplicitContent);
            InitializeForwardRecognizer(allowExplicitContent);
        }

        /// <summary>
        /// Creates forward recognizer
        /// </summary>
        private void InitializeForwardRecognizer(bool allowExplicitContent)
        {
            if (allowExplicitContent)
                ProfanityOption = ProfanityOption.Raw;
            else
                ProfanityOption = ProfanityOption.Masked;

            ForwardTranslationConfig = SpeechTranslationConfig.FromSubscription(AzureKey, AzureRegion);
            ForwardTranslationConfig.SpeechRecognitionLanguage = SourceLanguage.Code;
            ForwardTranslationConfig.AddTargetLanguage(TargetLanguage.Code);

            if (TargetLanguage.UseNeuralVoice)
            {
                ForwardNeuralSynthesizer = SetupNeuralVoiceSynthesizer(TargetLanguage.Code, TargetLanguage.VoiceName);
                ForwardNeuralSynthesizer.OnAudioAvailable += Synthesizer_OnAudioAvailable;
                ForwardNeuralSynthesizer.OnError += Synthesizer_OnError;
            }
            else
            {
                ForwardTranslationConfig.VoiceName = TargetLanguage.VoiceName;
            }

            ForwardTranslationConfig.SetProfanity(ProfanityOption);
            ForwardTranslationConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
            ForwardTranslationConfig.RequestWordLevelTimestamps();
            //var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT);
            //AudioConfig audioConfig = AudioConfig.FromStreamInput(_forwardAudioInputStream, audioProcessingOptions);
            AudioConfig audioConfig = AudioConfig.FromStreamInput(_forwardAudioInputStream);
            ForwardRecognizer = new TranslationRecognizer(ForwardTranslationConfig, audioConfig);

            ForwardRecognizer.Recognizing += OnRecognizing;
            ForwardRecognizer.Recognized += OnRecognized;
            ForwardRecognizer.Synthesizing += OnSynthesizing;
            ForwardRecognizer.Canceled += OnCanceled;
        }

        /// <summary>
        /// Creates Reverse Recognizer
        /// </summary>
        private void InitializeReverseRecognizer(bool allowExplicitContent)
        {
            if (allowExplicitContent)
                ProfanityOption = ProfanityOption.Raw;
            else
                ProfanityOption = ProfanityOption.Masked;

            ReverseTranslationConfig = SpeechTranslationConfig.FromSubscription(AzureKey, AzureRegion);
            ReverseTranslationConfig.SpeechRecognitionLanguage = TargetLanguage.Code;
            ReverseTranslationConfig.AddTargetLanguage(SourceLanguage.Code);

            if (SourceLanguage.UseNeuralVoice)
            {
                ReverseNeuralSynthesizer = SetupNeuralVoiceSynthesizer(SourceLanguage.Code, SourceLanguage.VoiceName);
                ReverseNeuralSynthesizer.OnAudioAvailable += Synthesizer_OnAudioAvailable;
                ReverseNeuralSynthesizer.OnError += Synthesizer_OnError;
            }
            else
            {
                ReverseTranslationConfig.VoiceName = TargetLanguage.VoiceName;
            }

            ReverseTranslationConfig.SetProfanity(ProfanityOption);
            ReverseTranslationConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
            ReverseTranslationConfig.RequestWordLevelTimestamps();
            var audioProcessingOptions = AudioProcessingOptions.Create(AudioProcessingConstants.AUDIO_INPUT_PROCESSING_ENABLE_DEFAULT);
            AudioConfig audioConfig = AudioConfig.FromStreamInput(_reverseAudioInputStream, audioProcessingOptions);
            ReverseRecognizer = new TranslationRecognizer(ReverseTranslationConfig, audioConfig);

            ReverseRecognizer.Recognizing += OnRecognizing;
            ReverseRecognizer.Recognized += OnRecognized;
            ReverseRecognizer.Synthesizing += OnSynthesizing;
            ReverseRecognizer.Canceled += OnCanceled;
        }

        private async void OnRecognized(object sender, TranslationRecognitionEventArgs e)
        {
            if (e.Result.Text.Length > 0)
            {
                TranslationResult result = new TranslationResult();
                if (offsetQueue.Any())
                {
                    var firstItem = offsetQueue.Dequeue();
                    result = new TranslationResult
                    {
                        Guid = Guid,
                        OriginalText = e.Result.Text,
                        SourceLanguageCode = SourceLanguage.Code,
                        TargetLanguageCode = TargetLanguage.Code,
                        Duration = e.Result.Duration,
                        OffsetInTicks = firstItem
                    };
                }
                else
                {
                    result = new TranslationResult
                    {
                        Guid = Guid,
                        OriginalText = e.Result.Text,
                        SourceLanguageCode = SourceLanguage.Code,
                        TargetLanguageCode = TargetLanguage.Code,
                        Duration = e.Result.Duration,
                        OffsetInTicks = e.Result.OffsetInTicks
                    };
                }

                foreach (var t in e.Result.Translations)
                {
                    result.TranslatedText = t.Value;
                }

                try
                {
                    if (IsReversed())
                    {
                        await ReverseNeuralSynthesizer.Synthesize(CancellationToken.None, result);
                    }
                    else if (!IsReversed())
                    {
                        await ForwardNeuralSynthesizer.Synthesize(CancellationToken.None, result);
                    }

                    TranscriptionResultReady?.Invoke(result);
                }
                catch (Exception ex)
                {
                    //TODO log exception
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private bool IsReversed()
        {
            if (Reversed && SourceLanguage.UseNeuralVoice)
                return true;
            else if (!Reversed && TargetLanguage.UseNeuralVoice)
                return false;
            return false;
        }

        private void Synthesizer_OnError(object sender, SynthesizerEventArgs<Exception> e)
        {
            Console.WriteLine($"Neural Synthesizer Error: {e.EventData}");
        }

        private void Synthesizer_OnAudioAvailable(object sender, SynthesizerEventArgs<TranslationResult> e)
        {
            var result = e.EventData;
            TranscriptionResultReady?.Invoke(result);
            TranslationSpeechReady?.Invoke(result);
            FinalResultReady?.Invoke(result);
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
                Guid = Guid,
                VoiceType = Gender.Female,
                Locale = targetLanguageCode,
                VoiceName = voiceName,
                OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                AuthorizationToken = "Bearer " + accessToken
            };

            return new MicrosoftNeuralVoiceSynthesizer(inputOptions);
        }

        Queue<long> offsetQueue = new Queue<long>();
        long offset;
        private void OnRecognizing(object sender, TranslationRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.TranslatingSpeech)
            {
                var partialResult = new TranslationResult
                {
                    Guid = Guid,
                    OriginalText = e.Result.Text,
                    SourceLanguageCode = SourceLanguage.Code,
                    TargetLanguageCode = TargetLanguage.Code,
                    Duration = e.Result.Duration,
                    OffsetInTicks = e.Result.OffsetInTicks
                };

                if (offset != e.Result.OffsetInTicks)
                {
                    offset = e.Result.OffsetInTicks;
                    offsetQueue.Enqueue(offset);
                }

                foreach (var t in e.Result.Translations)
                {
                    partialResult.TranslatedText = t.Value;
                }

                PartialResultReady?.Invoke(partialResult);
            }
        }

        private void OnSynthesizing(object sender, TranslationSynthesisEventArgs translationSynthesisEventArgs)
        {
            var audio = translationSynthesisEventArgs.Result.GetAudio();

            if (audio.Length == 0)
                return;

            var result = new TranslationResult
            {
                Guid = Guid,
                SourceLanguageCode = SourceLanguage.Code,
                TargetLanguageCode = TargetLanguage.Code,
                AudioResult = translationSynthesisEventArgs.Result.GetAudio()
            };

            TranslationSpeechReady?.Invoke(result);
            FinalResultReady?.Invoke(result);
        }

        private void OnCanceled(object sender, TranslationRecognitionCanceledEventArgs e)
        {
            var translationCancelled = new TranslationCancelled
            {
                Reason = e.Reason.ToString(),
                ErrorCode = e.ErrorCode.ToString(),
                ErrorDetails = e.ErrorDetails
            };

            TranslationCancelled?.Invoke(translationCancelled);
        }

        private void CreateAudioStream()
        {
            if (_forwardAudioInputStream == null)
            {
                _forwardAudioInputStream = AudioInputStream.CreatePushStream(CreateAudioStreamFormat());
            }

            if (_reverseAudioInputStream == null)
            {
                _reverseAudioInputStream = AudioInputStream.CreatePushStream(CreateAudioStreamFormat());

            }
        }

        /// <summary>
        /// Creates a 16KHz, 16 bit, mono PCM audio stream
        /// </summary>
        /// <returns></returns>
        private AudioStreamFormat CreateAudioStreamFormat()
        {
            byte channels = 1;
            byte bitsPerSample = 16;
            uint samplesPerSecond = 16000;

            return AudioStreamFormat.GetWaveFormatPCM(samplesPerSecond, bitsPerSample, channels);
        }

        /// <summary>
        /// Method for stopping and reseting 
        /// recognizer 
        /// Called when user clicks on Stop translating
        /// </summary>
        /// <returns></returns>
        public async Task StopAndResetSpeechRecognizer()
        {
            try
            {
                if (_audioFileSaver != null)
                    await _audioFileSaver.SaveFile();

                if (_audioRecorder != null)
                    _audioRecorder.DataAvailable -= Recorder_OnDataAvailable;

                if (ForwardRecognizer != null)
                {
                    ForwardRecognizer.Recognizing -= OnRecognizing;
                    ForwardRecognizer.Recognized -= OnRecognized;
                    ForwardRecognizer.Synthesizing -= OnSynthesizing;
                    ForwardRecognizer.Canceled -= OnCanceled;
                    ForwardNeuralSynthesizer.OnAudioAvailable -= Synthesizer_OnAudioAvailable;
                }

                if (ReverseRecognizer != null)
                {
                    ReverseRecognizer.Recognizing -= OnRecognizing;
                    ReverseRecognizer.Recognized -= OnRecognized;
                    ReverseRecognizer.Synthesizing -= OnSynthesizing;
                    ReverseRecognizer.Canceled -= OnCanceled;
                    ReverseNeuralSynthesizer.OnAudioAvailable -= Synthesizer_OnAudioAvailable;
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

                // Stop continous recognition for forward recognizer
                if (ForwardRecognizer != null)
                {
                    await ForwardRecognizer.StopContinuousRecognitionAsync();
                }

                // Stop continous recognition for reverse recognizer
                if (ReverseRecognizer != null)
                {
                    await ReverseRecognizer.StopContinuousRecognitionAsync();
                }

                // Reset config
                ForwardTranslationConfig = null;
                ReverseTranslationConfig = null;

                // Reset recognizer
                ReverseRecognizer = null;
                ForwardRecognizer = null;
                IsMute = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task SwitchSpeakers()
        {
            Reversed = !Reversed;
        }
    }

    public delegate void SpeechStartDetected(object sender, RecognitionEventArgs args);
    public delegate void Synthesizing(object sender, TranslationSynthesisEventArgs args);
    public delegate void Recognizing(object sender, TranslationRecognitionEventArgs args);
    public delegate void Recognized(object sender, TranslationRecognitionEventArgs args);
    public delegate void Cancelled(object sender, TranslationRecognitionCanceledEventArgs args);
}
