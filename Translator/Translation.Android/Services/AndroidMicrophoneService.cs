
using Android;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using Google.Android.Material.Snackbar;
using Plugin.CurrentActivity;
using System.Threading.Tasks;
using Translation.Droid.Services;
using Translation.Interface;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidMicrophoneService))]
namespace Translation.Droid.Services
{
    public class AndroidMicrophoneService : IMicrophoneService
    {
        public const int RecordAudioPermissionCode = 1;
        private TaskCompletionSource<bool> tcsPermissions;
        string[] permissions = new string[] { Manifest.Permission.RecordAudio };

        public Task<bool> GetPermissionAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();

            if ((int)Build.VERSION.SdkInt < 23)
            {
                tcsPermissions.TrySetResult(true);
            }
            else
            {
                var currentActivity = CrossCurrentActivity.Current.Activity;
                if (ActivityCompat.CheckSelfPermission(currentActivity, Manifest.Permission.RecordAudio) != (int)Permission.Granted)
                {
                    RequestMicPermissions();
                }
                else
                {
                    tcsPermissions.TrySetResult(true);
                }

            }

            return tcsPermissions.Task;
        }

        public void OnRequestPermissionResult(bool isGranted)
        {
            tcsPermissions.TrySetResult(isGranted);
        }

        void RequestMicPermissions()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(CrossCurrentActivity.Current.Activity, Manifest.Permission.RecordAudio))
            {
                Snackbar.Make(CrossCurrentActivity.Current.Activity.FindViewById(Android.Resource.Id.Content),
                        "Microphone permissions are required for speech transcription.",
                        Snackbar.LengthIndefinite)
                        .SetAction("Ok", v =>
                        {
                            (CrossCurrentActivity.Current.Activity).RequestPermissions(permissions, RecordAudioPermissionCode);
                        })
                        .Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(CrossCurrentActivity.Current.Activity, permissions, RecordAudioPermissionCode);
            }
        }

        public void MuteMicrophone()
        {
            AudioManager.FromContext(CrossCurrentActivity.Current.AppContext).MicrophoneMute = true;
        }

        public void UnMuteMicrophone()
        {
            AudioManager.FromContext(CrossCurrentActivity.Current.AppContext).MicrophoneMute = false;
        }
    }
}