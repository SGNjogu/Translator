using Translation.DataService.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Components.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrganizationQuestions
    {
        public OrganizationQuestions()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            var selectedQuestion = e.Item as UserQuestions;

            if (selectedQuestion != null)
            {
                MessagingCenter.Instance.Send(selectedQuestion, "SelectOrganizationQuestion");
            }
        }
    }
}