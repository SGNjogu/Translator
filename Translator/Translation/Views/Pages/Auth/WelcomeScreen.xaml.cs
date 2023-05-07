using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.Auth
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WelcomeScreen : ContentPage
    {
        public WelcomeScreen()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await AutoScroll();
        }

        async Task AutoScroll()
        {
            await Task.Delay(1000);
            carouselView.ScrollTo(carouselView.Position + 1);
            (BindingContext as ViewModels.WelcomeScreenViewModel).NextIntroItemCommand.Execute(null);
            await Task.Delay(1000);
            carouselView.ScrollTo(carouselView.Position - 1);
        }
    }
}