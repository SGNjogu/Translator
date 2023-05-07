using TinyIoC;
using Translation.AuditTracking;
using Translation.Core.Interfaces;
using Translation.Core.Services.TranslationService;
using Translation.DataService.Interfaces;
using Translation.DataService.Services;
using Translation.DataSync.Interfaces;
using Translation.DataSync.Services;
using Translation.Helpers;
using Translation.Interface;
using Translation.Services.AudioFileSaver;
using Translation.Services.DataSync.Interfaces;
using Translation.Services.DataSync.Services;
using Translation.Services.Download;
using Translation.Services.Languages;
using Translation.Services.SignalR;
using Translation.Services.Usage;
using Translation.Services.Versioning;
using Translation.TextAnalytics;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class ViewModelLocator
    {
        IDataService Dataservice;
        ILanguagesService LanguagesService;
        IPushDataService PushDataService;
        IPullDataService PullDataService;
        IAzureStorageService AzureStorageService;
        IAppAnalytics AppAnalytics;
        IAppCrashlytics AppCrashlytics;
        ISentimentAnalysis SentimentAnalysis;
        IDataExportHelper DataExportHelper;
        ITranslator Translator;
        IAudioRecorder AudioRecorder;
        IMicrophoneService MicrophoneService;
        IAudioFileSaver AudioFileSaver;
        ISessionNumberService SessionNumberService;
        IOrganizationSettingsService OrganizationSettingsService;
        IUsageTracking UsageTracking;
        IAppVersionService AppVersionService;
        IAudioDeviceService AudioDeviceService;
        ISignalRService SignalRService;
        IMicrosoftTextToTextTranslator MicrosoftTextToTextTranslator;
        IMicrosoftStandardVoiceSynthesizer MicrosoftStandardVoiceSynthesizer;
        IMicrosoftTextToSpeechProvider MicrosoftTextToSpeechProvider;
        IDownloadService DownloadService;
        IFileService FileService;

        public MainPageViewModel MainPageViewModel
        {
            get
            {
                return new MainPageViewModel(
                     PushDataService,
                     PullDataService,
                     AppVersionService,
                     OrganizationSettingsService,
                     Dataservice,
                     UsageTracking,
                     AppAnalytics
                    );
            }
        }

        public DashboardViewModel DashboardViewModel
        {
            get
            {
                return new DashboardViewModel(
                    Dataservice,
                    PushDataService,
                    PullDataService,
                    AppAnalytics,
                    AppCrashlytics,
                    SentimentAnalysis,
                    AudioFileSaver,
                    AudioRecorder,
                    MicrophoneService,
                    SessionNumberService,
                    OrganizationSettingsService,
                    AudioDeviceService,
                    SignalRService,
                    MicrosoftTextToSpeechProvider,
                    LanguagesService
                    );
            }
        }

        public HelpViewModel HelpViewModel
        {
            get
            {
                return new HelpViewModel(Dataservice, AppAnalytics, AppCrashlytics);
            }
        }

        public HistoryViewModel HistoryViewModel
        {
            get
            {
                return new HistoryViewModel(Dataservice, AppAnalytics, PullDataService, PushDataService, OrganizationSettingsService);
            }
        }

        public TranscriptionsHistoryViewModel TranscriptionsHistoryViewModel
        {
            get
            {
                return new TranscriptionsHistoryViewModel(
                    Dataservice,
                    AppAnalytics,
                    SentimentAnalysis,
                    DataExportHelper,
                    AzureStorageService,
                    PushDataService,
                    OrganizationSettingsService,
                    LanguagesService,
                    DownloadService
                    );
            }
        }

        public AuthViewModel AuthViewModel
        {
            get
            {
                return new AuthViewModel(AppAnalytics, AppCrashlytics);
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get
            {
                return new SettingsViewModel(AppAnalytics, Dataservice, AudioDeviceService, LanguagesService);
            }
        }

        public WelcomeScreenViewModel WelcomeScreenViewModel
        {
            get
            {
                return new WelcomeScreenViewModel(AppAnalytics);
            }
        }

        public ImmersiveReaderViewModel ImmersiveReaderViewModel
        {
            get
            {
                return new ImmersiveReaderViewModel();
            }
        }

        public SessionMetaDataViewModel SessionMetaDataViewModel
        {
            get
            {
                return new SessionMetaDataViewModel(Dataservice);
            }
        }

        public UpdateViewModel UpdateViewModel
        {
            get
            {
                return new UpdateViewModel();
            }
        }

        public QuickStartViewModel QuickStartViewModel
        {
            get
            {
                return new QuickStartViewModel
                    (
                    Dataservice,
                    AudioDeviceService,
                    MicrosoftTextToTextTranslator,
                    MicrosoftStandardVoiceSynthesizer,
                    LanguagesService,
                    AppAnalytics
                    );
            }
        }

        public FontSizeViewViewModel FontSizeViewViewModel
        {
            get
            {
                return new FontSizeViewViewModel();
            }
        }

        public ViewModelLocator()
        {
            MicrophoneService = DependencyService.Get<IMicrophoneService>();
            Dataservice = TinyIoCContainer.Current.Resolve<Database>();
            Dataservice.InitializeAsync();
            AppAnalytics = TinyIoCContainer.Current.Resolve<AppAnalytics>();
            AppCrashlytics = TinyIoCContainer.Current.Resolve<AppCrashlytics>();
            AzureStorageService = new AzureStorageService();
            UsageTracking = new UsageTracking(Dataservice);
            LanguagesService = new LanguagesService(Dataservice);
            PushDataService = new PushDataService(Dataservice, AppCrashlytics, AzureStorageService, UsageTracking);
            PullDataService = new PullDataService(Dataservice, AppCrashlytics, LanguagesService);
            SentimentAnalysis = new SentimentAnalysis();
            DataExportHelper = new DataExportHelper(Dataservice);
            AudioRecorder = DependencyService.Get<IAudioRecorder>();
            AudioFileSaver = new AudioFileSaver();
            Translator = new MicrosoftTranslationProvider(AudioRecorder, AudioFileSaver);
            SessionNumberService = TinyIoCContainer.Current.Resolve<SessionNumberService>();
            OrganizationSettingsService = new OrganizationSettingsService(Dataservice);
            AppVersionService = new AppVersionService(Dataservice);
            AudioDeviceService = DependencyService.Get<IAudioDeviceService>();
            SignalRService = TinyIoCContainer.Current.Resolve<SignalRService>();
            MicrosoftTextToTextTranslator = new MicrosoftTextToTextTranslator();
            MicrosoftStandardVoiceSynthesizer = new MicrosoftStandardVoiceSynthesizer();
            MicrosoftTextToSpeechProvider = new MicrosoftTextToSpeechProvider(MicrosoftTextToTextTranslator, MicrosoftStandardVoiceSynthesizer);
            FileService = DependencyService.Get<IFileService>();
            DownloadService = new DownloadService(FileService);

        }
    }
}