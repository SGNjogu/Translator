using Translation.Interface;
using Translation.iOS.Utils;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(StatusBarUtility))]
namespace Translation.iOS.Utils
{
    public class StatusBarUtility : IStatusBar
    {
        private static bool IsDefaultStatusBar = false;

        public void ChangeStatusBarColorToBlack()
        {
            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
            IsDefaultStatusBar = true;
        }

        public void ChangeStatusBarColorToWhite()
        {
            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            IsDefaultStatusBar = false;
        }

        public void HideStatusBar()
        {
            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackTranslucent;
        }

        public void ShowStatusBar()
        {
            if(IsDefaultStatusBar)
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.Default;
            else
                UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
        }
    }
}