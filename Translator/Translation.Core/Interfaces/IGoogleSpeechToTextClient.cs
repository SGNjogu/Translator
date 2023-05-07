using System;
using System.Threading.Tasks;
using Translation.Core.Events;

namespace Translation.Core.Interfaces
{
    public interface IGoogleSpeechToTextClient
    {
        event Action<GoogleSpeechToTextResponse> GoogleSpeechToTextOnTextAvailable;
        event AudioInputDataAvailable InputDataAvailable;
        Task StartSpeechToTextAsync();
        Task<bool> StopTranslationAsync();
        void PauseSpeechToTextClient();
    }
}
