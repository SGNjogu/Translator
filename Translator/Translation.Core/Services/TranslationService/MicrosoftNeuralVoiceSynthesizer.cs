using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Translation.Core.Domain;
using Translation.Core.Events;
using Translation.Core.Utils;

namespace Translation.Core.Services.TranslationService
{
    /// <summary>
    /// Synthesize request
    /// </summary>
    public class MicrosoftNeuralVoiceSynthesizer
    {
        /// <summary>
        /// Generates SSML.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="name">The voice name.</param>
        /// <param name="text">The text input.</param>
        private string GenerateSsml(string locale, string gender, string name, string text)
        {
            var ssmlDoc = new XDocument(
                              new XElement("speak",
                                  new XAttribute("version", "1.0"),
                                  new XAttribute(XNamespace.Xml + "lang", locale),
                                  new XElement("voice",
                                      new XAttribute(XNamespace.Xml + "lang", locale),
                                      new XAttribute(XNamespace.Xml + "gender", gender),
                                      new XAttribute("name", name),
                                      text)));
            return ssmlDoc.ToString();
        }

        private HttpClient client;
        private HttpClientHandler handler;
        private SynthesizerInputOptions inputOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Synthesize"/> class.
        /// </summary>
        public MicrosoftNeuralVoiceSynthesizer(SynthesizerInputOptions _inputOptions)
        {
            var cookieContainer = new CookieContainer();
            handler = new HttpClientHandler() { CookieContainer = new CookieContainer(), UseProxy = false };
            client = new HttpClient(handler);
            inputOptions = _inputOptions;
        }

        ~MicrosoftNeuralVoiceSynthesizer()
        {
            client.Dispose();
            handler.Dispose();
        }

        /// <summary>
        /// Called when a TTS request has been completed and audio is available.
        /// </summary>
        public event EventHandler<SynthesizerEventArgs<TranslationResult>> OnAudioAvailable;

        /// <summary>
        /// Called when an error has occured. e.g this could be an HTTP error.
        /// </summary>
        public event EventHandler<SynthesizerEventArgs<Exception>> OnError;

        /// <summary>
        /// Sends the specified text to be spoken to the TTS service and saves the response audio to a file.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="inputOptions">SynthesizerInputOptions</param>
        /// <param name="text">Text to synthesize</param>
        /// <returns>A Task</returns>
        public async Task Synthesize(CancellationToken cancellationToken, TranslationResult translationResut)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                foreach (var header in inputOptions.Headers)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }

                var genderValue = "";
                switch (inputOptions.VoiceType)
                {
                    case Gender.Male:
                        genderValue = "Male";
                        break;

                    case Gender.Female:
                    default:
                        genderValue = "Female";
                        break;
                }

                var request = new HttpRequestMessage(HttpMethod.Post, inputOptions.RequestUri)
                {
                    Content = new StringContent(GenerateSsml(inputOptions.Locale, genderValue, inputOptions.VoiceName, translationResut.TranslatedText))
                };

                var responseMessage = await client.SendAsync(request);
                Console.WriteLine("Response status code: [{0}]", responseMessage.StatusCode);

                try
                {
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var bytes = await responseMessage.Content.ReadAsByteArrayAsync();
                        translationResut.AudioResult = bytes;
                        this.AudioAvailable(new SynthesizerEventArgs<TranslationResult>(translationResut));
                    }
                    else
                    {
                        this.Error(new SynthesizerEventArgs<Exception>(new Exception(String.Format("Service returned {0}", responseMessage.StatusCode))));
                    }
                }
                catch (Exception e)
                {
                    this.Error(new SynthesizerEventArgs<Exception>(e.GetBaseException()));
                }
                finally
                {
                    responseMessage.Dispose();
                    request.Dispose();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Called when a TTS requst has been successfully completed and audio is available.
        /// </summary>
        private void AudioAvailable(SynthesizerEventArgs<TranslationResult> e)
        {
            EventHandler<SynthesizerEventArgs<TranslationResult>> handler = this.OnAudioAvailable;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Error handler function
        /// </summary>
        /// <param name="e">The exception</param>
        private void Error(SynthesizerEventArgs<Exception> e)
        {
            EventHandler<SynthesizerEventArgs<Exception>> handler = this.OnError;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
