using System.Linq;
using dotMorten.Xamarin.Forms;
using MvvmHelpers;
using Translation.Interface;
using Translation.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Components.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SessionMetaDataPopup
    {
        private SessionMetaDataViewModel _bindingContext;

        public SessionMetaDataPopup()
        {
            InitializeComponent();
            _bindingContext = BindingContext as SessionMetaDataViewModel;
        }

        protected override void OnAppearing()
        {
            sessionName.Focus();
            _bindingContext.ClearTags();
            DependencyService.Get<IBackButtonService>().DisableBackNavigation();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DependencyService.Get<IBackButtonService>().EnableBackNavigation();
            if (_bindingContext.IsEnabled == false)
            {
                MessagingCenter.Instance.Send("", "SaveSession");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return _bindingContext.IsEnabled;
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string suggestion = sender.Text;
                customTag.ItemsSource = GetSuggestions(suggestion);

                if (customTag.Text.Length >= 40)
                {
                    customTag.Text = customTag.Text.Substring(0, 40);
                }
            }
        }


        private void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // Set sender.Text. You can use args.SelectedItem to build your text string.
        }


        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
{
                customTag.Text = args.ChosenSuggestion.ToString();
                _bindingContext.CustomTags.Add(customTag.Text);
                customTag.Text = string.Empty;
                customTag.ItemsSource = GetSuggestions(string.Empty);
            }
            else
            {
                _bindingContext.CustomTags.Add(customTag.Text);
                customTag.Text = string.Empty;
                customTag.ItemsSource = GetSuggestions(string.Empty);
            }
        }

        private ObservableRangeCollection<string> GetSuggestions(string text)
        {
            ObservableRangeCollection<string> suggestions = new ObservableRangeCollection<string>();

            var organizationCustomTagsList = _bindingContext.OrganizationCustomTags.ToList();
            if (organizationCustomTagsList.Count() > 0 && !string.IsNullOrEmpty(text))
            {
                var filteredList = organizationCustomTagsList.FindAll(t => t.ToLower().Contains(text.ToLower()));
                if (filteredList.Count() > 0)
                    suggestions = new ObservableRangeCollection<string>(filteredList);
            }

            return suggestions;
        }
    }
}