using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using System;
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
using Translation.Utils;
using Xamarin.Forms;
using Language = Translation.Models.Language;

namespace Translation.ViewModels
{
    public class QuickStartSetupViewModel : BaseViewModel
    {
        private Language _languageOne;

        public Language LanguageOne
        {
            get { return _languageOne; }
            set
            {
                _languageOne = value;
                OnPropertyChanged();
            }
        }

        private Language _languageTwo;

        public Language LanguageTwo
        {
            get { return _languageTwo; }
            set
            {
                _languageTwo = value;
                OnPropertyChanged();
            }
        }

        private AudioDevice _selectedAudioDevice;

        public AudioDevice SelectedAudioDevice
        {
            get { return _selectedAudioDevice; }
            set
            {
                _selectedAudioDevice = value;
                OnPropertyChanged();
            }
        }

        private bool _rememberSetup;

        public bool RememberSetup
        {
            get { return _rememberSetup; }
            set
            {
                _rememberSetup = value;
                OnPropertyChanged();
            }
        }

        private readonly IDataService _dataService;
        private readonly IAudioDeviceService _audioDeviceService;
        private readonly ILanguagesService _languagesService;
        private readonly IAppAnalytics _appAnalytics;

        public QuickStartSetupViewModel(IDataService dataService, IAudioDeviceService audioDeviceService, ILanguagesService languagesService, IAppAnalytics appAnalytics)
        {
            _dataService = dataService;
            _audioDeviceService = audioDeviceService;
            _languagesService = languagesService;
            _appAnalytics = appAnalytics;

            RememberSetup = false;

            MessagingCenter.Subscribe<AudioDevice>(this, "ChangeIODevice", (sender) =>
            {
                SelectedAudioDevice = sender;
            });

            MessagingCenter.Subscribe<Language>(this, "UpdateLanguageOne", (sender) =>
            {
                LanguageOne = sender;
            });

            MessagingCenter.Subscribe<Language>(this, "UpdateLanguageTwoSetup", (sender) =>
            {
                LanguageTwo = sender;
            });

            MessagingCenter.Subscribe<string>(this, "UpdateLanguageLists", async (sender) =>
            {
                await ReloadLanguages();
            });

            LoadUserSetup();
        }

        private async Task ReloadLanguages()
        {
            await Task.Delay(1000);
            LoadUserSetup();
        }

        private async void LoadUserSetup()
        {
            var rememberSetup = Settings.GetSetting(Settings.Setting.RememberSetup);

            if (!string.IsNullOrEmpty(rememberSetup) && Convert.ToBoolean(rememberSetup))
            {
                RememberSetup = true;

                var languages = await _languagesService.GetSupportedLanguages();
                var defaultLanguages = await _languagesService.GetDefaultLanguages();
                var defaultSourceLanguage = defaultLanguages[EnumsConverter.ConvertToString(Settings.Setting.DefaultSourceLanguage)];
                var defaultTargetLanguage = defaultLanguages[EnumsConverter.ConvertToString(Settings.Setting.DefaultTargetLanguage)];

                var defaultLanguageOverridden = Settings.IsDefaultLanguageOverridden();
                if (defaultLanguageOverridden)
                {
                    LanguageOne = languages.Where(s => s.Code.Equals(defaultSourceLanguage)).FirstOrDefault();
                }
                else
                {
                    var organizationSettings = await _dataService.GetOrganizationSettingsAsync();
                    if (organizationSettings.Count != 0 && !string.IsNullOrEmpty(organizationSettings[0].LanguageCode) && organizationSettings[0].LanguageCode != "string")
                    {
                        LanguageOne = languages.Where(c => c.Code == organizationSettings[0].LanguageCode).FirstOrDefault();
                    }
                    else
                    {
                        LanguageOne = languages.Where(s => s.Code.Equals(defaultSourceLanguage)).FirstOrDefault();
                    }
                }

                LanguageTwo = languages.Where(s => s.Code.Equals(defaultTargetLanguage)).FirstOrDefault();

                var deviceAddress = Settings.GetSetting(Settings.Setting.DeviceAddress);
                var devices = await _audioDeviceService.GetIODevices();

                if (deviceAddress != null)
                {
                    var selectedDevice = devices.FirstOrDefault(s => s.OutputDevice.Address == deviceAddress);

                    if (selectedDevice != null)
                        SelectedAudioDevice = selectedDevice;
                    else
                        SelectedAudioDevice = devices[0];
                }
                else
                {
                    SelectedAudioDevice = devices[0];
                }

                MessagingCenter.Instance.Send(LanguageOne, "UpdateLanguageOne");
                MessagingCenter.Instance.Send(LanguageTwo, "UpdateLanguageTwo");
                MessagingCenter.Instance.Send(SelectedAudioDevice, "ChangeIODevice");
            }
        }

        private async void StartTranslation()
        {
            _appAnalytics.CaptureCustomEvent("Quick start used",
                     new Dictionary<string, string>
                     {
                            {"User", App.CurrentUser.Email},
                            {"Organization", App.CurrentUser.Organization }

                     });
            if (RememberSetup)
            {
                _languagesService.SetDefaultLanguage(LanguageOne.Code, Settings.Setting.DefaultSourceLanguage);

                _languagesService.SetDefaultLanguage(LanguageTwo.Code, Settings.Setting.DefaultTargetLanguage);
                Settings.AddSetting(Settings.Setting.IsDefaultLanguageOverridden, true.ToString());

                Settings.AddSetting(Settings.Setting.DeviceAddress, SelectedAudioDevice.OutputDevice.Address);
                Settings.AddSetting(Settings.Setting.RememberSetup, true.ToString());
            }
            else
            {
                Settings.AddSetting(Settings.Setting.RememberSetup, false.ToString());
            }

            MessagingCenter.Instance.Send(LanguageOne, "UpdateLanguageOne");
            MessagingCenter.Instance.Send(LanguageTwo, "UpdateLanguageTwo");
            await PopupNavigation.Instance.PopAsync();
            MessagingCenter.Instance.Send("", "StartTranslation");
        }

        private void ChangeSetup()
        {
            Settings.AddSetting(Settings.Setting.RememberSetup, false.ToString());
            MessagingCenter.Instance.Send("", "ChangeSetup");
        }

        ICommand _startTranslationCommand = null;

        public ICommand StartTranslationCommand
        {
            get
            {
                return _startTranslationCommand ?? (_startTranslationCommand = new Command(() => StartTranslation()));
            }
        }

        ICommand _changeSetupCommand = null;

        public ICommand ChangeSetupCommand
        {
            get
            {
                return _changeSetupCommand ?? (_changeSetupCommand = new Command(() => ChangeSetup()));
            }
        }
    }
}
