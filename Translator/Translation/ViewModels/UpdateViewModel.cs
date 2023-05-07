using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.Models;
using Translation.Utils;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class UpdateViewModel : BaseViewModel
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _description1;
        public string Description1
        {
            get { return _description1; }
            set
            {
                _description1 = value;
                OnPropertyChanged();
            }
        }

        private string _description2;
        public string Description2
        {
            get { return _description2; }
            set
            {
                _description2 = value;
                OnPropertyChanged();
            }
        }

        private ObservableRangeCollection<string> _releaseNotes;
        public ObservableRangeCollection<string> ReleaseNotes
        {
            get { return _releaseNotes; }
            set
            {
                _releaseNotes = value;
                OnPropertyChanged();
                if (ReleaseNotes.Any())
                {
                    ReleaseNotesVisible = true;
                }
            }
        }

        private bool _releaseNotesVisible;
        public bool ReleaseNotesVisible
        {
            get { return _releaseNotesVisible; }
            set
            {
                _releaseNotesVisible = value;
                OnPropertyChanged();
            }
        }

        private DateTime _releaseDate;
        public DateTime ReleaseDate
        {
            get { return _releaseDate; }
            set
            {
                _releaseDate = value;
                OnPropertyChanged();
            }
        }

        public UpdateViewModel()
        {
            MessagingCenter.Subscribe<AppVersion>(this, "ShowAppVersionAlertMessage", (sender) =>
            {
                UpdateAlertMessage(sender);
            });
        }

        public void UpdateAlertMessage(AppVersion appVersion)
        {
            if (appVersion.IsUnsurpotedVersion)
            {
                Title = "Unsupported Version";
                Description1 = "Critical updates are required for Tala to function. Features may stop working without notice. Please update the application.";
                Description2 = "Update your app to continue getting feature and security updates.";
            }
            else
            {
                Title = "New App Version";
                Description1 = "Install new app version of Tala.";
                Description2 = "New features and bug fixes will be missing from Tala and your version of Speechly may become unsupported.";
            }

            if (appVersion.ReleaseNotesList.Any())
            {
                ReleaseDate = appVersion.ReleaseDate;
                ReleaseNotes = new ObservableRangeCollection<string>(appVersion.ReleaseNotesList);
            }
        }

        private async Task Dismiss()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private async Task Update()
        {
            await Dismiss();
            await Dialogs.OpenBrowser("https://play.google.com/store/apps/details?id=com.fitts.speechly");
        }

        private async Task ForceUpdate()
        {
            await Dialogs.OpenBrowser("https://play.google.com/store/apps/details?id=com.fitts.speechly");
        }

        ICommand _dismissCommand = null;
        public ICommand DismissCommand
        {
            get
            {
                return _dismissCommand ?? (_dismissCommand =
                                          new Command(async () => await Dismiss()));
            }
        }

        ICommand _updateCommand = null;
        public ICommand UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand =
                                          new Command(async () => await Update()));
            }
        }

        ICommand _forceUpdateCommand = null;
        public ICommand ForceUpdateCommand
        {
            get
            {
                return _forceUpdateCommand ?? (_forceUpdateCommand =
                                          new Command(async () => await ForceUpdate()));
            }
        }
    }
}
