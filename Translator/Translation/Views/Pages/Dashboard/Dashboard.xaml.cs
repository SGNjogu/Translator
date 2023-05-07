
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Dashboard : ContentView
    {
        public Dashboard()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<string>(this, "HideTabBar", (sender) =>
            {
                HideTabBar();
            });
            MessagingCenter.Subscribe<string>(this, "ShowTabBar", (sender) =>
            {
                ShowTabBar();
            });

            // TitleView = Shell.GetTitleView(this);

            MessagingCenter.Subscribe<Models.TranslationResultText>(this, "ScrollToMessage", (sender) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                  listview.ScrollTo(sender, ScrollToPosition.Start, false)
              );
            });
        }

        void HideTabBar()
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                Shell.SetTabBarIsVisible(this, false);
                Shell.SetNavBarIsVisible(this, false);
            });
        }

        void ShowTabBar()
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                Shell.SetTabBarIsVisible(this, true);
                Shell.SetNavBarIsVisible(this, true);
            });
        }
    }
}