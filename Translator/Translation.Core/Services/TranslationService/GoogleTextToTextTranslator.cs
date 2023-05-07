using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Translation.Core.Events;
using Translation.Core.Interfaces;

namespace Translation.Core.Services.TranslationService
{
    public class GoogleTextToTextTranslator : IGoogleTextToTextTranslator
    {
        private TranslationServiceClient _translationServiceClient { get; set; }

        public event Action<GoogleTextTranslationResponse> GoogleTextTranslationOnTextAvailable;

        private string _jsonCredentials;
        private string _projectId;

        public GoogleTextToTextTranslator(string jsonCredentials)
        {
            _jsonCredentials = jsonCredentials ?? throw new ArgumentNullException(nameof(jsonCredentials));
        }

        public async Task Initialize()
        {
            try
            {
                var credentialsJson = JObject.Parse(_jsonCredentials);
                _projectId = credentialsJson["project_id"].ToString();

                TranslationServiceClientBuilder translationServiceClient = new TranslationServiceClientBuilder
                {
                    JsonCredentials = _jsonCredentials
                };

                _translationServiceClient = await translationServiceClient.BuildAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task TranslateAsync
            (
            string originalText,
            string sourceLanguge,
            string targetLanguage,
            long OffsetInTicks,
            TimeSpan duration
            )
        {
            try
            {
                TranslateTextRequest request = new TranslateTextRequest
                {
                    Contents =
                    {
                        // The content to translate.
                        originalText,
                    },
                    TargetLanguageCode = targetLanguage,
                    SourceLanguageCode = sourceLanguge,
                    Parent = new ProjectName(_projectId).ToString()
                };

                TranslateTextResponse response = await _translationServiceClient.TranslateTextAsync(request);
                // Display the translation for each input text provided
                //foreach (Translation translation in response.Translations)
                //{
                //    Console.WriteLine($"Translated text: {translation.TranslatedText}");
                //}

                string translatedText = response.Translations[0].TranslatedText;

                GoogleTextTranslationOnTextAvailable?.Invoke(new GoogleTextTranslationResponse { OriginalText = originalText, TranslatedText = translatedText, Duration = duration, OffsetInTicks = OffsetInTicks });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
