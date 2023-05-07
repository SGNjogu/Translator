using MediaManager;
using MvvmHelpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Timers;
using Translation.Core.Domain;
using Translation.Core.Interfaces;
using Translation.DataService.Interfaces;
using Translation.DataSync.Interfaces;
using Translation.Interface;
using Translation.Messages;
using Translation.Models;
using Translation.Services.DataSync.Interfaces;
using Translation.Services.Languages;
using Translation.Services.SignalR;
using Translation.TextAnalytics;
using Translation.Utils;
using Xamarin.Forms;
using Language = Translation.Models.Language;
using UserQuestions = Translation.DataService.Models.UserQuestions;

namespace Translation.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// List of Languages
        /// </summary>
        private List<Language> _languages;
        public List<Language> Languages
        {
            get { return _languages; }
            set
            {
                _languages = value;
                OnPropertyChanged();
            }
        }

        private List<Language> _recentLanguages;
        public List<Language> RecentLanguages
        {
            get { return _recentLanguages; }
            set
            {
                _recentLanguages = value;
                OnPropertyChanged();
            }
        }

        private string _languageSearchText;
        public string LanguageSearchText
        {
            get { return _languageSearchText; }
            set
            {
                _languageSearchText = value;
                OnPropertyChanged();
                SearchLanguage(LanguageSearchText);
            }
        }

        /// <summary>
        /// List of Primary Languages
        /// </summary>
        private List<Language> _primaryLanguageList;
        public List<Language> PrimaryLanguageList
        {
            get { return _primaryLanguageList; }
            set
            {
                _primaryLanguageList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// List of Secondary Languages
        /// </summary>
        private List<Language> _secondaryLanguageList;
        public List<Language> SecondaryLanguageList
        {
            get { return _secondaryLanguageList; }
            set
            {
                _secondaryLanguageList = value;
                OnPropertyChanged();
            }
        }

        private List<Language> _autodetectSecondaryLanguages { get; set; }

        /// <summary>
        /// Selected Primary Language
        /// </summary>
        private Language _primaryLanguage;
        public Language PrimaryLanguage
        {
            get { return _primaryLanguage; }
            set
            {
                _primaryLanguage = value;
                OnPropertyChanged();
                DisplayPrimaryLanguage = PrimaryLanguage;
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
                DisplaySecondaryLanguage = SecondaryLanguage;
            }
        }

        /// <summary>
        /// Selected Primary Language
        /// Displayed on UI
        /// </summary>
        private Language _displayPrimaryLanguage;
        public Language DisplayPrimaryLanguage
        {
            get { return _displayPrimaryLanguage; }
            set
            {
                _displayPrimaryLanguage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Selected Secondary Language
        /// Displayed on UI
        /// </summary>
        private Language _displaySecondaryLanguage;
        public Language DisplaySecondaryLanguage
        {
            get { return _displaySecondaryLanguage; }
            set
            {
                _displaySecondaryLanguage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True if is translating
        /// </summary>
        private bool _isTranslating;
        public bool IsTranslating
        {
            get { return _isTranslating; }
            set
            {
                _isTranslating = value;
                OnPropertyChanged();
                if (IsTranslating)
                {
                    if (OrganizationQuestions != null && OrganizationQuestions.Any())
                    {
                        MessagingCenter.Instance.Send(new FontResizerMessage { ShowFontResizer = true, ShowOrganizationQuestions = true }, "FontResizerMessage");
                    }
                    else
                    {
                        MessagingCenter.Instance.Send(new FontResizerMessage { ShowFontResizer = true }, "FontResizerMessage");
                    }
                    TranslateBtnIcon = FontAwesomeIcons.Times;
                    TranslateBtnText = "End Session";
                }
                else
                {
                    MessagingCenter.Instance.Send(new FontResizerMessage { ShowFontResizer = false, ShowOrganizationQuestions = false }, "FontResizerMessage");
                    TranslateBtnIcon = FontAwesomeIcons.Microphone;
                    TranslateBtnText = "Translate";
                }
            }
        }

        /// <summary>
        /// True if is first translating
        /// </summary>
        private bool _isFirstTranslation;
        public bool IsFirstTranslation
        {
            get { return _isFirstTranslation; }
            set
            {
                _isFirstTranslation = value;
                OnPropertyChanged();
            }
        }

        private bool _isSessionMetaDataVisible;
        public bool IsSessionMetaDataVisible
        {
            get { return _isSessionMetaDataVisible; }
            set
            {
                _isSessionMetaDataVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Icon of translateBtn
        /// Changes depending on Translation State
        /// </summary>
        private string _translateBtnIcon;
        public string TranslateBtnIcon
        {
            get { return _translateBtnIcon; }
            set
            {
                _translateBtnIcon = value;
                OnPropertyChanged();
            }
        }

        private bool _isMicrophoneMute = false;

        public bool IsMicrophoneMute
        {
            get { return _isMicrophoneMute; }
            set
            {
                _isMicrophoneMute = value;
                UpdateMicStatus();
            }
        }


        /// <summary>
        /// Icon to show Microphone status
        /// Changes depending on user input
        /// </summary>
        private string _muteBtnIcon;
        public string MuteBtnIcon
        {
            get { return _muteBtnIcon; }
            set
            {
                _muteBtnIcon = value;
                OnPropertyChanged();
            }
        }

        private string _translateBtnText;
        public string TranslateBtnText
        {
            get { return _translateBtnText; }
            set
            {
                _translateBtnText = value;
                OnPropertyChanged();
            }
        }

        private string _muteBtnText;
        public string MuteBtnText
        {
            get { return _muteBtnText; }
            set
            {
                _muteBtnText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Image displayed on the Dashboard
        /// Changes depending on Theme
        /// </summary>
        private string _dashboardImage;
        public string DashboardImage
        {
            get { return _dashboardImage; }
            set
            {
                _dashboardImage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True if Mic has permissions 
        /// </summary>
        private bool IsMicEnabled = false;
        private bool IsStorageAccessEnabled = false;

        bool IsPlaying { get; set; }

        /// <summary>
        /// List of transcriptions to be displayed in the UI
        /// </summary>
        ObservableRangeCollection<TranslationResultText> _chatList;
        public ObservableRangeCollection<TranslationResultText> ChatList
        {
            get { return _chatList; }
            set
            {
                _chatList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Halo Color
        /// </summary>
        Color _haloColor;
        public Color HaloColor
        {
            get { return _haloColor; }
            set
            {
                _haloColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// SpeakIn 
        /// </summary>
        string _speakIn;
        public string SpeakIn
        {
            get { return _speakIn; }
            set
            {
                _speakIn = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Session Number binding property
        /// </summary>
        private string _sessionNumber;

        public string SessionNumber
        {
            get { return _sessionNumber; }
            set
            {
                _sessionNumber = value;
                OnPropertyChanged();
            }
        }


        bool CanSyncData { get; set; }
        bool AllowExplicitContent { get; set; } = false;
        bool EnableSessionTags { get; set; } = false;

        private ConcurrentQueue<Stream> AudioQueue { get; set; }
        private IMicrophoneService _microphoneService;

        private Timer _audioNotDetectedTimer;
        private Timer _countDownTimer;
        private int _timeCounterSeconds = 0;
        string _timeCounter;
        public string TimeCounter
        {
            get { return _timeCounter; }
            set
            {
                _timeCounter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Session start time
        /// </summary>
        private long StartTime;
        bool IsSelectingPrimaryLanguage = true;
        private string _audioFilePath { get; set; }
        private double SessionBillableSeconds { get; set; }
        private double? _maxUserSessionTime { get; set; }
        private Timer _translationTimer;
        private int _translationTimeCounterSeconds { get; set; } = 0;

        string _sessionName;
        public string SessionName
        {
            get { return _sessionName; }
            set
            {
                _sessionName = value;
                OnPropertyChanged();
            }
        }

        private bool _showSwitchButton = false;
        public bool ShowSwitchButton
        {
            get { return _showSwitchButton; }
            set
            {
                _showSwitchButton = value;
                OnPropertyChanged();
            }
        }

        private bool _isSecondaryLanguageVisible = true;
        public bool IsSecondaryLanguageVisible
        {
            get { return _isSecondaryLanguageVisible; }
            set
            {
                _isSecondaryLanguageVisible = value;
                OnPropertyChanged();
            }
        }

        List<SessionTag> SessionTags { get; set; }
        List<string> CustomTags { get; set; }

        int _tagsCount;
        public int TagsCount
        {
            get { return _tagsCount; }
            set
            {
                _tagsCount = value;
                OnPropertyChanged();
            }
        }

        private bool _showOrganizationQuestions = false;
        public bool ShowOrganizationQuestions
        {
            get { return _showOrganizationQuestions; }
            set
            {
                _showOrganizationQuestions = value;
                OnPropertyChanged();
            }
        }

        private ObservableRangeCollection<UserQuestions> _organizationQuestions = null;
        public ObservableRangeCollection<UserQuestions> OrganizationQuestions
        {
            get { return _organizationQuestions; }
            set
            {
                _organizationQuestions = value;
                OnPropertyChanged();
            }
        }

        private bool _useNerualVoice { get; set; }
        private List<Language> AutoDetectLanguages { get; set; }
        public bool CannotBeAutoDetected { get; set; } = true;

        private AudioDevice _selectedDevice { get; set; }

        private ITranslator _translator { get; set; }

        private readonly IPushDataService _pushDataService;
        private readonly IPullDataService _pullDataService;
        private readonly IDataService _dataservice;
        private readonly IAppAnalytics _appAnalytics;
        private readonly IAppCrashlytics _appCrashlytics;
        private readonly ISentimentAnalysis _sentimentAnalysis;
        private readonly IAudioFileSaver _audioFileSaver;
        private readonly IAudioRecorder _audioRecorder;
        private readonly ISessionNumberService _sessionNumberService;
        private readonly IOrganizationSettingsService _organizationSettingsService;
        private readonly IAudioDeviceService _audioDeviceService;
        private readonly ISignalRService _signalRService;
        private readonly IMicrosoftTextToSpeechProvider _microsoftTextToSpeechProvider;
        private readonly ILanguagesService _languagesService;

        #endregion

        #region Constructor

        public DashboardViewModel(
            IDataService database,
            IPushDataService dataSyncService,
            IPullDataService pullDataService,
            IAppAnalytics appAnalytics,
            IAppCrashlytics appCrashlytics,
            ISentimentAnalysis sentimentAnalysis,
            IAudioFileSaver audioFileSaver,
            IAudioRecorder audioRecorder,
            IMicrophoneService microphoneService,
            ISessionNumberService sessionNumberService,
            IOrganizationSettingsService organizationSettingsService,
            IAudioDeviceService audioDeviceService,
            ISignalRService signalRService,
            IMicrosoftTextToSpeechProvider microsoftTextToSpeechProvider,
            ILanguagesService languagesService
            )
        {
            _dataservice = database;
            _pushDataService = dataSyncService;
            _pullDataService = pullDataService;
            _appAnalytics = appAnalytics;
            _appCrashlytics = appCrashlytics;
            _sentimentAnalysis = sentimentAnalysis;
            _audioFileSaver = audioFileSaver;
            _audioRecorder = audioRecorder;
            _appAnalytics.CaptureCustomEvent("Dashboard Navigated");
            _microphoneService = microphoneService;
            _sessionNumberService = sessionNumberService;
            _organizationSettingsService = organizationSettingsService;
            _audioDeviceService = audioDeviceService;
            _signalRService = signalRService;
            _microsoftTextToSpeechProvider = microsoftTextToSpeechProvider;
            _languagesService = languagesService;

            HaloColor = Color.Transparent;
            Languages = new List<Language>();
            RecentLanguages = new List<Language>();
            PrimaryLanguageList = new List<Language>();
            SecondaryLanguageList = new List<Language>();
            ChatList = new ObservableRangeCollection<TranslationResultText>();
            _chatList.CollectionChanged += _chatList_CollectionChanged;
            _ = LoadAudioDevices();
            LoadLanguages();
            UpdateMicStatus();
            IsTranslating = false;
            IsFirstTranslation = true;
            IsPermissionsGranted().ConfigureAwait(false);
            AudioQueue = new ConcurrentQueue<Stream>();
            CrossMediaManager.Current.Init();
            IsPlaying = false;
            LoadDashboardImagImage();
            SessionNumber = "...";
            SessionName = string.Empty;
            IsSessionMetaDataVisible = false;

            _audioNotDetectedTimer = new Timer();
            _audioNotDetectedTimer.Interval = TimeSpan.FromSeconds(60).TotalMilliseconds;
            _audioNotDetectedTimer.Elapsed += AudioNotDetectedTimer_Elapsed;

            _countDownTimer = new Timer();
            _countDownTimer.Interval = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
            _countDownTimer.Elapsed += CountDownTimer_Elapsed;
            _timeCounterSeconds = 60;

            _translationTimer = new Timer();
            _translationTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
            _translationTimer.Elapsed += TranslationTimer_Elapsed;

            MessagingCenter.Subscribe<string>(this, "ChangeDashboardImage", (sender) =>
            {
                LoadDashboardImagImage();
            });

            MessagingCenter.Subscribe<SessionMetaDataMessage>(this, "SaveSessionMetadata", async (sender) =>
            {
                await SaveSessionMetadata(sender);
            });

            MessagingCenter.Subscribe<SessionMetaDataMessage>(this, "SaveSession", async (sender) =>
            {
                await SaveSession().ConfigureAwait(false);
            });

            MessagingCenter.Subscribe<SessionMetaDataMessage>(this, "UpdateSessionMetadata", async (sender) =>
            {
                await UpdateSessionMetadata(sender);
            });

            MessagingCenter.Subscribe<string>(this, "UpdateOrganizationSettings", (sender) =>
            {
                _ = GetOrganizationSettings();
            });

            MessagingCenter.Subscribe<string>(this, "ReLoadLanguages", (sender) =>
            {
                LoadLanguages();
            });

            MessagingCenter.Subscribe<AudioDevice>(this, "ChangeIODevice", (sender) =>
            {
                _selectedDevice = sender;
            });

            MessagingCenter.Subscribe<Language>(this, "UpdateLanguageOne", (sender) =>
            {
                PrimaryLanguage = sender;
            });

            MessagingCenter.Subscribe<Language>(this, "UpdateLanguageTwo", (sender) =>
            {
                SecondaryLanguage = sender;
                _autodetectSecondaryLanguages = null;
            });

            MessagingCenter.Subscribe<List<Language>>(this, "UpdateAutoDetectTranslationLanguages", (sender) =>
            {
                _autodetectSecondaryLanguages = sender;
            });

            MessagingCenter.Subscribe<string>(this, "StartTranslation", async (sender) =>
            {
                await AnimateAndStartTranslation();
            });

            MessagingCenter.Subscribe<string>(this, "LaunchOrganizationSettings", async (sender) =>
            {
                await LaunchOrganizationSettings();
            });

            MessagingCenter.Subscribe<UserQuestions>(this, "SelectOrganizationQuestion", async (sender) =>
            {
                await SelectOrganizationQuestion(sender);
            });
            
            MessagingCenter.Subscribe<string>(this, "UpdateLanguageLists", async (sender) =>
            {
                await UpdateLanguageLists();
            });

            _ = MaybeLaunchQuickStart();

            _ = GetOrganizationSettings();

            InitializeAutoDetectLanguages();
        }

        private void _chatList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // if(e.Action != NotifyCollectionChangedAction.Add || e.Action != NotifyCollectionChangedAction.Remove)
        }

        #endregion
    }
}