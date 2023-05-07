using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.DataSync.Interfaces;
using Translation.Interface;
using Translation.Models;
using Translation.Services.DataSync.Interfaces;
using Translation.Utils;
using Translation.Views.Components.Popups;
using Xamarin.Forms;
using BaseViewModel = MvvmHelpers.BaseViewModel;

namespace Translation.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// List of Sessions
        /// </summary>
        private ObservableRangeCollection<HistorySession> _historySessionsList;
        public ObservableRangeCollection<HistorySession> HistorySessionsList
        {
            get { return _historySessionsList; }
            set
            {
                _historySessionsList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Search text bindinded to UI
        /// </summary>
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Filter search by session number search text
        /// </summary>
        private string _sessionNumberSearchText;

        public string SessionNumberSearchText
        {
            get { return _sessionNumberSearchText; }
            set
            {
                _sessionNumberSearchText = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// True if no search results
        /// </summary>
        private bool _noResults;
        public bool NoResults
        {
            get { return _noResults; }
            set
            {
                _noResults = value;
                OnPropertyChanged();
            }
        }

        private bool _historySectionEnabled = true;
        public bool HistorySectionEnabled
        {
            get { return _historySectionEnabled; }
            set
            {
                _historySectionEnabled = value;
                OnPropertyChanged();
            }
        }

        private DateTime _selectedStartDate;
        public DateTime SelectedStartDate
        {
            get { return _selectedStartDate; }
            set
            {
                _selectedStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _maxStartDate;
        public DateTime MaxStartDate
        {
            get { return _maxStartDate; }
            set
            {
                _maxStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _minStartDate;
        public DateTime MinStartDate
        {
            get { return _minStartDate; }
            set
            {
                _minStartDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _selectedEndDate;
        public DateTime SelectedEndDate
        {
            get { return _selectedEndDate; }
            set
            {
                _selectedEndDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _maxEndDate;
        public DateTime MaxEndDate
        {
            get { return _maxEndDate; }
            set
            {
                _maxEndDate = value;
                OnPropertyChanged();
            }
        }

        private DateTime _minEndDate;
        public DateTime MinEndDate
        {
            get { return _minEndDate; }
            set
            {
                _minEndDate = value;
                OnPropertyChanged();
            }
        }

        private readonly IDataService Dataservice;
        private readonly IAppAnalytics AppAnalytics;
        private readonly IPullDataService _pullDataService;
        private readonly IPushDataService _pushDataService;
        private readonly IOrganizationSettingsService _organizationSettingsService;

        #endregion

        #region Constructor

        public HistoryViewModel(IDataService database, IAppAnalytics appAnalytics, IPullDataService pullDataService, IPushDataService pushDataService, IOrganizationSettingsService organizationSettingsService)
        {
            Dataservice = database;
            AppAnalytics = appAnalytics;
            _pullDataService = pullDataService;
            _pushDataService = pushDataService;
            _organizationSettingsService = organizationSettingsService;
            HistorySessionsList = new ObservableRangeCollection<HistorySession>();
            NoResults = false;

            AppAnalytics.CaptureCustomEvent("HistoryPage Navigated");

            MessagingCenter.Subscribe<Session>(this, "AddSession", (sender) =>
            {
                AddSession(sender);
            });

            MessagingCenter.Subscribe<Session>(this, "UpdateSessionSyncStatus", (sender) =>
            {
                UpdateSyncStatus(sender);
            });

            MessagingCenter.Subscribe<string>(this, "UpdateOrganizationSettings", async (sender) =>
            {
                await GetOrganizationSettings();
            });

            MessagingCenter.Subscribe<string>(this, "ShowFilterPopup", async (sender) =>
            {
                await OpenSearchFilterPopup();
            });

            _ = GetOrganizationSettings();

            ClearFilter();
            _ = LoadLocalSessions();
        }

        #endregion

        #region Methods

        async Task GetOrganizationSettings()
        {
            var orgSettings = await _organizationSettingsService.GetOrganizationSettings();
            HistorySectionEnabled = orgSettings.HistoryPlaybackEnabled;
        }

        void UpdateSyncStatus(Session session)
        {
            if (HistorySessionsList.Any())
            {
                var existingHistorySession = HistorySessionsList.FirstOrDefault(s => s.Any(d => d.ID == session.ID));

                if (existingHistorySession != null)
                {
                    var existingSession = existingHistorySession.FirstOrDefault(s => s.ID == session.ID);

                    if (existingSession != null)
                        existingSession.SyncedToServer = true;
                }
            }
        }

        void AddSession(Session session)
        {
            var yearCreated = session.Created.Year;
            var monthCreated = session.Created.Month;
            var dayCreated = session.Created.Day;
            var date = new DateTime(yearCreated, monthCreated, dayCreated);

            var existingHistorySession = HistorySessionsList.FirstOrDefault(s => s.SessionDate == date);

            if (existingHistorySession != null)
            {
                existingHistorySession.Insert(0, session);
            }
            else
            {
                var newHistorySession = new HistorySession
                {
                    SessionDate = date
                };

                newHistorySession.Add(session);

                HistorySessionsList.Insert(0, newHistorySession);
            }
        }

        async Task LoadLocalSessions()
        {
            Dialogs.ProgressDialog.Show();

            await LoadSessions();

            Dialogs.ProgressDialog.Hide();
        }

        /// <summary>
        /// Method to get sessions from local DB
        /// </summary>
        async Task LoadSessions()
        {
            try
            {
                NoResults = false;

                IsBusy = true;

                await Task.Delay(500);

                if (HistorySessionsList.Any())
                {
                    HistorySessionsList.Clear();
                }

                var sessions = await Dataservice.GetSessionsAsync();
                var groupedData = await GroupData(sessions.ToList());
                HistorySessionsList = new ObservableRangeCollection<HistorySession>(groupedData);

                IsBusy = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Dialogs.ProgressDialog.Hide();
            }
        }

        async Task OpenSearchFilterPopup()
        {
            await PopupNavigation.Instance.PushAsync(new SearchFilterPopup { BindingContext = this });
        }

        async Task ApplySearchFilter()
        {
            Dialogs.ProgressDialog.Show();
            await Task.Delay(500);

            if (SelectedStartDate > SelectedEndDate)
            {
                await ClosePopup();
                Dialogs.ProgressDialog.Hide();
                Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, message: "Start Date cannot be longer than End Date", backgroundColor: "#d50000");
                return;
            }

            await LoadLocalSessions();

            var dateRange = DateTimeUtility.GetDateRange(SelectedStartDate, SelectedEndDate);
            var sessions = GetSessions(HistorySessionsList);
            var dateFilteredSession = FilterByDate(sessions, dateRange);
            var filteredSessions = await FilterAsync(dateFilteredSession);
            var groupedData = await GroupData(filteredSessions.ToList());
            HistorySessionsList = new ObservableRangeCollection<HistorySession>(groupedData);

            if (!HistorySessionsList.Any())
                NoResults = true;

            ClearFilter();
            await ClosePopup();

            Dialogs.ProgressDialog.Hide();
        }

        private ObservableRangeCollection<Session> GetSessions(ObservableRangeCollection<HistorySession> historySessions)
        {
            ObservableRangeCollection<Session> sessionsList = new ObservableRangeCollection<Session>();

            foreach (var historySession in historySessions)
            {
                var yearCreated = historySession.SessionDate.Year;
                var monthCreated = historySession.SessionDate.Month;
                var dayCreated = historySession.SessionDate.Day;

                var dateFilteredSessions = historySession.ToList().FindAll
                    (
                      s => s.Created.Year == yearCreated &&
                      s.Created.Month == monthCreated &&
                      s.Created.Day == dayCreated
                    );

                if (dateFilteredSessions.Any())
                {
                    foreach (var item in dateFilteredSessions)
                    {
                        if (!sessionsList.Contains(item))
                        {
                            sessionsList.Add(item);
                        }
                    }
                }
            }

            return sessionsList;
        }

        private ObservableRangeCollection<Session> FilterByDate(ObservableRangeCollection<Session> originalList, DateRange dateRange)
        {
            ObservableRangeCollection<Session> filter = new ObservableRangeCollection<Session>();

            for (int i = 0; i < originalList.Count; i++)
            {
                Session session = originalList[i];

                if (DateTimeUtility.IsInDateRange(session.Created, dateRange))
                {
                    filter.Add(session);
                }
            }

            return filter;
        }

        public async Task<ObservableRangeCollection<Session>> FilterAsync(ObservableRangeCollection<Session> sessions)
        {
            HashSet<Session> filteredList = new HashSet<Session>();

            try
            {
                for (int i = 0; i < sessions.Count(); i++)
                {
                    var searchItem = sessions[i];

                    if (!string.IsNullOrEmpty(SearchText))
                    {
                        // filter by language
                        if
                            (
                             searchItem.SourceLanguage.ToLower().Contains(SearchText.ToLower()) ||
                             searchItem.TargeLanguage.ToLower().Contains(SearchText.ToLower())
                            )
                        {
                            if (!filteredList.Contains(searchItem))
                                filteredList.Add(searchItem);
                        }

                        // filter by sessionNumber
                        if
                            (
                            !string.IsNullOrEmpty(searchItem.SessionNumber) &&
                            searchItem.SessionNumber.ToLower().Contains(SearchText.ToLower())
                            )
                        {
                            if (!filteredList.Contains(searchItem))
                                filteredList.Add(searchItem);
                        }

                        // filter by sessionName
                        if
                            (
                            !string.IsNullOrEmpty(searchItem.SessionName) &&
                            searchItem.SessionName.ToLower().Contains(SearchText.ToLower())
                            )
                        {
                            if (!filteredList.Contains(searchItem))
                                filteredList.Add(searchItem);
                        }

                        // filter by custom tag
                        if
                            (
                            !string.IsNullOrEmpty(searchItem.CustomTags) &&
                            searchItem.CustomTags.ToLower().Contains(SearchText.ToLower())
                            )
                        {
                            if (!filteredList.Contains(searchItem))
                                filteredList.Add(searchItem);
                        }

                        // filter by session tag
                        var sessionTags = await Dataservice.GetSessionTags(searchItem.ID);

                        if (sessionTags != null && sessionTags.Any())
                        {
                            if (sessionTags.Any(t => !string.IsNullOrEmpty(t.TagValue) && t.TagValue.Contains(SearchText)))
                                if (!filteredList.Contains(searchItem))
                                    filteredList.Add(searchItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return new ObservableRangeCollection<Session>(filteredList);
        }

        void ClearFilter()
        {
            NoResults = false;
            SearchText = "";
            SessionNumberSearchText = "";
            SelectedStartDate = DateTime.Now;
            SelectedEndDate = DateTime.Now;
            MaxEndDate = DateTime.Now;
            MaxStartDate = DateTime.Now;
            MinStartDate = new DateTime(2019, 1, 1);
            MinEndDate = new DateTime(2019, 1, 1);
        }

        async Task ResetFilter()
        {
            ClearFilter();
            await ClosePopup();
            await LoadLocalSessions();
        }

        async Task ClosePopup()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        async Task RefreshHistory()
        {
            try
            {
                IsBusy = true;

                await _pullDataService.SyncDatabase();
                await LoadSessions();

                var currentUser = await AppSettings.Settings.CurrentUser();

                if (currentUser.DataConsentStatus == true)
                {
                    Thread pushThread = new Thread(_pushDataService.BeginDataSync);
                    pushThread.Start();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            IsBusy = false;
        }

        private async Task<List<HistorySession>> GroupData(List<Session> sessionsList)
        {
            List<HistorySession> historySessions = new List<HistorySession>();

            await Task.Run(() =>
            {
                foreach (var session in sessionsList)
                {
                    var yearCreated = session.Created.Year;
                    var monthCreated = session.Created.Month;
                    var dayCreated = session.Created.Day;

                    var groupedSessions = sessionsList.FindAll
                    (
                        s => s.Created.Year == yearCreated &&
                        s.Created.Month == monthCreated &&
                        s.Created.Day == dayCreated
                    );

                    var historySession = new HistorySession
                    {
                        SessionDate = new DateTime(yearCreated, monthCreated, dayCreated)
                    };

                    foreach (var groupedSession in groupedSessions)
                    {
                        historySession.Add(groupedSession);
                    }

                    if (!historySessions.Any(s => s.SessionDate == historySession.SessionDate))
                        historySessions.Add(historySession);
                }
            });

            return historySessions.OrderBy(d => d.SessionDate).Reverse().ToList();
        }

        /// <summary>
        /// Command to refresh page data
        /// </summary>
        ICommand _refreshCommand = null;

        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand =
                                          new Command(async () => await RefreshHistory()));
            }
        }

        /// <summary>
        /// Command to open filter popup
        /// </summary>
        ICommand _filterPopupCommand = null;

        public ICommand FilterPopupCommand
        {
            get
            {
                return _filterPopupCommand ?? (_filterPopupCommand =
                                          new Command(async () => await OpenSearchFilterPopup()));
            }
        }

        /// <summary>
        /// Command to apply filter
        /// </summary>
        ICommand _applyFilterCommand = null;

        public ICommand ApplyFilterCommand
        {
            get
            {
                return _applyFilterCommand ?? (_applyFilterCommand =
                                          new Command(async () => await ApplySearchFilter()));
            }
        }

        /// <summary>
        /// Command to clear filter
        /// </summary>
        ICommand _clearFilterCommand = null;

        public ICommand ClearFilterCommand
        {
            get
            {
                return _clearFilterCommand ?? (_clearFilterCommand =
                                          new Command(async () => await ResetFilter()));
            }
        }

        ICommand _closePopupCommand = null;

        public ICommand ClosePopupCommand
        {
            get
            {
                return _closePopupCommand ?? (_closePopupCommand =
                                          new Command(async () => await ClosePopup()));
            }
        }

        #endregion
    }
}
