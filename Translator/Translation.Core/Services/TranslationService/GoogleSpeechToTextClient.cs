using Google.Cloud.Speech.V1;
using Google.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Core.Events;
using Translation.Core.Interfaces;

namespace Translation.Core.Services.TranslationService
{
    public class GoogleSpeechToTextClient : IGoogleSpeechToTextClient
    {
        private SpeechClient _client;

        public event Action<GoogleSpeechToTextResponse> GoogleSpeechToTextOnTextAvailable;

        /// <summary>
        /// Sample code for infinite streaming. The strategy for infinite streaming is to restart each stream
        /// shortly before it would time out (currently at 5 minutes). We keep track of the end result time of
        /// of when we last saw a "final" transcription, and resend the audio data we'd recorded past that point.
        /// </summary>

        private const int SampleRate = 16000;
        private const int ChannelCount = 1;
        private const int BytesPerSample = 2;
        private const int BytesPerSecond = SampleRate * ChannelCount * BytesPerSample;
        private static readonly TimeSpan s_streamTimeLimit = TimeSpan.FromSeconds(290);

        ///// <summary>
        ///// Microphone chunks that haven't yet been processed at all.
        ///// </summary>
        //private readonly BlockingCollection<ByteString> _microphoneBuffer = new BlockingCollection<ByteString>();

        ///// <summary>
        ///// Chunks that have been sent to Cloud Speech, but not yet finalized.
        ///// </summary>
        //private readonly LinkedList<ByteString> _processingBuffer = new LinkedList<ByteString>();

        private ByteString _dequeuedByteString;

        private ConcurrentQueue<ByteString> _audioInput { get; set; }

        /// <summary>
        /// The start time of the processing buffer, in relation to the start of the stream.
        /// </summary>
        private TimeSpan _processingBufferStart;

        /// <summary>
        /// The current RPC stream, if any.
        /// </summary>
        private SpeechClient.StreamingRecognizeStream _rpcStream;

        /// <summary>
        /// The deadline for when we should stop the current stream.
        /// </summary>
        private DateTime _rpcStreamDeadline;

        /// <summary>
        /// The task indicating when the next response is ready, or when we've
        /// reached the end of the stream. (The task will complete in either case, with a result
        /// of True if it's moved to another response, or False at the end of the stream.)
        /// </summary>
        private ValueTask<bool> _serverResponseAvailableTask;

        private bool _listenToMicInput { get; set; }

        private Thread _translationThread { get; set; }

        public event AudioInputDataAvailable InputDataAvailable;
        private AudioInputDataAvailableArgs _audioInputDataAvailableArgs { get; set; }

        private readonly object _lockObject = new object();
        private readonly string _jsonCredentials;
        private readonly string _languageCode;
        private readonly IAudioRecorder _audioRecorder;
        private readonly InputDevice _inputDevice;
        private bool _processAudio { get; set; }

        public GoogleSpeechToTextClient
            (
            string languageCode,
            IAudioRecorder audioRecorder,
            InputDevice inputDevice,
            string jsonCredentials
            )
        {
            _languageCode = languageCode;
            _audioRecorder = audioRecorder;
            _inputDevice = inputDevice;
            _jsonCredentials = jsonCredentials;
            _processAudio = false;
            _listenToMicInput = false;
        }

        private async Task Initialize()
        {
            try
            {
                if (_client == null)
                {
                    SpeechClientBuilder speechClientBuilder = new SpeechClientBuilder
                    {
                        JsonCredentials = _jsonCredentials
                    };

                    _client = await speechClientBuilder.BuildAsync();
                }

                _audioInput = new ConcurrentQueue<ByteString>();
                _audioInputDataAvailableArgs = new AudioInputDataAvailableArgs();
                _audioRecorder.DataAvailable += OnDataAvailable;

                _listenToMicInput = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Runs the main loop until "exit" or "quit" is heard.
        /// </summary>
        public async Task StartSpeechToTextAsync()
        {
            await Initialize();
            await _audioRecorder.StartRecording(_inputDevice);
            StartProcessingSpeech();
        }

        private async void StartProcessingSpeech()
        {
            while (_listenToMicInput)
            {
                await MaybeStartStreamAsync();
                await ProcessResponsesAsync();
            }
        }

        private async void OnDataAvailable(object sender, DataAvailableEventArgs args)
        {
            try
            {
                if (!_listenToMicInput)
                    return;
                _audioInputDataAvailableArgs.Buffer = args.AudioDataInput.Bytes;
                _audioInputDataAvailableArgs.Count = args.AudioDataInput.ByteCount;
                InputDataAvailable?.Invoke(this, _audioInputDataAvailableArgs);
                _audioInput.Enqueue(ByteString.CopyFrom(args.AudioDataInput.Bytes, 0, args.AudioDataInput.ByteCount));
                if (_processAudio)
                    await ProcessAudioInput();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task ProcessAudioInput()
        {
            try
            {
                while (_audioInput.Any() && _processAudio)
                {
                    _audioInput.TryDequeue(out _dequeuedByteString);

                    if (_dequeuedByteString != null && _processAudio)
                        await WriteAudioChunk(_dequeuedByteString);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Starts a new RPC streaming call if necessary. This will be if either it's the first call
        /// (so we don't have a current request) or if the current request will time out soon.
        /// In the latter case, after starting the new request, we copy any chunks we'd already sent
        /// in the previous request which hadn't been included in a "final result".
        /// </summary>
        private async Task MaybeStartStreamAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                if (_rpcStream != null && now >= _rpcStreamDeadline)
                {
                    Console.WriteLine($"Closing stream before it times out");
                    // STOP AUDIO PROCESSING
                    _processAudio = false;
                    await _rpcStream.WriteCompleteAsync();
                    _rpcStream.GrpcCall.Dispose();
                    _rpcStream = null;
                }

                // If we have a valid stream at this point, we're fine.
                if (_rpcStream != null)
                {
                    return;
                }
                // We need to create a new stream, either because we're just starting or because we've just closed the previous one.
                _rpcStream = _client.StreamingRecognize();
                _rpcStreamDeadline = now + s_streamTimeLimit;
                _processingBufferStart = TimeSpan.Zero;
                _serverResponseAvailableTask = _rpcStream.GetResponseStream().MoveNextAsync();
                await _rpcStream.WriteAsync(new StreamingRecognizeRequest
                {
                    StreamingConfig = new StreamingRecognitionConfig
                    {
                        Config = new RecognitionConfig
                        {
                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = SampleRate,
                            LanguageCode = _languageCode.Substring(0, 2),
                            MaxAlternatives = 1,
                            EnableWordTimeOffsets = true,
                            EnableAutomaticPunctuation = true,
                            EnableWordConfidence = true,
                            Metadata = new RecognitionMetadata
                            {
                                InteractionType = RecognitionMetadata.Types.InteractionType.Dictation,
                                MicrophoneDistance = RecognitionMetadata.Types.MicrophoneDistance.Nearfield,
                                OriginalMediaType = RecognitionMetadata.Types.OriginalMediaType.Audio,
                                RecordingDeviceType = RecognitionMetadata.Types.RecordingDeviceType.Smartphone
                            },
                        },
                        InterimResults = true,
                        SingleUtterance = false
                    }
                });

                _processAudio = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Processes responses received so far from the server,
        /// returning whether "exit" or "quit" have been heard.
        /// </summary>
        private async Task ProcessResponsesAsync()
        {
            try
            {
                if (_processAudio)
                {
                    await Task.Run(() =>
                    {
                        while (_serverResponseAvailableTask.IsCompleted && _serverResponseAvailableTask.Result)
                        {
                            var response = _rpcStream.GetResponseStream().Current;

                            _serverResponseAvailableTask = _rpcStream.GetResponseStream().MoveNextAsync();
                            // Uncomment this to see the details of interim results when InterimResults = true

                            foreach (var interim in response.Results)
                            {
                                foreach (var result in interim.Alternatives)
                                {
                                    Debug.WriteLine($"Alternertives: {result.Transcript} Stability {interim.Stability} Offset {interim.ResultEndTime}");
                                }
                            }

                            // See if one of the results is a "final result".
                            var hasFinal = response.Results.Any(s => s.IsFinal);

                            if (hasFinal)
                            {
                                var finalResult = response.Results.FirstOrDefault(s => s.IsFinal);
                                if (finalResult != null)
                                {
                                    string transcript = finalResult.Alternatives[0].Transcript;
                                    TimeSpan resultEndTime = finalResult.ResultEndTime.ToTimeSpan();

                                    // Rather than explicitly iterate over the list, we just always deal with the first
                                    // element, either removing it or stopping.
                                    double duration = 0;
                                    int removed = 0;
                                    while (_dequeuedByteString != null)
                                    {
                                        var sampleDuration = TimeSpan.FromSeconds(_dequeuedByteString.Length / (double)BytesPerSecond);
                                        duration += sampleDuration.TotalSeconds;
                                        var sampleEnd = _processingBufferStart + sampleDuration;

                                        // If the first sample in the buffer ends after the result ended, stop.
                                        // Note that part of the sample might have been included in the result, but the samples
                                        // are short enough that this shouldn't cause problems.
                                        if (sampleEnd > resultEndTime)
                                        {
                                            break;
                                        }
                                        _processingBufferStart = sampleEnd;
                                        removed++;
                                    }

                                    GoogleSpeechToTextOnTextAvailable?.Invoke(new GoogleSpeechToTextResponse { SpeechText = transcript, Duration = TimeSpan.FromSeconds(duration), OffsetInTicks = resultEndTime.Ticks });

                                    Debug.WriteLine($"Final Response Invoked: {transcript}");
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Writes a single chunk to the RPC stream.
        /// </summary>
        private async Task WriteAudioChunk(ByteString chunk)
        {
            await _rpcStream.WriteAsync(new StreamingRecognizeRequest { AudioContent = chunk });
        }

        public async Task<bool> StopTranslationAsync()
        {
            try
            {
                //if (_translationThread.IsAlive)
                //    _translationThread.Abort();

                if (_rpcStream != null)
                {
                    _listenToMicInput = false;
                    _processAudio = false;
                    await _rpcStream.WriteCompleteAsync();
                    _rpcStream.GrpcCall.Dispose();
                    _rpcStream = null;
                }

                if (_audioRecorder != null)
                {
                    _audioRecorder.DataAvailable -= OnDataAvailable;
                    if (_audioRecorder.IsRecording)
                        _audioRecorder.StopRecording();
                }

                if (_audioInput != null)
                {
                    // empty queue to free up memory
                    ByteString item;
                    while (_audioInput.Any())
                    {
                        _audioInput.TryDequeue(out item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return true;
        }

        public void PauseSpeechToTextClient()
        {
            //_listenToMicInput = false;
            //_audioRecorder.DataAvailable -= OnDataAvailable;

            // will think about this implementation in future
            // the above implementation causes the app to crash
            // probably because without stoping _rpcStream, it will throw an
            // error if it doesn't recieve microphone input after a while
            // no loss though, stopping and starting is pretty fast :)
        }
    }
}
