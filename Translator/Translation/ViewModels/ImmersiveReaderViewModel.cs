using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class ImmersiveReaderViewModel : MvvmHelpers.BaseViewModel
    {
        private Uri _webViewSource;
        public Uri WebViewSource
        {
            get { return _webViewSource; }
            set
            {
                _webViewSource = value;
                OnPropertyChanged();
            }
        }

        public ImmersiveReaderViewModel()
        {
            MessagingCenter.Subscribe<Uri>(this, "WebViewSource", (sender) =>
            {
                ImmersiveRead(sender);
            });
        }

        void ImmersiveRead(Uri source)
        {
            WebViewSource = source;
        }

        private async Task Close()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        /// <summary>
        /// Command to close page
        /// </summary>
        ICommand _closeCommand = null;

        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand =
                                          new Command(async () => await Close()));
            }
        }
    }
}
