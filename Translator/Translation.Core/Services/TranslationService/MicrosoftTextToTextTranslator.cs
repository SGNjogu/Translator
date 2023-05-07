using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Translation.Core.Interfaces;

namespace Translation.Core.Services.TranslationService
{
    public class MicrosoftTextToTextTranslator : IMicrosoftTextToTextTranslator
    {
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";

        public async Task<string> TranslateTextToText(string apiKey, string apiRegion, string sourceLanguageCode, string textToTranslate, string targetLanguageCode)
        {
            string result = string.Empty;
            try
            {
                string route = $"/translate?api-version=3.0&from={sourceLanguageCode}&to={targetLanguageCode}";
                object[] body = new object[] { new { Text = textToTranslate } };
                var requestBody = JsonConvert.SerializeObject(body);

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", apiRegion);

                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);

                    var interimresult = await response.Content.ReadAsStringAsync();

                    var resultList = JsonConvert.DeserializeObject<List<Transcriptions>>(interimresult);

                    //Return the result as a string
                    result = resultList[0].translations[0].text;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return result;
        }
    }

    public class Transcriptions
    {
        public List<Translation> translations { get; set; }
    }

    public class Translation
    {
        public string text { get; set; }
        public string to { get; set; }
    }
}
