using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.AppSettings;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.Interface;
using Translation.Services.Auth;
using Translation.Utils;
using Translation.Views.Pages.Auth;
using Translation.Views.Pages.Help;
using Xamarin.Essentials;
using Xamarin.Forms;
using BaseViewModel = MvvmHelpers.BaseViewModel;

namespace Translation.ViewModels
{
    public class HelpViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// App Version
        /// </summary>
        private string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ResellerName
        /// </summary>
        private string _resellerName;
        public string ResellerName
        {
            get { return _resellerName; }
            set
            {
                _resellerName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ResellerEmail
        /// </summary>
        private string _resellerEmail;
        public string ResellerEmail
        {
            get { return _resellerEmail; }
            set
            {
                _resellerEmail = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// App Build
        /// </summary>
        private string _build;
        public string Build
        {
            get { return _build; }
            set
            {
                _build = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True if can store user data
        /// </summary>
        private string _dataConsentInfo = "...";

        public string DataConsentInfo
        {
            get { return _dataConsentInfo; }
            set
            {
                _dataConsentInfo = value;
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

        private ObservableRangeCollection<string> _releaseNotes;
        public ObservableRangeCollection<string> ReleaseNotes
        {
            get { return _releaseNotes; }
            set
            {
                _releaseNotes = value;
                OnPropertyChanged();
            }
        }

        IDataService Dataservice;
        IAppAnalytics AppAnalytics;
        IAppCrashlytics AppCrashlytics;

        #endregion

        #region Constructor

        public HelpViewModel(IDataService dataService, IAppAnalytics appAnalytics, IAppCrashlytics appCrashlytics)
        {
            Dataservice = dataService;
            AppAnalytics = appAnalytics;
            AppCrashlytics = appCrashlytics;

            AppAnalytics.CaptureCustomEvent("HelpPage Navigated");
            GetAppInfo();
            _ = GetResellerInfo();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to get Application Information
        /// </summary>
        async void GetAppInfo()
        {
            // Application Version (1.0.0)
            Version = AppInfo.VersionString;

            // Application Build Number (1)
            Build = AppInfo.BuildString;

            var currentUser = await Settings.CurrentUser();

            bool consent = currentUser != null ? currentUser.DataConsentStatus : false;

            if (consent)
                DataConsentInfo = "Your transcription data is saved in the cloud.";
            else
                DataConsentInfo = "Your transcription data is not saved in the cloud.";

            var releaseNotes = await Dataservice.GetReleaseNotes();

            if (releaseNotes.Any())
            {
                ReleaseNotes = new ObservableRangeCollection<string>(releaseNotes.Select(s => s.Note));
                ReleaseDate = releaseNotes.FirstOrDefault().DateReleased;
            }
        }

        private async Task GetResellerInfo()
        {
            var currentUser = await AppSettings.Settings.CurrentUser();
            if (currentUser == null)
                return;

            ResellerName = "N/A";
            ResellerEmail = "N/A";

            if (!string.IsNullOrEmpty(currentUser.ResellerEmail))
                ResellerEmail = currentUser.ResellerEmail;

            if (!string.IsNullOrEmpty(currentUser.ResellerName))
                ResellerName = currentUser.ResellerName;
        }

        /// <summary>
        /// Method to send email feedback
        /// </summary>
        async Task SendFeedback()
        {
            Dialogs.ProgressDialog.Show();
            AppAnalytics.CaptureCustomEvent("Help Section Events",
                 new Dictionary<string, string>
                  {
                         {"User", App.CurrentUser?.Email},
                         {"Organization", App.CurrentUser?.Organization },
                         {"Action", "Send feedback navigation" }

                  });
            await Dialogs.OpenBrowser("https://forms.office.com/Pages/ResponsePage.aspx?id=d14g-cUG-U-gVJe_Gj00dBGhqSccpxVFrAal71GS1VdURFdNN0JUTjRLQVExVDZHQlJYVzRZNlRHQS4u");

            Dialogs.ProgressDialog.Hide();
        }

        /// <summary>
        /// Method open terms of use
        /// </summary>
        async Task TermsOfUse()
        {
            Dialogs.ProgressDialog.Show();
            AppAnalytics.CaptureCustomEvent("Help Section Events",
                  new Dictionary<string, string>
                   {
                         {"User", App.CurrentUser?.Email},
                         {"Organization", App.CurrentUser?.Organization },
                         {"Action", "Terms of use navigation" }

                   });
            await Dialogs.OpenBrowser("https://tala.global/terms-of-service/");

            Dialogs.ProgressDialog.Hide();
        }

        /// <summary>
        /// Method to navigate to about Page
        /// </summary>
        async Task GoToAbout()
        {
            AppAnalytics.CaptureCustomEvent("Help Section Events",
                   new Dictionary<string, string>
                    {
                         {"User", App.CurrentUser?.Email},
                         {"Organization", App.CurrentUser?.Organization },
                         {"Action", "About Navigation" }

                    });
            await Application.Current.MainPage.Navigation.PushAsync(new About());
        }

        /// <summary>
        /// Method to navigate to sgn website
        /// </summary>
        async Task GoToSite(string sgnUrl)
        {
            await Dialogs.OpenBrowser(sgnUrl);
        }

        /// <summary>
        /// Method to navigate to Features
        /// </summary>
        async Task GoToFeatures()
        {

            await Application.Current.MainPage.Navigation.PushAsync(new Features());
            AppAnalytics.CaptureCustomEvent("Help Section Events",
                    new Dictionary<string, string>
                     {
                         {"User", App.CurrentUser?.Email},
                         {"Organization", App.CurrentUser?.Organization },
                         {"Action", "Whats new Navigation" }

                     });
        }

        /// <summary>
        /// Method to navigate to reseller info page
        /// </summary>
        async Task GoToResellerInfo()
        {
            AppAnalytics.CaptureCustomEvent("Help Section Events",
                   new Dictionary<string, string>
                    {
                         {"User", App.CurrentUser?.Email},
                         {"Organization", App.CurrentUser?.Organization },
                         {"Action", "Reseller Info Navigation" }

                    });

            await Application.Current.MainPage.Navigation.PushAsync(new ResellerInfo());
        }

        /// <summary>
        /// Method to sign out user
        /// </summary>
        async Task Logout()
        {
            try
            {
                var signoutConfirmed = await UserDialogs.Instance.ConfirmAsync("Are you sure you want to sign out?", "Sign Out", "Confirm", "Cancel");

                if (signoutConfirmed)
                {
                    Dialogs.ProgressDialog.Show();

                    Analytics.TrackEvent("User Logged Out",
                        new Dictionary<string, string> {
                    { "User", App.CurrentUser.Name },
                    { "UserId", App.CurrentUser.UserId },
                    { "Time", DateTime.UtcNow.ToString() } });

                    App.CurrentUser = null;

                    Settings.ClearSettings();
                    Settings.RemoveAllSecureSettings();
                    await Dataservice.DeleteAllItemsAsync<Session>();
                    await Dataservice.DeleteAllItemsAsync<Transcription>();

                    await ADB2CAuthenticationService.Instance.SignOutAsync();

                    Application.Current.MainPage = new NavigationPage(new LoginPage());
                    await Application.Current.MainPage.Navigation.PopToRootAsync();

                    Dialogs.ProgressDialog.Hide();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await AppCrashlytics.Attachments());
            }
        }
        /// <summary>
        /// Command to initiate send feedback
        /// </summary>
        ICommand _sendFeedbackCommand = null;

        public ICommand SendFeedbackCommand
        {
            get
            {
                return _sendFeedbackCommand ?? (_sendFeedbackCommand =
                                          new Command(async () => await SendFeedback()));
            }
        }

        /// <summary>
        /// Command to initiate send feedback
        /// </summary>
        ICommand _goToAboutCommand = null;

        public ICommand GoToAboutCommand
        {
            get
            {
                return _goToAboutCommand ?? (_goToAboutCommand =
                                          new Command(async () => await GoToAbout()));
            }
        }


        /// <summary>
        /// Command to open fitts website
        /// </summary>
        ICommand _goToSiteCommand = null;

        public ICommand GoToSiteCommand
        {
            get
            {
                return _goToSiteCommand ?? (_goToSiteCommand =
                                          new Command(async (object obj) => await GoToSite((string)obj)));
            }
        }

        /// <summary>
        /// Command to logout user
        /// </summary>
        ICommand _termsOfUseCommand = null;

        public ICommand TermsOfUseCommand
        {
            get
            {
                return _termsOfUseCommand ?? (_termsOfUseCommand = new Command(async () => await TermsOfUse()));
            }
        }

        /// <summary>
        /// Command to initiate send feedback
        /// </summary>
        ICommand _goToFeaturesCommand = null;

        public ICommand GoToFeaturesCommand
        {
            get
            {
                return _goToFeaturesCommand ?? (_goToFeaturesCommand =
                                          new Command(async (object obj) => await GoToFeatures()));
            }
        }

        /// <summary>
        /// Command to logout user
        /// </summary>
        ICommand _logoutCommand = null;

        public ICommand LogoutCommand
        {
            get
            {
                return _logoutCommand ?? (_logoutCommand = new Command(async () => await Logout()));
            }
        }

        /// <summary>
        /// Command to view reseller information
        /// </summary>
        ICommand _resellerInfoCommand = null;

        public ICommand ResellerInfoCommand
        {
            get
            {
                return _resellerInfoCommand ?? (_resellerInfoCommand = new Command(async () => await GoToResellerInfo()));
            }
        }

        #endregion
    }
}
