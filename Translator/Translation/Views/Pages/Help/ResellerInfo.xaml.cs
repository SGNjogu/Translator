
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.Help
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResellerInfo : ContentPage
    {
        public ResellerInfo()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }
    }
}