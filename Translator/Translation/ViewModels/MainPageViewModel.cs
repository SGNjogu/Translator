using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.AppSettings;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.DataSync.Interfaces;
using Translation.Helpers;
using Translation.Interface;
using Translation.Messages;
using Translation.Services.DataSync.Interfaces;
using Translation.Services.Usage;
using Translation.Services.Versioning;
using Translation.Utils;
using Translation.Views.Components.Popups;
using Translation.Views.Pages.Auth;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        private string _pageTitle;
        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
                OnPropertyChanged();
            }
        }

        private int _selectedMenuItem;
        public int SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set
            {
                _selectedMenuItem = value;
                OnPropertyChanged();
                OnMenuItemChanged(SelectedMenuItem);
            }
        }

        private DashboardViewModel _dashboardViewModel;
        public DashboardViewModel DashboardViewModel
        {
            get { return _dashboardViewModel; }
            set
            {
                _dashboardViewModel = value;
                OnPropertyChanged();
            }
        }

        private HistoryViewModel _historyViewModel;
        public HistoryViewModel HistoryViewModel
        {
            get { return _historyViewModel; }
            set
            {
                _historyViewModel = value;
                OnPropertyChanged();
            }
        }

        private SettingsViewModel _settingsViewModel;
        public SettingsViewModel SettingsViewModel
        {
            get { return _settingsViewModel; }
            set
            {
                _settingsViewModel = value;
                OnPropertyChanged();
            }
        }

        private HelpViewModel _helpViewModel;
        public HelpViewModel HelpViewModel
        {
            get { return _helpViewModel; }
            set
            {
                _helpViewModel = value;
                OnPropertyChanged();
            }
        }

        private bool _historySectionEnabled;
        public bool HistorySectionEnabled
        {
            get { return _historySectionEnabled; }
            set
            {
                _historySectionEnabled = value;
                OnPropertyChanged();
            }
        }

        public ViewModelLocator ViewModelLocator { get; set; }
        private bool _canSyncData { get; set; } = true;

        private readonly IPushDataService _pushDataService;
        private readonly IPullDataService _pullDataService;
        private readonly IAppVersionService _appVersionService;
        private readonly IOrganizationSettingsService _organizationSettingsService;
        private readonly IDataService _dataService;
        private readonly IUsageTracking _usageTracking;
        private BackgroundWorker BackgroundWorkerClient;
        private readonly IAppAnalytics _appAnalytics;
     

        public MainPageViewModel
            (
            IPushDataService pushDataService,
            IPullDataService pullDataService,
            IAppVersionService appVersionService,
            IOrganizationSettingsService organizationSettingsService,
            IDataService dataService,
            IUsageTracking usageTracking,
            IAppAnalytics appAnalytics
           )
        {
            _pushDataService = pushDataService;
            _pullDataService = pullDataService;
            _appVersionService = appVersionService;
            _organizationSettingsService = organizationSettingsService;
            _dataService = dataService;
            _usageTracking = usageTracking;
            _appAnalytics = appAnalytics;
           
            HistorySectionEnabled = false;

            BackgroundWorkerClient = new BackgroundWorker();
            BackgroundWorkerClient.DoWork += DoWorkAsync;
            BackgroundWorkerClient.RunWorkerCompleted += RunWorkerCompleted;

            MessagingCenter.Subscribe<string>(this, "ReInitialize", (sender) =>
            {
                CheckLoginStatus();
            });

            ConnectivityUtility.ListenForConnectionChanges();
            CheckLoginStatus();

            FontSizeHelper.GetTranscriptionsFontSize();

            ViewModelLocator = ResourceFinder.GetResource<ViewModelLocator>("Locator");
        }

        public void OnMenuItemChanged(int index)
        {

            switch (index)
            {
                case 0:
                    GetDashboard();
                    break;
                case 1:
                    GetHistory();
                    break;
                case 2:
                    GetSettings();
                    break;
                case 3:
                    GetHelp();
                    break;
            }

            if (index == 0)
            {

            }
        }

        private void GetDashboard()
        {
            MessagingCenter.Instance.Send(new FontResizerMessage { ShowFontResizer = true }, "QuickStartBtnMessage");

            if (DashboardViewModel == null)
            {
                DashboardViewModel = ViewModelLocator.DashboardViewModel;
            }

            PageTitle = "Home";

            HistorySectionEnabled = false;
        }

        private void GetHistory()
        {
            MessagingCenter.Instance.Send(new FontResizerMessage { ShowFontResizer = false, ShowOrganizationQuestions = false }, "QuickStartBtnMessage");

            if (HistoryViewModel == null)
            {
                HistoryViewModel = ViewModelLocator.HistoryViewModel;
            }

            PageTitle = "History";

            GetOrganizationSettings();
        }

        private async void GetOrganizationSettings()
        {
            var orgSettings = await _organizationSettingsService.GetOrganizationSettings();
            HistorySectionEnabled = orgSettings.HistoryPlaybackEnabled;
        }

        private void GetSettings()
        {
            MessagingCenter.Instance.Send(new FontResizerMessage { ShowFontResizer = false, ShowOrganizationQuestions = false }, "QuickStartBtnMessage");

            if (SettingsViewModel == null)
            {
                SettingsViewModel = ViewModelLocator.SettingsViewModel;
            }

            PageTitle = "Settings";

            HistorySectionEnabled = false;
        }

        private void GetHelp()
        {
            MessagingCenter.Instance.Send(new FontResizerMessage { ShowFontResizer = false, ShowOrganizationQuestions = false }, "QuickStartBtnMessage");

            if (HelpViewModel == null)
            {
                HelpViewModel = ViewModelLocator.HelpViewModel;
            }

            PageTitle = "Help";

            HistorySectionEnabled = false;
        }

        private async void CheckLoginStatus()
        {
            var isLoggedIn = Settings.IsUserLoggedIn();
            if (isLoggedIn)
            {
                await AttemptSlientLogin();
            }
            else
            {
                await SignOut();
            }
        }

        private async void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Do something when background work is completed
            var userLoggedIn = Settings.IsUserLoggedIn();

            if (!userLoggedIn)
            {
                await Services.Auth.ADB2CAuthenticationService.Instance.SignOutAsync();
            }
        }

        private void DoWorkAsync(object sender, DoWorkEventArgs e)
        {
            try
            {
                Thread pushThread = new Thread(_pushDataService.BeginDataSync);
                pushThread.Start();
                Thread pullThread = new Thread(_pullDataService.BeginDataSync);
                pullThread.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task AttemptSlientLogin()
        {
            try
            {
                var userContext = await Services.Auth.ADB2CAuthenticationService.Instance.AttemptSilentLogin().ConfigureAwait(true);
                var currentUser = await Settings.CurrentUser();
                if (userContext != null && currentUser != null)
                {
                    await GetAppVersion();
                    await _organizationSettingsService.UpdateOrganizationSettings();
                    MessagingCenter.Instance.Send("", "ReLoadLanguages");
                    await _usageTracking.GetUsageLimits(currentUser.UserIntID, currentUser.OrganizationId);

                    if (currentUser.DataConsentStatus == true)
                    {
                        BackgroundWorkerClient.RunWorkerAsync();
                    }
                }
                else
                {
                    await SignOut();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task GetAppVersion()
        {
            try
            {
                var appVersion = await _appVersionService.FetchAppVersion();

                if (appVersion != null && !appVersion.IsLatestVersion)
                {
                    if (appVersion.IsForcedUpdate)
                    {
                        Application.Current.MainPage = new ForcedUpdatePopup();
                        await Application.Current.MainPage.Navigation.PopToRootAsync();
                    }
                    else if (appVersion.IsUnsurpotedVersion)
                    {
                        // alert user to update app
                        await PopupNavigation.Instance.PushAsync(new UpdatePopup());
                        MessagingCenter.Instance.Send(appVersion, "ShowAppVersionAlertMessage");
                    }
                    else
                    {
                        // alert user to update app
                        await PopupNavigation.Instance.PushAsync(new UpdatePopup());
                        MessagingCenter.Instance.Send(appVersion, "ShowAppVersionAlertMessage");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task SignOut()
        {
            App.CurrentUser = null;
            Settings.ClearSettings();
            Settings.RemoveAllSecureSettings();
            await _dataService.DeleteAllItemsAsync<Session>();
            await _dataService.DeleteAllItemsAsync<Transcription>();
            await Services.Auth.ADB2CAuthenticationService.Instance.SignOutAsync();
            Application.Current.MainPage = new NavigationPage(new LoginPage());
            await Application.Current.MainPage.Navigation.PopToRootAsync();
        }

        private async Task MaybeLaunchQuickStart()
        {
            try
            {
                if (Settings.IsUserLoggedIn())
                    _appAnalytics.CaptureCustomEvent("Quick start launched",
                       new Dictionary<string, string>
                       {
                            {"User", App.CurrentUser.Email},
                            {"Organization", App.CurrentUser.Organization }
                           
                       });
                await PopupNavigation.Instance.PushAsync(new QuickStart());
            
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void LaunchOrganizationSettings()
        {
            MessagingCenter.Instance.Send("", "LaunchOrganizationSettings");
        }

        ICommand _filterPopupCommand = null;

        public ICommand FilterPopupCommand
        {
            get
            {
                return _filterPopupCommand ?? (_filterPopupCommand =
                                          new Command(() => MessagingCenter.Instance.Send("", "ShowFilterPopup")));
            }
        }

        ICommand _launchQuickStartCommand = null;

        public ICommand LaunchQuickStartCommand
        {
            get
            {
                return _launchQuickStartCommand ?? (_launchQuickStartCommand =
                                          new Command(async () => await MaybeLaunchQuickStart()));
            }
        }

        ICommand _launchOrganizationQuestionsCommand = null;

        public ICommand LaunchOrganizationQuestionsCommand
        {
            get
            {
                return _launchOrganizationQuestionsCommand ?? (_launchOrganizationQuestionsCommand =
                                          new Command(() => LaunchOrganizationSettings()));
            }
        }
    }
}
