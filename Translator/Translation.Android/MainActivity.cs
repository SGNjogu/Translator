
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MediaManager;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using Plugin.CurrentActivity;
using System;
using TouchEffect.Android;
using Translation.Auth;
using Translation.Droid.Services;
using Translation.Droid.Utils;
using Translation.Interface;
using Translation.Services.Download;
using Xamarin.Forms;

namespace Translation.Droid
{
    [Activity(Label = "Tala", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        IMicrophoneService MicService;
        IFileService FileService;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Initialize AppCenter
            AppCenter.Start(Constants.AndroidAppCenterKey, typeof(Analytics), typeof(Crashes));

            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            DependencyService.Register<IParentWindowLocatorService, AndroidParentWindowLocatorService>();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            // Experimental Flags
            Xamarin.Forms.Forms.SetFlags(new string[] { "Expander_Experimental", "IndicatorView_Experimental", "MediaElement_Experimental" });

            // Initialize Rg.Popups
            Rg.Plugins.Popup.Popup.Init(this);

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);

            // Initialize TouchView
            TouchEffectPreserver.Preserve();

            //Prevents the device from going to sleep when the application is launched
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            // Initialize UserDialogs
            UserDialogs.Init(() => this);

            // Initalize TouchView (TouchEffect)
            TouchEffectPreserver.Preserve();

            // Initialize MediaManager
            CrossMediaManager.Current.Init(this);

            LoadApplication(new App());

            MicService = DependencyService.Resolve<IMicrophoneService>();
            FileService = DependencyService.Resolve<IFileService>();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case AndroidMicrophoneService.RecordAudioPermissionCode:
                        if (grantResults[0] == Permission.Granted)
                        {
                            MicService.OnRequestPermissionResult(true);
                        }
                        else
                        {
                            MicService.OnRequestPermissionResult(false);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
        }

        public async override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
                if (BackButtonService.CanGoBack)
                {
                    await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopAsync();
                }
            }
        }
    }
}