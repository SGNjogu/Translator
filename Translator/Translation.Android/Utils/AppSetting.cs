using Android.Content;
using Translation.Droid.Utils;
using Translation.Interface;
using Xamarin.Forms;

[assembly: Dependency(typeof(AppSetting))]
namespace Translation.Droid.Utils
{
    public class AppSetting : IAppSetting
    {
        public void OpenAppSettings()
        {
            var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            string package_name = Constants.AndroidPackageName;
            var uri = Android.Net.Uri.FromParts("package", package_name, null);
            intent.SetData(uri);
            Android.App.Application.Context.StartActivity(intent);
        }
    }
}