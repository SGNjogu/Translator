using Microsoft.AppCenter.Crashes;
using System.Threading.Tasks;
using Translation.AppSettings;
using Translation.Interface;
using Xamarin.Essentials;

namespace Translation.AuditTracking
{
    public class AppCrashlytics : IAppCrashlytics
    {
        public async Task<ErrorAttachmentLog[]> Attachments()
        {
            var environment = "production";

#if DEBUG
            environment = "development";
#endif

            var appVersion = AppInfo.VersionString;
            var isLoggedIn = Settings.IsUserLoggedIn();

            if(isLoggedIn && App.CurrentUser == null)
            {
                App.CurrentUser = await Settings.CurrentUser();
            }

            if (isLoggedIn)
            {
                var userId = App.CurrentUser.UserId;
                var userName = App.CurrentUser.Name;
                var email = App.CurrentUser.Email;

                return new ErrorAttachmentLog[]
                {
                    ErrorAttachmentLog.AttachmentWithText(
                        $"Username: {userName} \n" +
                        $"UserId: {userId} \n" +
                        $"Email: {email} \n" +
                        $"AppVersion: {appVersion} \n" +
                        $"Environment: {environment} \n", "Details.txt")
                };
            }

            return new ErrorAttachmentLog[]
            {
                ErrorAttachmentLog.AttachmentWithText(
                        $"AppVersion: {appVersion} \n" +
                        $"Environment: {environment}", "Details.txt")
            };
        }
    }
}
