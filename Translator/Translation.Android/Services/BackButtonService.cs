using Translation.Droid.Services;
using Translation.Interface;
using Xamarin.Forms;

[assembly: Dependency(typeof(BackButtonService))]
namespace Translation.Droid.Services
{
    public class BackButtonService : IBackButtonService
    {
        public static bool CanGoBack { get; set; } = true;

        public void DisableBackNavigation()
        {
            CanGoBack = false;
        }

        public void EnableBackNavigation()
        {
            CanGoBack = true;
        }
    }
}