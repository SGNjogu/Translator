using Android.App;
using Android.OS;
using Android.Runtime;
using Plugin.CurrentActivity;
using System;

namespace Translation.Droid
{
#if DEBUG
    [Application(Debuggable = true)]
#else
	[Application(Debuggable = false)]
#endif
    public class MainApplication : Application
    {
        Services.AndroidMicrophoneService androidMicrophoneService;

        public MainApplication(IntPtr handle, JniHandleOwnership transer) : base(handle, transer)
        {
            androidMicrophoneService = new Services.AndroidMicrophoneService();
        }

        public override void OnCreate()
        {
            base.OnCreate();

            CrossCurrentActivity.Current.Init(this);
        }

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {
            androidMicrophoneService.UnMuteMicrophone();
        }

        public void OnActivityPaused(Activity activity)
        {
            androidMicrophoneService.UnMuteMicrophone();
        }

        public void OnActivityResumed(Activity activity)
        {
            androidMicrophoneService.UnMuteMicrophone();
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
            androidMicrophoneService.UnMuteMicrophone();
        }

        public void OnActivityStarted(Activity activity)
        {
            androidMicrophoneService.UnMuteMicrophone();
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
            androidMicrophoneService.UnMuteMicrophone();
        }
    }
}