using System.Threading.Tasks;

namespace Translation.Interface
{
    public interface IMicrophoneService
    {
        Task<bool> GetPermissionAsync();
        void OnRequestPermissionResult(bool isGranted);
        void MuteMicrophone();
        void UnMuteMicrophone();
    }
}
