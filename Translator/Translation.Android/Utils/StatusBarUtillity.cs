using Android.OS;
using Android.Views;
using Plugin.CurrentActivity;
using Translation.Droid.Utils;
using Translation.Interface;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(StatusBarUtillity))]
namespace Translation.Droid.Utils
{
    public class StatusBarUtillity : IStatusBar
    {
        private static bool IsDefaultStatusBar = false;

        public void ChangeStatusBarColorToBlack()
        {
            if(Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                MainThread.BeginInvokeOnMainThread(() => 
                {
                    var currentWindow = GetCurrentWindow();
                    currentWindow.DecorView.SystemUiVisibility = StatusBarVisibility.Visible;
                    currentWindow.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#121212"));
                    currentWindow.SetTitleColor(Android.Graphics.Color.White);
                    currentWindow.SetStatusBarColor(Android.Graphics.Color.Black);
                });
            }

            IsDefaultStatusBar = false;
        }

        public void ChangeStatusBarColorToWhite()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var currentWindow = GetCurrentWindow();
                currentWindow.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
                currentWindow.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#e0e0e0"));
                currentWindow.SetTitleColor(Android.Graphics.Color.Gray);
                currentWindow.SetStatusBarColor(Android.Graphics.Color.White);
            }

            IsDefaultStatusBar = true;
        }

        public void HideStatusBar()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var currentWindow = GetCurrentWindow();
                currentWindow.DecorView.SystemUiVisibility = StatusBarVisibility.Hidden;
                currentWindow.SetTitleColor(Android.Graphics.Color.Black);
                currentWindow.SetNavigationBarColor(Android.Graphics.Color.Black);
                currentWindow.SetStatusBarColor(Android.Graphics.Color.Black);
            }
        }

        public void ShowStatusBar()
        {
            if (IsDefaultStatusBar)
                ChangeStatusBarColorToWhite();
            else
                ChangeStatusBarColorToBlack();
        }

        Window GetCurrentWindow()
        {
            var window = CrossCurrentActivity.Current.Activity.Window;

            // clear FLAG_TRANSLUCENT_STATUS flag:
            window.ClearFlags(WindowManagerFlags.TranslucentStatus);

            // add FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS flag to the window
            window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            return window;
        }
    }
}