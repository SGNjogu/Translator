using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Core.Events;

namespace Translation.Core.Interfaces
{
    public interface IAudioRecorder
    {
        event OnDataAvailable DataAvailable;
        bool IsRecording { get; set; }
        Task StartRecording(InputDevice inputDevice);
        void StopRecording();
    }
}
