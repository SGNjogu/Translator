using MediaManager;
using MediaManager.Library;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.AppSettings;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.DataSync.Interfaces;
using Translation.Helpers;
using Translation.Hmac;
using Translation.Interface;
using Translation.Models;
using Translation.Services.DataSync.Interfaces;
using Translation.Services.Download;
using Translation.Services.Languages;
using Translation.TextAnalytics;
using Translation.Utils;
using Translation.Views.Components.Player;
using Translation.Views.Components.Popups;
using Translation.Views.Pages.ImmersiveReader;
using Xamarin.Forms;
using BaseViewModel = MvvmHelpers.BaseViewModel;

namespace Translation.ViewModels
{
    public class TranscriptionsHistoryViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// The selected session
        /// </summary>
        private Session _selectedSession;
        public Session SelectedSession
        {
            get { return _selectedSession; }
            set
            {
                _selectedSession = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Selected Primary Language
        /// Displayed on UI
        /// </summary>
        private Language _PrimaryLanguage;
        public Language PrimaryLanguage
        {
            get { return _PrimaryLanguage; }
            set
            {
                _PrimaryLanguage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Selected Secondary Language
        /// </summary>
        private Language _secondaryLanguage;
        public Language SecondaryLanguage
        {
            get { return _secondaryLanguage; }
            set
            {
                _secondaryLanguage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// List of Sessions
        /// </summary>
        private List<TranslationResultText> _transcriptionsList;
        public List<TranslationResultText> TranscriptionsList
        {
            get { return _transcriptionsList; }
            set
            {
                _transcriptionsList = value;
                OnPropertyChanged();
            }
        }

        private bool _isPlayerLoading;
        public bool IsPlayerLoading
        {
            get { return _isPlayerLoading; }
            set
            {
                _isPlayerLoading = value;
                OnPropertyChanged();
            }
        }

        private bool _noAudioFile;
        public bool NoAudioFile
        {
            get { return _noAudioFile; }
            set
            {
                _noAudioFile = value;
                OnPropertyChanged();
            }
        }

        private bool _showAudioPlayer = true;
        public bool ShowAudioPlayer
        {
            get { return _showAudioPlayer; }
            set
            {
                _showAudioPlayer = value;
                OnPropertyChanged();
                if (ShowAudioPlayer)
                    NoAudioFile = false;
                else
                    NoAudioFile = true;
            }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _position;
        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisibleCopyButton = true;
        public bool IsVisibleCopyButton
        {
            get { return _isVisibleCopyButton; }
            set
            {
                _isVisibleCopyButton = value;
                OnPropertyChanged();
            }
        }

        private bool _isVisibleExportButton = true;
        public bool IsVisibleExportButton
        {
            get { return _isVisibleExportButton; }
            set
            {
                _isVisibleExportButton = value;
                OnPropertyChanged();
            }
        }

        double _maximum = 100f;
        public double Maximum
        {
            get { return _maximum; }
            set
            {
                if (value > 0)
                {
                    _maximum = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isMute = false;
        public bool IsMute
        {
            get { return _isMute; }
            set
            {
                _isMute = value;
                OnPropertyChanged();
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        private bool _downloadButtonVisibility = false;
        public bool DownloadButtonVisibility
        {
            get { return _downloadButtonVisibility; }
            set
            {
                _downloadButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        private bool _showDownloadView = false;
        public bool ShowDownloadView
        {
            get { return _showDownloadView; }
            set
            {
                _showDownloadView = value;
                OnPropertyChanged();
            }
        }

        private string _audioFilePath;
        public string AudioFilePath
        {
            get { return _audioFilePath; }
            set
            {
                _audioFilePath = value;
                OnPropertyChanged();
            }
        }

        private string _downloadStatus;
        public string DownloadStatus
        {
            get { return _downloadStatus; }
            set
            {
                _downloadStatus = value;
                OnPropertyChanged();
            }
        }

        private string _buttonText;
        public string ButtonText
        {
            get { return _buttonText; }
            set
            {
                _buttonText = value;
                OnPropertyChanged();
            }
        }

        private int UserId { get; set; }

        IDataService Dataservice;
        IAppAnalytics AppAnalytics;
        ISentimentAnalysis SentimentAnalysis;
        private readonly IDataExportHelper _dataExport;
        private readonly IAzureStorageService _azureStorageService;
        private readonly IPushDataService _pushDataService;
        private readonly IOrganizationSettingsService _organizationSettingsService;
        private readonly ILanguagesService _languagesService;
        private readonly IDownloadService _downloadService;
        private CancellationTokenSource cancellationToken;
        #endregion

        #region Constructor

        public TranscriptionsHistoryViewModel
            (
            IDataService database,
            IAppAnalytics appAnalytics,
            ISentimentAnalysis sentimentAnalysis,
            IDataExportHelper dataExport,
            IAzureStorageService azureStorageService,
            IPushDataService pushDataService,
            IOrganizationSettingsService organizationSettingsService,
            ILanguagesService languagesService,
            IDownloadService downloadService
            )
        {
            Dataservice = database;
            AppAnalytics = appAnalytics;
            SentimentAnalysis = sentimentAnalysis;
            _dataExport = dataExport;
            _azureStorageService = azureStorageService;
            _pushDataService = pushDataService;
            _organizationSettingsService = organizationSettingsService;
            _languagesService = languagesService;
            _downloadService = downloadService;
            AppAnalytics.CaptureCustomEvent("TranscriptionsHistoryPage Navigated");
            TranscriptionsList = new List<TranslationResultText>();
            ShowDownloadView = false;

            IsPlayerLoading = true;
            IsPlaying = false;
            MessagingCenter.Subscribe<Session>(this, "SelectedSession", (sender) =>
            {
                LoadSelectedSession(sender);
            });
            MessagingCenter.Subscribe<string>(this, "Unload", async (sender) =>
            {
                await Unload();
            });
            MessagingCenter.Subscribe<SliderDragMessage>(this, "SliderDragCompleted", async (sender) =>
            {
                await HandleSliderDrag(sender);
            });
            GetUserId();
            MessagingCenter.Subscribe<string>(this, "UpdateOrganizationSettings", (sender) =>
            {
                LoadSelectedSession(SelectedSession);
            });
        }

        #endregion

        #region Methods

        async void GetUserId()
        {
            var user = await Settings.CurrentUser();
            UserId = user.UserIntID;
        }

        async void LoadSelectedSession(Session session)
        {
            IsMute = false;
            CrossMediaManager.Current.Volume.Muted = false;
            var laguages = await _languagesService.GetSupportedLanguages();
            var primaryLanguage = laguages.FirstOrDefault(s => s.Code == session.SourceLangISO);
            var secondaryLanguage = laguages.FirstOrDefault(s => s.Code == session.TargetLangIso);

            if (primaryLanguage != null)
            {
                PrimaryLanguage = primaryLanguage;
            }

            if (secondaryLanguage != null)
            {
                SecondaryLanguage = secondaryLanguage;
            }

            await LoadTranscriptions(session).ConfigureAwait(false);
            if (ShowAudioPlayer)
                await LoadAudioPlayer().ConfigureAwait(false);
        }

        async Task LoadTranscriptions(Session session)
        {
            IsBusy = true;
            SelectedSession = session;

            var orgSettings = await _organizationSettingsService.GetOrganizationSettings();
            IsVisibleCopyButton = orgSettings.CopyPasteEnabled;
            IsVisibleExportButton = orgSettings.ExportEnabled;
            ShowAudioPlayer = orgSettings.HistoryAudioPlaybackEnabled;

            var transcriptions = await Dataservice.GetSessionTranscriptions(session.ID);

            var transcriptionsList = new List<TranslationResultText>();

            if (transcriptions.Any())
            {
                foreach (var transcription in transcriptions)
                {
                    var chat = new TranslationResultText
                    {
                        IsCopyPasteEnabled = IsVisibleCopyButton,
                        LanguageName = $"{SelectedSession.DisplaySourceLanguage} | {SelectedSession.DisplayTargetLanguage}",
                        Date = transcription.ChatTime,
                        OriginalText = transcription.OriginalText,
                        Person = transcription.ChatUser,
                        TranslatedText = transcription.TranslatedText,
                        SourceLanguageCode = SelectedSession.SourceLangISO,
                        TargetLanguageCode = SelectedSession.TargetLangIso,
                        DateString = transcription.ChatTime.ToString("HH:mm:ss"),
                        Sentiment = transcription.Sentiment,
                        SentimentEmoji = SentimentAnalysis.SentimentEmoji(transcription.Sentiment)
                    };

                    if (transcription.ChatUser == "Person 1")
                        chat.IsPerson1 = true;
                    else
                        chat.IsPerson1 = false;

                    transcriptionsList.Add(chat);
                }
            }

            TranscriptionsList = new List<TranslationResultText>(transcriptionsList);

            IsBusy = false;
        }

        async Task LoadAudioPlayer()
        {
            try
            {
                CrossMediaManager.Current.Queue.Clear();
                CrossMediaManager.Current.Dispose();
                IsPlayerLoading = true;
                string fileName = $"{SelectedSession.StartTime}.wav";
                string audioFilePath = await AudioFileUri(fileName);

                if (!string.IsNullOrEmpty(audioFilePath))
                {
                    NoAudioFile = false;
                    LoadMediaItem(audioFilePath, fileName);
                    AudioFilePath = audioFilePath;  
                    DownloadButtonVisibility = true;
                }
                else
                    NoAudioFile = true;

                IsPlayerLoading = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        void LoadMediaItem(string audioFilePath, string fileName)
        {
            CrossMediaManager.Current.Init();
            var queue = CrossMediaManager.Current.Queue;
            IMediaItem mediaItem = new MediaItem { MediaUri = audioFilePath, FileName = fileName };
            queue.Add(mediaItem);

            Duration = TimeSpan.FromSeconds((SelectedSession.EndTime - SelectedSession.StartTime));
            Maximum = Duration.TotalSeconds;
            Position = TimeSpan.FromSeconds(0);

            CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.Off;
            CrossMediaManager.Current.MediaItemFinished += Current_MediaItemFinished;
        }

        private async void Current_MediaItemFinished(object sender, MediaManager.Media.MediaItemEventArgs e)
        {
            IsPlaying = false;
            await CrossMediaManager.Current.Pause();
            await CrossMediaManager.Current.SeekToStart();
        }

        async Task<string> AudioFileUri(string audioName)
        {
            bool localFileExists = CheckIfLocalAudioFileExists(audioName);

            if (localFileExists)
                return FileAccessUtility.ReturnFilePath(audioName);

            bool blobExists = await _azureStorageService.CheckIfBlobExists(audioName, Constants.AzureStorageConnectionString, Constants.RecordingsContainer);

            if (blobExists)
                return $"{Constants.RecordingsURL}{audioName}";

            return null;
        }

        bool CheckIfLocalAudioFileExists(string audioName)
        {
            var waveFilePath = FileAccessUtility.ReturnFilePath(audioName);
            if (System.IO.File.Exists(waveFilePath))
            {
                return true;
            }
            return false;
        }

        private async Task PlayPause()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                await CrossMediaManager.Current.Pause();
            }
            else
            {
                IsPlaying = true;
                await CrossMediaManager.Current.Play();
                Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
                {
                    Duration = CrossMediaManager.Current.Duration - CrossMediaManager.Current.Position;
                    Maximum = Duration.TotalSeconds;
                    Position = CrossMediaManager.Current.Position;
                    return true;
                });
            }
        }

        async Task HandleSliderDrag(SliderDragMessage position)
        {
            Position = position.Position;
            await CrossMediaManager.Current.SeekTo(Position);
        }

        async Task CopyAll()
        {
            await ClosePopup();

            if (TranscriptionsList.Any())
            {
                Dialogs.ProgressDialog.Show();

                string textToCopy = string.Empty;

                foreach (var item in TranscriptionsList)
                {
                    string text = $"{item.Person}:\nOriginal: {item.OriginalText}\nTranslated: {item.TranslatedText}\n{item.DateString}\n\n";

                    textToCopy += text;
                }

                await Dialogs.CopyTextToClipBoard(textToCopy, "Copied to clipboard");
                AppAnalytics.CaptureCustomEvent("Copy Events",
                        new Dictionary<string, string> {
                        {"User", App.CurrentUser?.Email },
                        {"Organisation", App.CurrentUser?.Organization },
                        {"Action", "Copy all translation text" }
                 });
                Dialogs.ProgressDialog.Hide();
            }
        }

        async Task ExportData()
        {
            await ClosePopup();

            Dialogs.ProgressDialog.Show();

            try
            {
                var isStoragePermissionGranted = await FileAccessUtility.CheckStoragePermissions();

                if (isStoragePermissionGranted)
                {
                    await GenerateExcelFile();
                    Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, message: "File saved to Downloads Folder.", seconds: 3);
                    AppAnalytics.CaptureCustomEvent("Download Events",
                       new Dictionary<string, string> {
                        {"User", App.CurrentUser?.Email },
                        {"Organisation", App.CurrentUser?.Organization },
                        {"Action", "Download excel file" }
                });
                }
                else
                {
                    await PermissionsHelper.AskForStoragePermissions();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Dialogs.HandleDialogMessage(Dialogs.DialogMessage.UndefinedError);
            }

            Dialogs.ProgressDialog.Hide();
        }

        async Task GenerateExcelFile()
        {
            var memoryStream = await _dataExport.GenerateSessionData(SelectedSession.ID);
            string fileName = $"Speechly_{DateTime.Now.Ticks.ToString().Substring(10)}.xlsx";
            await FileAccessUtility.SaveFileExternally(ExternalFolder.Downloads, fileName, memoryStream.ToArray());
        }

        async Task ClosePopup()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        async Task Unload()
        {
            await CrossMediaManager.Current.Stop();
            CrossMediaManager.Current.Queue.Clear();
            CrossMediaManager.Current.Dispose();
        }

        async Task ImmersiveRead()
        {
            List<ImmersiveReaderRequest> request = new List<ImmersiveReaderRequest>();

            if (TranscriptionsList.Count != 0)
            {
                foreach (var item in TranscriptionsList)
                {
                    TranslationResultText chat = item;
                    request.AddRange(new List<ImmersiveReaderRequest>() {
                        new ImmersiveReaderRequest()
                            {
                                Content = chat.OriginalText,
                                LanguageCode = chat.SourceLanguageCode,
                            },
                        new ImmersiveReaderRequest()
                        {
                            Content = chat.TranslatedText,
                            LanguageCode = chat.TargetLanguageCode,
                        }
                    });
                }
                await ImmersiveRead(request);
            }
        }

        async Task ImmersiveRead(List<ImmersiveReaderRequest> request)
        {
            try
            {
                string endpoint = $"{Constants.BackendAPiEndpoint}{Constants.ImmersiveReaderEndpoint}";
                var client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.PostAsJsonAsync(endpoint, request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var webviewSource = new Uri($"{endpoint}/{long.Parse(content)}");

                    await PopupNavigation.Instance.PushAsync(new ImmersiveReader());
                    MessagingCenter.Instance.Send(webviewSource, "WebViewSource");
                }
                else
                {
                    Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, message: "Error, please try again.", seconds: 3);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Dialogs.HandleDialogMessage(Dialogs.DialogMessage.UndefinedError);
            }
        }

        async Task DownloadSessionAudio()
        {
            ButtonText = "Cancel";
            DownloadStatus = "Downloading";
            ShowDownloadView= true;
            var progressIndicator = new Progress<double>(ReportProgress);
            
            try
            {
                DownloadButtonVisibility = false;
                cancellationToken = new CancellationTokenSource();
                await _downloadService.DownloadFileAsync(AudioFilePath, progressIndicator, cancellationToken.Token, $"{SelectedSession.SessionNumber}.wav");
            }
            catch (OperationCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                //Manage cancellation here
            }
        }

         void ReportProgress(double value)
        {
            ProgressValue = value;
          
           if(value >= 100)
            {
                DownloadButtonVisibility = true;
                DownloadStatus = "Downloaded";
                ButtonText = "Close";
            }
   }

        private void CloseAudioDownload()
        {
            if(ButtonText == "Cancel" && ProgressValue < 100)
            {
                ButtonText = "Close";
                _downloadService.CancelDownload(cancellationToken);
                DownloadStatus = "Cancelled";
                return;
            }
           if(ButtonText == "Close")
            {
                ShowDownloadView = false;
                DownloadButtonVisibility= true;
            }
            
        }

        private void MuteUnmute()
        {
            if (IsMute)
            {
                // UnMute
                CrossMediaManager.Current.Volume.Muted = false;
                IsMute = false;
            }
            else
            {
                // Mute 
                CrossMediaManager.Current.Volume.Muted = true;
                IsMute = true;
            }
        }

        /// <summary>
        /// Command to show export data/copy all popup
        /// </summary>
        ICommand _showExportPopupCommand = null;
        public ICommand ShowExportPopupCommand
        {
            get
            {
                return _showExportPopupCommand ?? (_showExportPopupCommand =
                                          new Command(async () => await PopupNavigation.Instance.PushAsync(new ExportPopup { BindingContext = this })));
            }
        }

        /// <summary>
        /// Copy all command
        /// </summary>
        ICommand _copyAllCommand = null;
        public ICommand CopyAllCommand
        {
            get
            {
                return _copyAllCommand ?? (_copyAllCommand =
                                          new Command(async () => await CopyAll()));
            }
        }

        /// <summary>
        /// Copy to export data to excel file
        /// </summary>
        ICommand _exportDataCommand = null;
        public ICommand ExportDataCommand
        {
            get
            {
                return _exportDataCommand ?? (_exportDataCommand =
                                          new Command(async () => await ExportData()));
            }
        }

        /// <summary>
        /// Copy to export data to excel file
        /// </summary>
        ICommand _playPauseCommand = null;
        public ICommand PlayPauseCommand
        {
            get
            {
                return _playPauseCommand ?? (_playPauseCommand =
                                          new Command(async () => await PlayPause()));
            }
        }

        /// <summary>
        /// Command to open the ImmersiveReader
        /// </summary>
        ICommand _openImmersiveReaderCommand = null;

        public ICommand OpenImmersiveReaderCommand
        {
            get
            {
                return _openImmersiveReaderCommand ?? (_openImmersiveReaderCommand =
                                          new Command(async (object obj) => await ImmersiveRead()));
            }
        }

        ICommand _muteCommand = null;

        public ICommand MuteCommand
        {
            get
            {
                return _muteCommand ?? (_muteCommand =
                                          new Command(() => MuteUnmute()));
            }
        }

        /// <summary>
        /// Command to download audio file
        /// </summary>
        ICommand _downloadCommand = null;

        public ICommand DownloadCommand
        {
            get
            {
                return _downloadCommand ?? (_downloadCommand =
                                          new Command(async (object obj) => await DownloadSessionAudio()));
            }
        }

        /// <summary>
        /// Command to cancel download audio file
        /// </summary>
        ICommand _cancelDownloadCommand = null;

        public ICommand CancelDownloadCommand
        {
            get
            {
                return _cancelDownloadCommand ?? (_cancelDownloadCommand =
                                          new Command( (object obj) =>  CloseAudioDownload()));
            }
        }

        #endregion
    }
}
