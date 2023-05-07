using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Translation.TextAnalytics
{
    public class SentimentAnalysis : ISentimentAnalysis
    {
        private TextAnalyticsClient Client;
        private readonly AzureKeyCredential Credentials;
        private readonly Uri Endpoint;

        public SentimentAnalysis()
        {
            Endpoint = new Uri(Constants.TextAnalyticsAPIEndpoint);
            Credentials = new AzureKeyCredential(Constants.TextAnalyticsAzureKey);
            Client = new TextAnalyticsClient(Endpoint, Credentials);
        }

        public async Task<string> GetSentiment(string inputText)
        {
            try
            {
                DocumentSentiment documentSentiment = await Client.AnalyzeSentimentAsync(inputText);

                return documentSentiment.Sentences.ToList()[0].Sentiment.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public string SentimentEmoji(string sentiment)
        {
            switch (sentiment)
            {
                case "Positive":
                    return Utils.ImageUtility.ReturnImageSourceFromFile("smiling.png");
                case "Negative":
                    return Utils.ImageUtility.ReturnImageSourceFromFile("negative.png");
                case "Neutral":
                    return Utils.ImageUtility.ReturnImageSourceFromFile("neutral.png");
                default:
                    return Utils.ImageUtility.ReturnImageSourceFromFile("neutral.png");
            }
        }
    }
}
