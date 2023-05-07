
using Translation.Messages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Translation.Views.Pages.AppShell
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private bool _isTranslating;
        public MainPage()
        {
            InitializeComponent();
            _isTranslating = false;
            fontResizer.IsVisible = false;
            organizationQuestions.IsVisible = false;
            quickStartBtn.IsVisible = true;
            MessagingCenter.Subscribe<FontResizerMessage>(this, "FontResizerMessage", (sender) =>
            {
                HandleFontResizerVisibility(sender);
            });

            MessagingCenter.Subscribe<FontResizerMessage>(this, "QuickStartBtnMessage", (sender) =>
            {
                HandleQuickStartVisibility(sender);
            });
        }

        private void HandleQuickStartVisibility(FontResizerMessage sender)
        {
            if (!_isTranslating && sender.ShowFontResizer)
            {
                quickStartBtn.IsVisible = true;
            }
            else
            {
                quickStartBtn.IsVisible = false;
            }
        }

        private void HandleFontResizerVisibility(FontResizerMessage sender)
        {
            _isTranslating = sender.ShowFontResizer;

            if (_isTranslating)
            {
                quickStartBtn.IsVisible = false;
                fontResizer.IsVisible = true;
                organizationQuestions.IsVisible = sender.ShowOrganizationQuestions;
            }
            else
            {
                fontResizer.IsVisible = false;
                organizationQuestions.IsVisible = false;
                quickStartBtn.IsVisible = true;
            }
        }
    }
}