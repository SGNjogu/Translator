using Translation.Messages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.Auth
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginFailedPage : ContentPage
    {
        public LoginFailedPage()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<LoginErrorMessage>(this, "LoginErrorMessage", (sender) =>
            {
                InitializeErrorMessages(sender);
            });
        }

        private void InitializeErrorMessages(LoginErrorMessage loginErrorMessage)
        {
            errorMessage.Text = loginErrorMessage.ErrorMessage;
            errorActionMessage.Text = loginErrorMessage.ErrorActionMessage;
        }
    }
}