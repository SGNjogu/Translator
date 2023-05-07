using System.Threading.Tasks;
using Translation.Interface;
using Xamarin.Forms;

namespace Translation.Utils
{
    public static class PermissionsHelper
    {
        public static async Task AskForPermissions()
        {
            var action = await Application.Current.MainPage.DisplayAlert($"\"{Constants.AppName}\" would like to access your Microphone",
            $"{Constants.AppName} would like to access your microphone to translate speech. \nAllow in settings?", "Yes", "No");
            if (action == true)
            {
                DependencyService.Get<IAppSetting>().OpenAppSettings();
            }
        }

        public static async Task AskForStoragePermissions()
        {
            var action = await Application.Current.MainPage.DisplayAlert($"\"{Constants.AppName}\" would like to access your Storage",
            $"{Constants.AppName} would like to access your storage to save files externally e.g to Downloads folder. \nAllow in settings?", "Yes", "No");
            if (action == true)
            {
                DependencyService.Get<IAppSetting>().OpenAppSettings();
            }
        }
    }
}
