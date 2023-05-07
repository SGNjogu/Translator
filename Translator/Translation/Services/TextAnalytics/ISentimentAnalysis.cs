using System.Threading.Tasks;

namespace Translation.TextAnalytics
{
    public interface ISentimentAnalysis
    {
        Task<string> GetSentiment(string inputText);
        string SentimentEmoji(string sentiment);
    }
}
