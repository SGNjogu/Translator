using System.Threading.Tasks;

namespace Translation.Core.Interfaces
{
    public interface IMicrosoftTextToTextTranslator
    {
        Task<string> TranslateTextToText(string apiKey, string apiRegion, string sourceLanguageCode, string textToTranslate, string targetLanguageCode);
    }
}
