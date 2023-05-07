using MvvmHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Translation.AppSettings;
using Translation.Core.Domain;
using Translation.Core.Interfaces;
using Translation.DataService.Interfaces;
using Translation.Helpers;
using Translation.Interface;
using Translation.Services.Languages;
using Translation.Styles;
using Translation.Utils;
using Xamarin.Forms;
using Language = Translation.Models.Language;

namespace Translation.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// True if LightTheme has been selected
        /// </summary>
        bool _isLightTheme;
        public bool IsLightTheme
        {
            get { return _isLightTheme; }
            set
            {
                _isLightTheme = value;
                OnPropertyChanged();
                if (IsLightTheme)
                {
                    IsDarkTheme = false;
                    IsSystemPreferredTheme = false;
                }
            }
        }

        /// <summary>
        /// True if DarkTheme has been selected
        /// </summary>
        bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get { return _isDarkTheme; }
            set
            {
                _isDarkTheme = value;
                OnPropertyChanged();
                if (IsDarkTheme)
                {
                    IsLightTheme = false;
                    IsSystemPreferredTheme = false;
                }
            }
        }

        /// <summary>
        /// True if SystemPreferredTheme has been selected
        /// </summary>
        bool _isSystemPreferredTheme;
        public bool IsSystemPreferredTheme
        {
            get { return _isSystemPreferredTheme; }
            set
            {
                _isSystemPreferredTheme = value;
                OnPropertyChanged();
                if (IsSystemPreferredTheme)
                {
                    IsLightTheme = false;
                    IsDarkTheme = false;
                }
            }
        }

        /// <summary>
        /// Selected default source language
        /// </summary>
        private Language _defaultSourceLanguage;
        public Language DefaultSourceLanguage
        {
            get { return _defaultSourceLanguage; }
            set
            {
                _defaultSourceLanguage = value;
                OnPropertyChanged();
                UpdateLanguages();
            }
        }

        /// <summary>
        /// Selected default target language
        /// </summary>
        private Language _defaultTargetLanguage;
        public Language DefaultTargetLanguage
        {
            get { return _defaultTargetLanguage; }
            set
            {
                _defaultTargetLanguage = value;
                OnPropertyChanged();
                UpdateLanguages();
            }
        }

        /// <summary>
        /// List of languages
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

        /// <summary>
        /// List of Audio Devices Available
        /// </summary>
        private List<AudioDevice> _audioDevices;
        public List<AudioDevice> AudioDevices
        {
            get { return _audioDevices; }
            set
            {
                _audioDevices = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Selected Audio Device
        /// </summary>
        private AudioDevice _selectedAudioDevice;
        public AudioDevice SelectedAudioDevice
        {
            get { return _selectedAudioDevice; }
            set
            {
                _selectedAudioDevice = value;
                OnPropertyChanged();
                if (SelectedAudioDevice != null)
                {
                    AppAnalytics.CaptureCustomEvent("Settings Changes",
                   new Dictionary<string, string> {
                        {"User", App.CurrentUser?.Email },
                        {"Organisation", App.CurrentUser?.Organization },
                        {"Action", "Audio Device Selection Changed" }
                   });
                    MessagingCenter.Instance.Send(SelectedAudioDevice, "ChangeIODevice");
                }
            }
        }


        IAppAnalytics AppAnalytics;
        IDataService DataService;
        private readonly IAudioDeviceService _audioDeviceService;
        private readonly ILanguagesService _languagesService;

        #endregion

        #region Constructor

        public SettingsViewModel(
            IAppAnalytics appAnalytics,
            IDataService dataService,
            IAudioDeviceService audioDeviceService,
            ILanguagesService languagesService
            )
        {
            AppAnalytics = appAnalytics;
            DataService = dataService;
            _audioDeviceService = audioDeviceService;
            _languagesService = languagesService;
            AppAnalytics.CaptureCustomEvent("SettingsPage Navigated");
            Languages = new List<Language>();
            MessagingCenter.Subscribe<string>(this, "UpdateLanguageLists", async (sender) =>
            {
                await ReloadLanguages();
            });

            LoadLanguages();
            LoadAudioDevices();
            GetThemeSetting();
        }

        private async Task ReloadLanguages()
        {
            await Task.Delay(1000);
            LoadLanguages();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to load languages
        /// </summary>
        async void LoadLanguages()
        {
            Languages = await _languagesService.GetSupportedLanguages();

            var defaultLanguages = await _languagesService.GetDefaultLanguages();

            var defaultSourceLanguage = defaultLanguages[EnumsConverter.ConvertToString(Settings.Setting.DefaultSourceLanguage)];
            var defaultTargetLanguage = defaultLanguages[EnumsConverter.ConvertToString(Settings.Setting.DefaultTargetLanguage)];

            var defaultLanguageOverridden = Settings.IsDefaultLanguageOverridden();
            if (defaultLanguageOverridden)
            {
                DefaultSourceLanguage = Languages.Where(s => s.Code.Equals(defaultSourceLanguage)).FirstOrDefault();
            }
            else
            {
                var organizationSettings = await DataService.GetOrganizationSettingsAsync();
                if (organizationSettings.Count != 0 && !string.IsNullOrEmpty(organizationSettings[0].LanguageCode) && organizationSettings[0].LanguageCode != "string")
                {
                    DefaultSourceLanguage = Languages.Where(c => c.Code == organizationSettings[0].LanguageCode).FirstOrDefault();
                }
                else
                {
                    DefaultSourceLanguage = Languages.Where(s => s.Code.Equals(defaultSourceLanguage)).FirstOrDefault();
                }
            }
            DefaultTargetLanguage = Languages.Where(s => s.Code.Equals(defaultTargetLanguage)).FirstOrDefault();
            Settings.AddSetting(Settings.Setting.IsDefaultLanguageOverridden, true.ToString());
        }

        private async void LoadAudioDevices()
        {
            var deviceAddress = Settings.GetSetting(Settings.Setting.DeviceAddress);
            AudioDevices = await _audioDeviceService.GetIODevices();

            if (deviceAddress != null && AudioDevices != null && AudioDevices.Any())
            {
                var selectedDevice = AudioDevices.FirstOrDefault(s => s.OutputDevice.Address == deviceAddress);

                if (selectedDevice != null)
                    SelectedAudioDevice = selectedDevice;
                else
                    SelectedAudioDevice = AudioDevices[0];
            }
            else
            {
                SelectedAudioDevice = AudioDevices[0];
            }
        }

        /// <summary>
        /// Function to get the current user's Theme preference
        /// </summary>
        void GetThemeSetting()
        {
            string theme = Settings.GetSetting(Settings.Setting.AppTheme);

            if (!string.IsNullOrEmpty(theme))
            {
                var appTheme = EnumsConverter.ConvertToEnum<Settings.Theme>(theme);

                switch (appTheme)
                {
                    case Settings.Theme.LightTheme:
                        IsLightTheme = true;
                        break;
                    case Settings.Theme.DarkTheme:
                        IsDarkTheme = true;
                        break;
                    case Settings.Theme.SystemPreferred:
                        IsSystemPreferredTheme = true;
                        break;
                    default:
                        IsSystemPreferredTheme = true;
                        break;
                }
            }
            else
            {
                IsSystemPreferredTheme = true;
            }
        }

        /// <summary>
        /// Function to change user's Theme in realtime 
        /// when user chooses a  different Theme preference
        /// </summary>
        void ChangeTheme(string theme)
        {
            var appTheme = EnumsConverter.ConvertToEnum<Settings.Theme>(theme);

            switch (appTheme)
            {
                case Settings.Theme.LightTheme:
                    IsLightTheme = true;
                    ThemeHelper.ChangeToLightTheme();
                    break;
                case Settings.Theme.DarkTheme:
                    IsDarkTheme = true;
                    ThemeHelper.ChangeToDarkTheme();
                    break;
                case Settings.Theme.SystemPreferred:
                    IsSystemPreferredTheme = true;
                    ThemeHelper.ChangeToSystemPreferredTheme();
                    break;
                default:
                    IsSystemPreferredTheme = true;
                    ThemeHelper.ChangeToSystemPreferredTheme();
                    break;
            }

            MessagingCenter.Instance.Send("ChangeDashboardImage", "ChangeDashboardImage");
        }

        private void UpdateLanguages()
        {
            UpdateLanguageOne();
            UpdateLanguageTwo();
        }

        private void UpdateLanguageOne()
        {
            if (DefaultSourceLanguage != null)
            {
                MessagingCenter.Instance.Send(DefaultSourceLanguage, "UpdateLanguageOne");
                _languagesService.SetDefaultLanguage(DefaultSourceLanguage.Code, Settings.Setting.DefaultSourceLanguage);
                Settings.AddSetting(Settings.Setting.IsDefaultLanguageOverridden, true.ToString());
            }
        }

        private void UpdateLanguageTwo()
        {
            if (DefaultTargetLanguage != null)
            {
                MessagingCenter.Instance.Send(DefaultTargetLanguage, "UpdateLanguageTwo");
                _languagesService.SetDefaultLanguage(DefaultTargetLanguage.Code, Settings.Setting.DefaultTargetLanguage);
                Settings.AddSetting(Settings.Setting.IsDefaultLanguageOverridden, true.ToString());
            }
        }

        /// <summary>
        /// Command to change theme
        /// </summary>
        ICommand _themeChangeCommand = null;

        public ICommand ThemeChangeCommand
        {
            get
            {
                return _themeChangeCommand ?? (_themeChangeCommand =
                                          new Command((object obj) => ChangeTheme((string)obj)));
            }
        }

        #endregion
    }
}
