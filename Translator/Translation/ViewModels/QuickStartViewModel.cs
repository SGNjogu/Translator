using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.AppSettings;
using Translation.Core.Interfaces;
using Translation.DataService.Interfaces;
using Translation.Interface;
using Translation.Messages;
using Translation.Services.DataSync.Interfaces;
using Translation.Services.Languages;
using Translation.Views.Components.QuickStartComponents;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class QuickStartViewModel : BaseViewModel
    {
        private ObservableRangeCollection<ContentView> _views;
        public ObservableRangeCollection<ContentView> Views
        {
            get { return _views; }
            set
            {
                _views = value;
                OnPropertyChanged();
            }
        }

        private int _carouselPosition;
        public int CarouselPosition
        {
            get { return _carouselPosition; }
            set
            {
                _carouselPosition = value;
                OnPropertyChanged();
                if (!RememberSetup)
                {
                    HandleViewChange(CarouselPosition);
                }
            }
        }

        private ContentView _selectedView;
        public ContentView SelectedView
        {
            get { return _selectedView; }
            set
            {
                _selectedView = value;
                OnPropertyChanged();
            }
        }

        private string _instructionText;
        public string InstructionText
        {
            get { return _instructionText; }
            set
            {
                _instructionText = value;
                OnPropertyChanged();
            }
        }

        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                SearchLanguage(SearchText);
            }
        }

        private bool _isSearchTextVisible;
        public bool IsSearchTextVisible
        {
            get { return _isSearchTextVisible; }
            set
            {
                _isSearchTextVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isBackButtonVisible;
        public bool IsBackButtonVisible
        {
            get { return _isBackButtonVisible; }
            set
            {
                _isBackButtonVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isNextButtonVisible;
        public bool IsNextButtonVisible
        {
            get { return _isNextButtonVisible; }
            set
            {
                _isNextButtonVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isDetecteLanguageBtnVisible = false;
        public bool IsDetecteLanguageBtnVisible
        {
            get { return _isDetecteLanguageBtnVisible; }
            set
            {
                _isDetecteLanguageBtnVisible = value;
                OnPropertyChanged();
            }
        }

        private string _nextBtnText = "Next";
        public string NextBtnText
        {
            get { return _nextBtnText; }
            set
            {
                _nextBtnText = value;
                OnPropertyChanged();
            }
        }

        private bool IsDetectLanguageVisible { get; set; } = true;
        private bool LanguagesCanAutoDetect { get; set; } = false;
        private bool ShowQuickStartSetup { get; set; } = true;

        private bool RememberSetup { get; set; } = false;

        private readonly IDataService _dataService;
        private readonly IAudioDeviceService _audioDeviceService;
        private readonly IMicrosoftTextToTextTranslator _microsoftTextToTextTranslator;
        private readonly IMicrosoftStandardVoiceSynthesizer _microsoftStandardVoiceSynthesizer;
        private readonly ILanguagesService _languagesService;
        private readonly IAppAnalytics _appAnalytics;

        public QuickStartViewModel
            (
            IDataService dataService,
            IAudioDeviceService audioDeviceService,
            IMicrosoftTextToTextTranslator microsoftTextToTextTranslator,
            IMicrosoftStandardVoiceSynthesizer microsoftStandardVoiceSynthesizer,
            ILanguagesService languagesService,
            IAppAnalytics appAnalytics
            )
        {
            _dataService = dataService;
            _audioDeviceService = audioDeviceService;
            _microsoftTextToTextTranslator = microsoftTextToTextTranslator;
            _microsoftStandardVoiceSynthesizer = microsoftStandardVoiceSynthesizer;
            _languagesService = languagesService;
            _appAnalytics = appAnalytics;
            MessagingCenter.Subscribe<string>(this, "ChangeSetup", (sender) =>
            {
                LoadQuickStartViews();
            });

            MessagingCenter.Subscribe<AutoDetectionMessage>(this, "LanguagesCanAutoDetect", (sender) =>
            {
                LanguagesCanAutoDetect = sender.LanguagesCanAutoDetect;
            });

            MessagingCenter.Subscribe<string>(this, "MoveToNextItem", (sender) =>
            {
                MoveToNextCarausel();
            });

            MessagingCenter.Subscribe<QuickStartSetupMessage>(this, "ShowQuickStartSetup", (sender) =>
            {
                if(sender.ShowQuickStartSetup == false)
                {
                    ShowQuickStartSetup = false;
                }
                else
                {
                    ShowQuickStartSetup = true;
                }
            });

            IsNextButtonVisible = true;

            LoadQuickStartViews();
        }

        private async void LoadQuickStartViews()
        {
            try
            {
                var rememberSetup = Settings.GetSetting(Settings.Setting.RememberSetup);

                if (!string.IsNullOrEmpty(rememberSetup) && Convert.ToBoolean(rememberSetup))
                {
                    Views = new ObservableRangeCollection<ContentView>
                {
                    new QuickStartSetupView { BindingContext = new QuickStartSetupViewModel(_dataService, _audioDeviceService, _languagesService,_appAnalytics) }
                };

                    RememberSetup = true;
                    IsSearchTextVisible = false;
                    IsBackButtonVisible = false;
                    IsNextButtonVisible = false;

                    CarouselPosition = 0;

                    await Task.Delay(2000);

                    SelectedView = null;

                    SelectedView = Views[CarouselPosition];
                }
                else
                {
                    //Initialize view models
                    var quickStartLanguageOneViewModel = new QuickStartLanguageOneViewModel(_dataService, _languagesService);
                    var quickStartLanguageTwoViewModel = new QuickStartLanguageTwoViewModel(_languagesService);
                    var autoDetectFlagsViewModel = new AutoDetectionFlagsViewModel(_dataService, _languagesService);
                    var autoDetectLanguagesViewModel = new AutoDetectionLanguagesViewModel(_microsoftTextToTextTranslator, _microsoftStandardVoiceSynthesizer, _languagesService);
                    var quickStartDeviceViewModel = new QuickStartDeviceViewModel(_audioDeviceService);
                    var quickStartSetupViewModel = new QuickStartSetupViewModel(_dataService, _audioDeviceService, _languagesService, _appAnalytics);

                    //Initialize views
                    var quickStartLanguageOneView = new QuickStartLanguageOneView { BindingContext = quickStartLanguageOneViewModel };
                    var quickStartLanguageTwoView = new QuickStartLanguageTwoView { BindingContext = quickStartLanguageTwoViewModel };
                    var autoDetectFlagsView = new AutoDetectionFlags { BindingContext = autoDetectFlagsViewModel };
                    var autoDetectLanguagesView = new AutoDetectionLanguages { BindingContext = autoDetectLanguagesViewModel };
                    var quickStartDeviceView = new QuickStartDeviceView { BindingContext = quickStartDeviceViewModel };
                    var quickStartSetupView = new QuickStartSetupView { BindingContext = quickStartSetupViewModel };

                    Views = new ObservableRangeCollection<ContentView>
                    {
                        quickStartLanguageOneView,
                        quickStartLanguageTwoView,
                        autoDetectFlagsView,
                        autoDetectLanguagesView,
                        quickStartDeviceView,
                        quickStartSetupView
                    };

                    RememberSetup = false;

                    await Task.Delay(500);

                    SelectedView = null;
                    CarouselPosition = 0;

                    await Task.Delay(500);

                    MessagingCenter.Instance.Send("", "QuickStartBringLanguageOneIntoView");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        private void SearchLanguage(string searchText)
        {
            switch (CarouselPosition)
            {
                case 0:
                    MessagingCenter.Instance.Send(searchText, "LanguageOneSearchText");
                    break;
                case 1:
                    MessagingCenter.Instance.Send(searchText, "LanguageTwoSearchText");
                    break;
                case 2:
                    MessagingCenter.Instance.Send(searchText, "CountrySearchText");
                    break;
            }
        }

        private void HandleViewChange(int carousel)
        {
            switch (carousel)
            {
                case 0:
                    InstructionText = "Select your language";
                    IsSearchTextVisible = true;
                    IsBackButtonVisible = false;
                    IsNextButtonVisible = true;
                    IsDetecteLanguageBtnVisible = false;
                    break;
                case 1:
                    InstructionText = "Select language you're translating to";
                    IsSearchTextVisible = true;
                    IsBackButtonVisible = true;
                    IsNextButtonVisible = true;
                    IsDetecteLanguageBtnVisible = true;
                    break;
                case 2:
                    InstructionText = "Select country/flag";
                    IsSearchTextVisible = true;
                    IsBackButtonVisible = true;
                    IsNextButtonVisible = true;
                    IsDetecteLanguageBtnVisible = false;
                    break;
                case 3:
                    InstructionText = "Detect language you're translating to";
                    IsSearchTextVisible = false;
                    IsBackButtonVisible = true;
                    IsNextButtonVisible = false;
                    IsDetecteLanguageBtnVisible = false;
                    break;
                case 4:
                    InstructionText = "Select your device";
                    IsSearchTextVisible = false;
                    IsBackButtonVisible = true;
                    IsNextButtonVisible = true;
                    IsDetecteLanguageBtnVisible = false;
                    break;
                case 5:
                    InstructionText = "";
                    IsSearchTextVisible = false;
                    IsBackButtonVisible = true;
                    IsNextButtonVisible = false;
                    IsDetecteLanguageBtnVisible = false;
                    break;
                default:
                    InstructionText = "";
                    IsSearchTextVisible = false;
                    IsBackButtonVisible = true;
                    IsNextButtonVisible = true;
                    IsDetecteLanguageBtnVisible = false;
                    break;
            }

            SelectedView = Views[CarouselPosition];
        }

        private async void MoveToNextCarausel()
        {
            try
            {
                if (NextBtnText == "Translate")
                {
                    await PopupNavigation.Instance.PopAsync();
                    MessagingCenter.Instance.Send("", "StartTranslation");
                }
                else
                {
                    if (CarouselPosition == 1 && IsDetectLanguageVisible)
                    {
                        IsDetectLanguageVisible = false;
                        CarouselPosition = 4;
                    }
                    else if (CarouselPosition == 2 && LanguagesCanAutoDetect)
                    {
                        IsDetectLanguageVisible = true;
                        CarouselPosition = 4;
                    }
                    else
                    {
                        CarouselPosition++;
                    }

                    if (ShowQuickStartSetup)
                    {
                        NextBtnText = "Next";
                    }
                    else
                    {
                        NextBtnText = "Translate";
                    }

                    BringIntoView();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private void MoveToPreviousCarausel()
        {
            if (CarouselPosition == 4 && !IsDetectLanguageVisible)
            {
                IsDetectLanguageVisible = true;
                CarouselPosition = 1;
            }
            else if (CarouselPosition == 4 && !LanguagesCanAutoDetect)
            {
                IsDetectLanguageVisible = false;
                CarouselPosition = 2;
            }
            else if (CarouselPosition == 4 && LanguagesCanAutoDetect)
            {
                IsDetectLanguageVisible = true;
                CarouselPosition = 2;
            }
            else
            {
                CarouselPosition--;
            }

            if (CarouselPosition != 4 && !ShowQuickStartSetup)
            {
                NextBtnText = "Next";
                ShowQuickStartSetup = true;
            }

            BringIntoView();
        }

        private async void BringIntoView()
        {
            await Task.Delay(300);
            if (CarouselPosition == 0)
                MessagingCenter.Instance.Send("", "QuickStartBringLanguageOneIntoView");
            if (CarouselPosition == 1)
                MessagingCenter.Instance.Send("", "QuickStartBringLanguageTwoIntoView");
            if(CarouselPosition == 3)
                MessagingCenter.Instance.Send("", "AutoDetectionLanguagesSelectFirstLanguage");
        }

        private async Task SkipQuickStart()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private void DetectLanguage()
        {
            if (IsDetectLanguageVisible)
            {
                _appAnalytics.CaptureCustomEvent("Detect Language launched",
                       new Dictionary<string, string>
                       {
                            {"User", App.CurrentUser.Email},
                            {"Organization", App.CurrentUser.Organization }

                       });
                CarouselPosition++;
            }
        }

        ICommand _nextCommand = null;

        public ICommand NextCommand
        {
            get
            {
                return _nextCommand ?? (_nextCommand =
                                          new Command(() => MoveToNextCarausel()));
            }
        }

        ICommand _backCommand = null;

        public ICommand BackCommand
        {
            get
            {
                return _backCommand ?? (_backCommand =
                                          new Command(() => MoveToPreviousCarausel()));
            }
        }

        ICommand _skipCommand = null;

        public ICommand SkipCommand
        {
            get
            {
                return _skipCommand ?? (_skipCommand =
                                          new Command(async () => await SkipQuickStart()));
            }
        }

        ICommand _detectLanguageCommand = null;

        public ICommand DetectLanguageCommand
        {
            get
            {
                return _detectLanguageCommand ?? (_detectLanguageCommand =
                                          new Command(() => DetectLanguage()));
            }
        }
    }
}
