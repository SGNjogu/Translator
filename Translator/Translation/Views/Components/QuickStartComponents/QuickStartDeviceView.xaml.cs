
using Translation.Core.Domain;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Components.QuickStartComponents
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuickStartDeviceView : ContentView
    {
        public QuickStartDeviceView()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var selectedSession = e.Item as AudioDevice;
            MessagingCenter.Instance.Send(selectedSession, "QuickStartSelectedAudioDevice");
        }
    }
}