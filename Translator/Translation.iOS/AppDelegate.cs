
using Foundation;
using MediaManager;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using TouchEffect.iOS;
using UIKit;

namespace Translation.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // Initialize AppCenter
            AppCenter.Start(Constants.IOSAppCenterKey, typeof(Analytics), typeof(Crashes));

            // Experimental Flags
            Xamarin.Forms.Forms.SetFlags(new string[] { "Expander_Experimental", "IndicatorView_Experimental", "MediaElement_Experimental" });

            // Initialize Rg.Popups
            Rg.Plugins.Popup.Popup.Init();

            global::Xamarin.Forms.Forms.Init();

            // Initialize Sharpnado Tabs
            Sharpnado.Tabs.iOS.Preserver.Preserve();

            // Initialize TouchView
            TouchEffectPreserver.Preserve();

            //Prevent the device from going to sleep when the app is launched
            UIApplication.SharedApplication.IdleTimerDisabled = true;

            // Initialize TouchView (TouchEffect)
            TouchEffectPreserver.Preserve();

            // Initialize MediaManager
            CrossMediaManager.Current.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
            return true;
        }
    }
}
