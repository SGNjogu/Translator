
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.History
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TranscriptionsHistory : ContentPage
    {
        public TranscriptionsHistory()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }

        protected override void OnDisappearing()
        {
            MessagingCenter.Instance.Send("Unload", "Unload");
        }
    }
}