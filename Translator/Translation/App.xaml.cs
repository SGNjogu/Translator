using Translation.AppSettings;
using Translation.Auth;
using Translation.Interface;
using Translation.Styles;
using Translation.Views.Pages.AppShell;
using Translation.Views.Pages.Auth;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: ExportFont("fa-brands.ttf", Alias = "FontAwesomeBrands")]
[assembly: ExportFont("fa-solid.ttf", Alias = "FontAwesomeSolid")]
[assembly: ExportFont("fa-regular.ttf", Alias = "FontAwesomeRegular")]
[assembly: ExportFont("fa-light.ttf", Alias = "FontAwesomeLight")]
[assembly: ExportFont("lato-regular.ttf", Alias = "LatoRegular")]
[assembly: ExportFont("lato-bold.ttf", Alias = "LatoBold")]

namespace Translation
{
    public partial class App : Application
    {
        public static UserContext CurrentUser = null;

        public App()
        {
            InitializeComponent();

            // Initialize Sharpnardo Tabs
            Sharpnado.Tabs.Initializer.Initialize(false, false);
            Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);

            if (Settings.IsUserLoggedIn())
            {
                MainPage = new NavigationPage(new MainPage());
            }
            else
            {
                MainPage = new NavigationPage(new WelcomeScreen());
            }
        }

        protected async override void OnStart()
        {
            DependencyService.Get<IMicrophoneService>().UnMuteMicrophone();

            // Get Currently logged in user
            CurrentUser = await Settings.CurrentUser();

            // Get App Theme
            ThemeHelper.GetAppTheme();

            // Initiate version tracking
            VersionTracking.Track();
        }

        protected override void OnSleep()
        {
            DependencyService.Get<IMicrophoneService>().UnMuteMicrophone();
        }

        protected override void OnResume()
        {
            DependencyService.Get<IMicrophoneService>().UnMuteMicrophone();
        }
    }
}
