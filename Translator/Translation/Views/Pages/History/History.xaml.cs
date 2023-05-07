using Translation.DataService.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.History
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class History : ContentView
    {
        public History()
        {
            InitializeComponent();
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedSession = e.Item as Session;
            await Application.Current.MainPage.Navigation.PushAsync(new TranscriptionsHistory());
            MessagingCenter.Instance.Send(selectedSession, "SelectedSession");
        }
    }
}