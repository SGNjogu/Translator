
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.Help
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class About : ContentPage
    {
        public About()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }
    }
}