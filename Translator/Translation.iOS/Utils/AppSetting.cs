
using Foundation;
using Translation.Interface;
using Translation.iOS.Utils;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppSetting))]
namespace Translation.iOS.Utils
{
    public class AppSetting : IAppSetting
    {
        public void OpenAppSettings()
        {
            var url = new NSUrl($"app-settings:");
            UIApplication.SharedApplication.OpenUrl(url);
        }
    }
}