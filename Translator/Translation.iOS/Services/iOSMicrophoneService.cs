using AVFoundation;
using OpenTK.Audio;
using System.Threading.Tasks;
using Translation.Interface;
using Translation.iOS.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(iOSMicrophoneService))]
namespace Translation.iOS.Services
{
    public class iOSMicrophoneService : IMicrophoneService
    {
        TaskCompletionSource<bool> tcsPermissions;

        public Task<bool> GetPermissionAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();
            RequestMicPermission();
            return tcsPermissions.Task;
        }

        public void OnRequestPermissionResult(bool isGranted)
        {
            tcsPermissions.TrySetResult(isGranted);
        }

        void RequestMicPermission()
        {
            var session = AVAudioSession.SharedInstance();
            session.RequestRecordPermission((granted) =>
            {
                tcsPermissions.TrySetResult(granted);
            });
        }

        public void MuteMicrophone()
        {
            var session = AVAudioSession.SharedInstance();
            session.SetActive(false);
        }

        public void UnMuteMicrophone()
        {
            var session = AVAudioSession.SharedInstance();
            session.SetActive(true);
        }
    }
}