using MediaManager;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Translation.AppSettings;
using Translation.Core.Domain;
using Translation.Core.Interfaces;
using Translation.Core.Services.TranslationService;
using Translation.DataService.Models;
using Translation.Hmac;
using Translation.Messages;
using Translation.Models;
using Translation.Services.Auth;
using Translation.Styles;
using Translation.Utils;
using Translation.Views.Components.DashboardComponents;
using Translation.Views.Components.Popups;
using Translation.Views.Pages.ImmersiveReader;
using Xamarin.Essentials;
using Xamarin.Forms;
using CoreLanguage = Translation.Core.Domain.Language;
using Language = Translation.Models.Language;

namespace Translation.ViewModels
{
    public partial class DashboardViewModel
    {
        #region Methods

        private async Task UpdateLanguageLists()
        {
            await Task.Delay(1000);
            InitializeAutoDetectLanguages();
            LoadLanguages();
        }

        private void SetAutoDetectLanguageVisibility()
        {
            if (_autodetectSecondaryLanguages != null)
            {
                if (_autodetectSecondaryLanguages.Count == 1)
                {
                    IsSecondaryLanguageVisible = true;
                    SecondaryLanguage = _autodetectSecondaryLanguages[0];
                    DisplaySecondaryLanguage = _autodetectSecondaryLanguages[0];
                }
                else
                    IsSecondaryLanguageVisible = false;
            }
            else if (_autodetectSecondaryLanguages == null && SecondaryLanguage != null)
            {
                IsSecondaryLanguageVisible = true;
            }
        }

        private void ResetAutoDetectLanguages()
        {
            if (!CannotBeAutoDetected)
            {
                if (SecondaryLanguage != null && _autodetectSecondaryLanguages.Any(s => s.Code == SecondaryLanguage.Code))
                {
                    _autodetectSecondaryLanguages = new List<Language> { SecondaryLanguage };
                }
            }
            else if (CannotBeAutoDetected && SecondaryLanguage != null)
            {
                _autodetectSecondaryLanguages = null;
                IsSecondaryLanguageVisible = true;
            }
            else if (!CannotBeAutoDetected && !IsSecondaryLanguageVisible)
            {
                LoadLanguages();
                IsSecondaryLanguageVisible = true;
            }
        }

        private void ResetSecondaryLanguage()
        {
            if (SecondaryLanguage != null && AutoDetectLanguages.Exists(l => l.Code == SecondaryLanguage.Code))
            {
                _autodetectSecondaryLanguages = new List<Language> { SecondaryLanguage };
            }
            else
            {
                _autodetectSecondaryLanguages = new List<Language>();
            }
        }

        private async Task MaybeLaunchQuickStart()
        {
            try
            {
                if (Settings.IsUserLoggedIn())
                    await PopupNavigation.Instance.PushAsync(new QuickStart());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        private async Task LoadAudioDevices()
        {
            var deviceAddress = Settings.GetSetting(Settings.Setting.DeviceAddress);
            var audioDevices = await _audioDeviceService.GetIODevices();

            if (deviceAddress != null && audioDevices != null && audioDevices.Any())
            {
                var selectedDevice = audioDevices.FirstOrDefault(s => s.OutputDevice.Address == deviceAddress);

                if (selectedDevice != null)
                    _selectedDevice = selectedDevice;
                else
                    _selectedDevice = audioDevices[0];
            }
            else
            {
                _selectedDevice = audioDevices[0];
            }
        }

        private async Task GetOrganizationSettings()
        {
            var orgSettings = await _organizationSettingsService.GetOrganizationSettings();
            AllowExplicitContent = orgSettings.AllowExplicitContent;
            EnableSessionTags = orgSettings.EnableSessionTags;
            CanSyncData = Convert.ToBoolean(Settings.GetSetting(Settings.Setting.DataConsentStatus));
            await GetOrganizationQuestions();

            if (!IsTranslating)
            {
                LoadLanguages();
            }
        }

        private void InitializeAutoDetectLanguages()
        {
            AutoDetectLanguages = _languagesService.GetAutoDetectSupportedLanguages();
        }

        private void LoadDashboardImagImage()
        {
            DashboardImage = ThemeHelper.ImagePath("dashboard");
        }

        /// <summary>
        /// Method to get language
        /// </summary>
        private async void LoadLanguages()
        {
            try
            {
                if (!IsTranslating)
                {
                    Languages = await _languagesService.GetSupportedLanguages();
                    PrimaryLanguageList = Languages;
                    SecondaryLanguageList = Languages;

                    var defaultLanguages = await _languagesService.GetDefaultLanguages();
                    var defaultSourceLanguage = defaultLanguages[EnumsConverter.ConvertToString(Settings.Setting.DefaultSourceLanguage)];
                    var defaultTargetLanguage = defaultLanguages[EnumsConverter.ConvertToString(Settings.Setting.DefaultTargetLanguage)];

                    var defaultLanguageOverridden = Settings.IsDefaultLanguageOverridden();
                    if (defaultLanguageOverridden)
                    {
                        PrimaryLanguage = Languages.Where(s => s.Code.Equals(defaultSourceLanguage)).FirstOrDefault();
                    }
                    else
                    {
                        var organizationSettings = await _dataservice.GetOrganizationSettingsAsync();
                        if (organizationSettings.Count() != 0 && !string.IsNullOrEmpty(organizationSettings[0].LanguageCode) && organizationSettings[0].LanguageCode != "string")
                        {
                            PrimaryLanguage = Languages.Where(c => c.Code == organizationSettings[0].LanguageCode).FirstOrDefault();
                        }
                        else
                        {
                            PrimaryLanguage = Languages.Where(s => s.Code.Equals(defaultSourceLanguage)).FirstOrDefault();
                        }
                    }

                    if (SecondaryLanguage == null || !Languages.Exists(l => l.Code == SecondaryLanguage.Code))
                    {
                        SecondaryLanguage = SecondaryLanguageList.Where(s => s.Code.Equals(defaultTargetLanguage)).FirstOrDefault();
                    }

                    var recentLanguages = await _dataservice.GetRecentLanguages();

                    if (recentLanguages != null && recentLanguages.Any())
                    {
                        if (!RecentLanguages.Any())
                        {
                            RecentLanguages = new List<Language>();

                            foreach (var item in recentLanguages)
                            {
                                if (!RecentLanguages.Any(s => s.Code == item.LanguageCode))
                                {
                                    var language = Languages.First(s => s.Code == item.LanguageCode);
                                    if (language != null)
                                        RecentLanguages.Add(language);
                                }
                            }
                        }
                    }
                    else
                    {
                        await _dataservice.AddRecentangauages(PrimaryLanguage.Code);
                        RecentLanguages.Add(PrimaryLanguage);
                        await _dataservice.AddRecentangauages(SecondaryLanguage.Code);
                        RecentLanguages.Add(SecondaryLanguage);
                    }

                    GetNeuralVoiceSetting();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        private void GetNeuralVoiceSetting()
        {
            var useNerualVoice = Settings.GetSetting(Settings.Setting.UseNerualVoice);
            if (!string.IsNullOrEmpty(useNerualVoice))
            {
                _useNerualVoice = Convert.ToBoolean(useNerualVoice);
            }
            else
            {
                Settings.AddSetting(Settings.Setting.UseNerualVoice, true.ToString());
                _useNerualVoice = true;
            }
        }

        /// <summary>
        /// Method to check if Mic has permissions
        /// </summary>
        private async Task<bool> IsPermissionsGranted()
        {
            if (IsMicEnabled && IsStorageAccessEnabled)
                return true;

            IsMicEnabled = await _microphoneService.GetPermissionAsync();

            if (!IsMicEnabled)
            {
                await PermissionsHelper.AskForPermissions();
            }

            IsStorageAccessEnabled = await FileAccessUtility.CheckStoragePermissions();

            if (!IsStorageAccessEnabled)
            {
                await PermissionsHelper.AskForStoragePermissions();
            }

            return IsMicEnabled && IsStorageAccessEnabled;
        }

        private async Task StartStopTranslation()
        {
            if (IsTranslating)
            {
                await StopTranslating();
            }
            else
            {
                Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Setting up Translator.", seconds: 1);
                Dialogs.ProgressDialog.Show();

                var userSettings = await Settings.CurrentUser();

                //Send a SignalR Message to track this session
                await _signalRService.ConnectSignalR();
                _signalRService.SignalRMessageReceived += SignalRServiceMessageReceived;
                await _signalRService.SendSignalRMessage(new SignalRTranslateMessage { UserEmail = userSettings.Email });
            }
        }

        private async void SignalRServiceMessageReceived(SignalRTranslateMessage obj)
        {
            if (IsTranslating)
            {
                _signalRService.SignalRMessageReceived -= SignalRServiceMessageReceived;
                return;
            }

            if (obj.ConnectionId == _signalRService.ConnectionId)
            {
                if (obj.CanTranslate)
                    await Translate();
                else
                {
                    if (Dialogs.ProgressDialog.IsShowing)
                    {
                        Dialogs.ProgressDialog.Hide();
                    }
                    _signalRService.SignalRMessageReceived -= SignalRServiceMessageReceived;
                    Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Your licence is currently in use in another session.", seconds: 5000);
                }
            }
            else
            {
                if (Dialogs.ProgressDialog.IsShowing)
                {
                    Dialogs.ProgressDialog.Hide();
                }
            }
        }

        private async Task SaveSessionMetadata(SessionMetaDataMessage message)
        {
            await PopupNavigation.Instance.PopAsync();

            if (!string.IsNullOrEmpty(message.SessionName))
            {
                IsFirstTranslation = false;
                SessionName = message.SessionName;

                TagsCount = 0;
                SessionTags = new List<Models.SessionTag>();
                CustomTags = new List<string>();

                if (message.SessionTags != null && message.SessionTags.Any())
                {
                    SessionTags = message.SessionTags;
                    TagsCount += SessionTags.Count();
                }

                if (message.CustomTags != null && message.CustomTags.Any())
                {
                    CustomTags = message.CustomTags;
                    TagsCount += CustomTags.Count();
                }

                await SaveSession().ConfigureAwait(false);
            }
        }

        private async Task SaveSession()
        {
            await CreateSession().ConfigureAwait(true);

            if (!RecentLanguages.Any(s => s.Code == PrimaryLanguage.Code))
            {
                RecentLanguages.Add(PrimaryLanguage);
                await _dataservice.AddRecentangauages(PrimaryLanguage.Code);
            }

            if (!RecentLanguages.Any(s => s.Code == SecondaryLanguage.Code))
            {
                RecentLanguages.Add(SecondaryLanguage);
                await _dataservice.AddRecentangauages(SecondaryLanguage.Code);
            }

            SessionName = "";
            SessionNumber = "";
        }

        private async Task EditSessionMetadata()
        {
            SessionMetaDataMessage message = new SessionMetaDataMessage
            {
                SessionName = SessionName,
                CustomTags = CustomTags,
                SessionTags = SessionTags
            };
            await PopupNavigation.Instance.PushAsync(new SessionMetaDataPopup());
            MessagingCenter.Instance.Send(message, "EditSessionMetadata");
        }

        private async Task UpdateSessionMetadata(SessionMetaDataMessage message)
        {
            await PopupNavigation.Instance.PopAsync();

            if (!string.IsNullOrEmpty(message.SessionName))
            {
                IsFirstTranslation = false;
                SessionName = message.SessionName;

                TagsCount = 0;

                if (message.SessionTags != null && message.SessionTags.Any())
                {
                    SessionTags = message.SessionTags;
                    TagsCount += SessionTags.Count();
                }

                if (message.CustomTags != null && message.CustomTags.Any())
                {
                    CustomTags = message.CustomTags;
                    TagsCount += CustomTags.Count();
                }
            }
        }

        /// <summary>
        /// Method to initiate first translation
        /// And hide instructions screen
        /// </summary>
        private async Task AnimateAndStartTranslation()
        {
            Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Setting up Translator.", seconds: 1);
            Dialogs.ProgressDialog.Show();

            ResetSecondaryLanguage();

            IsFirstTranslation = false;

            var userSettings = await Settings.CurrentUser();

            //Send a SignalR Message to track this session
            await _signalRService.ConnectSignalR();
            _signalRService.SignalRMessageReceived += SignalRServiceMessageReceived;
            await _signalRService.SendSignalRMessage(new SignalRTranslateMessage { UserEmail = userSettings.Email });
        }

        /// <summary>
        /// Method to start/end translation
        /// </summary>
        private async Task Translate()
        {
            try
            {
                if (await IsPermissionsGranted())
                {
                    if (IsMicrophoneMute)
                        IsMicrophoneMute = false;

                    if (string.IsNullOrEmpty(ADB2CAuthenticationService.IdToken))
                    {
                        await ADB2CAuthenticationService.Instance.AttemptSilentLogin().ConfigureAwait(true);
                    }

                    var settings = await Settings.CurrentUser();
                    var usage = await _dataservice.GetUsageLimit();

                    bool hasUnlimitedLicense = usage.OrganizationLicensingType.ToLower() == "postpaid";
                    if (usage.OrganizationTranslationLimitExceeded == true && !hasUnlimitedLicense)
                    {
                        //Show user error about exceeding translation minutes
                        //Reset translation button UI

                        Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Your translation minutes have been depleted. Kindly contact your administrator.", seconds: 5000);
                        IsTranslating = false;
                        ShowSwitchButton = false;
                        IsPlaying = false;
                        return;
                    }

                    _maxUserSessionTime = usage.UserMaxSessionTime;

                    if (settings.DataConsentStatus == true)
                    {
                        if (usage.OrganizationStorageLimitExceeded && !hasUnlimitedLicense)
                        {
                            CanSyncData = false;
                        }
                    }
                    else
                    {
                        CanSyncData = false;
                    }

                    await StartTranslating();

                    if (ChatList.Any())
                    {
                        ChatList.Clear();
                    }
                }
                else
                {
                    await IsPermissionsGranted();
                }
            }
            catch (Exception ex)
            {
                Dialogs.ProgressDialog.Hide();
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        /// <summary>
        /// Method to switch speakers
        /// </summary>
        private async Task SwitchSpeakers()
        {
            TranslateBtnText = "Switching...";
            _audioNotDetectedTimer.Stop();
            _audioNotDetectedTimer.Start();
            await _translator.SwitchSpeakers();
            SwitchLanguages();
            TranslateBtnText = "End Session";
        }

        private void SwitchLanguages()
        {
            var primaryLanguage = DisplaySecondaryLanguage;
            var secondaryLanguage = DisplayPrimaryLanguage;

            DisplayPrimaryLanguage = primaryLanguage;
            DisplaySecondaryLanguage = secondaryLanguage;
        }

        /// <summary>
        /// Method to start translation
        /// </summary>
        private async Task StartTranslating()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    _autodetectSecondaryLanguages = null;
                    MessagingCenter.Instance.Send("", "HideTabBar");
                    IsSessionMetaDataVisible = true;
                    await Task.Delay(1000);

                    SessionNumber = "...";
                    await GetSessionNumber();

                    List<string> participantLanguageCodes = new List<string>();
                    if (_autodetectSecondaryLanguages != null && _autodetectSecondaryLanguages.Any())
                    {
                        participantLanguageCodes = new List<string>();
                        foreach (var language in _autodetectSecondaryLanguages)
                        {
                            participantLanguageCodes.Add(language.Code);
                        }
                    }
                    else if (SecondaryLanguage != null)
                    {
                        participantLanguageCodes.Add(SecondaryLanguage.Code);
                    }
                    else if (SecondaryLanguage == null)
                    {
                        _appAnalytics.CaptureCustomEvent("Invalid Sessions",
           new Dictionary<string, string> {
                            { "User", App.CurrentUser?.Email },
                            { "Organization", App.CurrentUser?.Organization }});
                        Dialogs.HandleDialogMessage(Dialogs.DialogMessage.UndefinedError);
                        await StopTranslating();
                        return;
                    }
                    participantLanguageCodes.Add(PrimaryLanguage.Code);

                    //Check if languages can be auto-detected
                    CannotBeAutoDetected = true;
                    List<bool> autoDetectStatus = new List<bool>();
                    foreach (var code in participantLanguageCodes)
                    {
                        autoDetectStatus.Add(AutoDetectLanguages.Any(l => l.Code == code));
                    }

                    CannotBeAutoDetected = autoDetectStatus.Contains(false);

                    if (CannotBeAutoDetected)
                    {
                        ShowSwitchButton = true;
                        IsSecondaryLanguageVisible = true;
                    }
                    else
                    {
                        ShowSwitchButton = false;
                        SetAutoDetectLanguageVisibility();
                    }

                    SessionBillableSeconds = 0;
                    CrossMediaManager.Current.Init();
                    CrossMediaManager.Current.RepeatMode = MediaManager.Playback.RepeatMode.Off;
                    FileAccessUtility.DeleteDirectory("Recordings");
                    TimeCounter = "00:00";
                    IsTranslating = true;
                    StartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    _audioFilePath = FileAccessUtility.ReturnFilePath($"{StartTime}.wav");

                    PrimaryLanguage.SetPreferredVoice(VoiceType.NeuralVoice);
                    SecondaryLanguage.SetPreferredVoice(VoiceType.NeuralVoice);

                    _translator = InitializeTranslator();

                    if (_translator == null)
                        return;

                    SubscribeEvents();

                    var primaryLanguage = new CoreLanguage
                    {
                        Code = PrimaryLanguage.Code,
                        DisplayName = PrimaryLanguage.DisplayName,
                        Name = PrimaryLanguage.Name,
                        UseNeuralVoice = PrimaryLanguage.UseNeuralVoice,
                        VoiceName = PrimaryLanguage.VoiceName,
                        Voice = new Voice { Code = PrimaryLanguage.Voice, IsNeuralVoice = true }
                    };

                    if (CannotBeAutoDetected)
                    {
                        var secondaryLanguage = new CoreLanguage
                        {
                            Code = SecondaryLanguage.Code,
                            DisplayName = SecondaryLanguage.DisplayName,
                            Name = SecondaryLanguage.Name,
                            UseNeuralVoice = SecondaryLanguage.UseNeuralVoice,
                            VoiceName = SecondaryLanguage.VoiceName,
                            Voice = new Voice { Code = SecondaryLanguage.Voice, IsNeuralVoice = true }
                        };

                        await _translator.Translate
                            (
                                primaryLanguage,
                                secondaryLanguage,
                                _audioFilePath,
                                AllowExplicitContent,
                                _selectedDevice,
                                Constants.CognitiveServicesApiKey,
                                Constants.CognitiveServicesRegion
                            );
                    }
                    else
                    {
                        if (_autodetectSecondaryLanguages == null && SecondaryLanguage != null)
                        {
                            _autodetectSecondaryLanguages = new List<Language> { SecondaryLanguage };
                        }
                        if (_autodetectSecondaryLanguages != null)
                        {
                            var secondaryLanguages = new List<CoreLanguage>();

                            foreach (var item in _autodetectSecondaryLanguages)
                            {
                                secondaryLanguages.Add(new CoreLanguage
                                {
                                    Code = item.Code,
                                    DisplayName = item.DisplayName,
                                    Name = item.Name,
                                    UseNeuralVoice = item.UseNeuralVoice,
                                    VoiceName = item.VoiceName,
                                    Voice = new Voice { Code = item.Code, IsNeuralVoice = true }
                                });
                            }

                            await _translator.AutoDetectTranslate
                            (
                                primaryLanguage,
                                null,
                                _audioFilePath,
                                AllowExplicitContent,
                                _selectedDevice,
                                Constants.CognitiveServicesApiKey,
                                Constants.CognitiveServicesRegion,
                                secondaryLanguages
                            );
                        }
                        else
                        {
                            _appAnalytics.CaptureCustomEvent("Invalid Sessions",
           new Dictionary<string, string> {
                            { "User", App.CurrentUser?.Email },
                            { "Organization", App.CurrentUser?.Organization }});
                            Dialogs.HandleDialogMessage(Dialogs.DialogMessage.UndefinedError);
                            await StopTranslating();
                            return;
                        }

                        _microsoftTextToSpeechProvider.TranscriptionResultReady += MicrosoftTextToSpeechProvider_OnTranscriptionResultReady;
                        _microsoftTextToSpeechProvider.TranslationSpeechReady += MicrosoftTextToSpeechProvider_OnTranslationSpeechReady;
                    }

                    _audioNotDetectedTimer.Start();
                    _translationTimer.Start();
                    _translator.UnMute();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
                }

                if (Dialogs.ProgressDialog.IsShowing)
                {
                    Dialogs.ProgressDialog.Hide();
                }
            });
        }

        public ITranslator InitializeTranslator()
        {
            //if (PrimaryLanguage.Code == "ur-PK" || SecondaryLanguage.Code == "ur-PK")
            //{
            //    if (PrimaryLanguage.UseNeuralVoice && SecondaryLanguage.UseNeuralVoice)
            //    {
            //        PrimaryLanguage.SetPreferredVoice(VoiceType.NeuralVoice);
            //        SecondaryLanguage.SetPreferredVoice(VoiceType.NeuralVoice);
            //        return new GoogleTranslationProvider(_audioRecorder, _audioFileSaver, Constants.GoogleJsonCredentials);
            //    }
            //    else
            //    {
            //        string language;
            //        if (!PrimaryLanguage.UseNeuralVoice)
            //            language = PrimaryLanguage.Name;
            //        else
            //            language = SecondaryLanguage.Name;
            //        Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, $"{language} is not supported for Urdu translation. Select a different language or dialect.", backgroundColor: "#FF0000");
            //        return null;
            //    }
            //}

            //Return microsoftTranslationProvider for all languages, including Urdu

            return new MicrosoftTranslationProvider(_audioRecorder, _audioFileSaver);
        }

        private async Task GetSessionNumber()
        {
            SessionNumber sessionNumber = new SessionNumber() { ReferenceNumber = "N/A" };
            try
            {
                sessionNumber = await _sessionNumberService.GetSessionNumber();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }

            SessionNumber = sessionNumber.ReferenceNumber;
        }

        private void SubscribeEvents()
        {
            _translator.PartialResultReady += OnPartialResultReady;
            _translator.TranscriptionResultReady += OnTranscriptionResultReady;
            _translator.TranslationSpeechReady += OnTranslationSpeechReady;
            _translator.TranslationCancelled += OnTranslationCancelled;
        }

        private async void OnTranslationSpeechReady(TranslationResult obj)
        {
            try
            {
                if (obj != null && obj.AudioResult != null)
                {
                    _translator.Mute();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Code to run on the main thread
                        PlayAudio(obj.AudioResult);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        private void OnTranslationCancelled(TranslationCancelled obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.Reason))
            {
                Debug.WriteLine(obj.Reason);
            }
        }

        private async void OnTranscriptionResultReady(TranslationResult obj)
        {
            try
            {
                if (obj != null && !string.IsNullOrEmpty(obj.TranslatedText))
                {
                    _audioNotDetectedTimer.Stop();
                    _audioNotDetectedTimer.Start();
                    var translationResult = await JsonConverter.DuplicateObject<TranslationResultText>(obj);
                    translationResult.IsComplete = true;
                    HandleChatMessage(translationResult);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        private async void OnPartialResultReady(TranslationResult obj)
        {
            try
            {
                if (obj != null && !string.IsNullOrEmpty(obj.TranslatedText))
                {
                    var translationResult = await JsonConverter.DuplicateObject<TranslationResultText>(obj);
                    translationResult.IsComplete = false;
                    HandleChatMessage(translationResult);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        /// <summary>
        /// Method to stop translation
        /// </summary>
        private async Task StopTranslating()
        {
            Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "Ending Session.");
            Dialogs.ProgressDialog.Show();

            try
            {
                if (_signalRService != null)
                {
                    _signalRService.SignalRMessageReceived -= SignalRServiceMessageReceived;
                    await _signalRService.DisconnectSignalR();
                }

                ClearAudioResponseQueue();
                await CrossMediaManager.Current.Stop();
                CrossMediaManager.Current.Dispose();
                UnsubscribeEvents();

                if (_audioNotDetectedTimer != null)
                    _audioNotDetectedTimer.Stop();

                if (_translationTimer != null)
                    _translationTimer.Stop();

                IsTranslating = false;
                ResetAutoDetectLanguages();
                ShowSwitchButton = false;
                IsPlaying = false;

                if (CannotBeAutoDetected)
                {
                    if (_translator != null)
                        await _translator.StopAndResetSpeechRecognizer().ConfigureAwait(true);
                }
                else
                {
                    if (_translator != null)
                        await _translator.StopAndResetAutoDetectSpeechRecognizer().ConfigureAwait(true);
                }

                if (ChatList != null && ChatList.Any())
                {
                    if (EnableSessionTags)
                        await PopupNavigation.Instance.PushAsync(new SessionMetaDataPopup());
                    else
                        await SaveSession();
                }

                if (IsMicrophoneMute)
                    IsMicrophoneMute = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }

            DisplayPrimaryLanguage = PrimaryLanguage;
            DisplaySecondaryLanguage = SecondaryLanguage;

            MessagingCenter.Instance.Send("", "ShowTabBar");
            MessagingCenter.Instance.Send("", "ClearSessionMetadata");
            TagsCount = 0;
            IsSessionMetaDataVisible = false;
            HaloColor = Color.Transparent;
            Dialogs.ProgressDialog.Hide();
        }

        private void UnsubscribeEvents()
        {
            if (_translator != null)
            {
                _translator.PartialResultReady -= OnPartialResultReady;
                _translator.TranscriptionResultReady -= OnTranscriptionResultReady;
                _translator.TranslationSpeechReady -= OnTranslationSpeechReady;
                _translator.TranslationCancelled -= OnTranslationCancelled;
            }
            if (_microsoftTextToSpeechProvider != null)
            {
                _microsoftTextToSpeechProvider.TranscriptionResultReady -= MicrosoftTextToSpeechProvider_OnTranscriptionResultReady;
                _microsoftTextToSpeechProvider.TranslationSpeechReady -= MicrosoftTextToSpeechProvider_OnTranslationSpeechReady;
            }
        }

        /// <summary>
        /// Method to create a Session
        /// </summary>
        private async Task CreateSession()
        {
            try
            {
                if (ChatList != null && ChatList.Any())
                {
                    var settings = await Settings.CurrentUser();

                    var rawStart = DateTimeOffset.FromUnixTimeSeconds(StartTime).ToLocalTime();
                    var EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var rawEnd = DateTimeOffset.FromUnixTimeSeconds(EndTime).ToLocalTime();
                    var sessionDuration = TimeSpan.FromSeconds((rawStart - rawEnd).TotalSeconds).ToString(@"m\:ss");

                    if (string.IsNullOrEmpty(SessionName))
                        SessionName = "--";

                    if (ChatList.Any())
                    {
                        Session session = new Session()
                        {
                            StartTime = StartTime,
                            RecordDate = rawStart.ToString("d"),
                            RawStartTime = rawStart.ToString("t"),
                            EndTime = EndTime,
                            RawEndTime = rawEnd.ToString("t"),
                            CustomerId = settings.UserIntID,
                            UserId = settings.UserIntID,
                            SoftwareVersion = VersionTracking.CurrentVersion,
                            SourceLangISO = PrimaryLanguage.Code,
                            TargetLangIso = SecondaryLanguage.Code,
                            SourceLanguage = PrimaryLanguage.Name,
                            TargeLanguage = SecondaryLanguage.Name,
                            ClientType = Settings.GetClientType(),
                            IngramMicroCustomerId = Constants.DefaultIngramMicroCustomerId,
                            LicenseKeyUsed = Constants.DefaultLicenseKey,
                            BillableSeconds = SessionBillableSeconds,
                            SessionNumber = SessionNumber,
                            SessionName = SessionName,
                            OrganizationId = settings.OrganizationId,
                            Duration = TimeCounter != "00:00" ? TimeCounter : sessionDuration
                        };

                        if (CustomTags != null && CustomTags.Any())
                        {
                            session.CustomTags = string.Join(",", CustomTags);
                        }

                        var newSession = await _dataservice.AddItemAsync<Session>(session).ConfigureAwait(true);

                        await CreateTranscription(newSession.ID);
                        await CreateSessionTags(newSession.ID, newSession.OrganizationId);

                        MessagingCenter.Instance.Send(newSession, "AddSession");

                        if (CanSyncData)
                        {
                            Thread pushThread = new Thread(_pushDataService.BeginDataSync);
                            pushThread.Start();
                            Thread pullThread = new Thread(_pullDataService.BeginDataSync);
                            pullThread.Start();
                        }

                        Analytics.TrackEvent("Translation Session Event",
                            new Dictionary<string, string> {
                    { "SourceLanguage", PrimaryLanguage.Name },
                    { "TargetLanguage", SecondaryLanguage.Name },
                    { "Duration (Seconds)", (rawStart - rawEnd).TotalSeconds.ToString() } });
                    }
                    else
                    {
                        FileAccessUtility.DeleteFile(_audioFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        /// <summary>
        /// Method to create a transcription
        /// </summary>
        private async Task CreateTranscription(int sessionId)
        {
            foreach (var chat in ChatList)
            {
                var transcription = new Transcription
                {
                    SessionId = sessionId,
                    ChatUser = chat.Person,
                    OriginalText = chat.OriginalText,
                    TranslatedText = chat.TranslatedText,
                    ChatTime = chat.Date,
                    SyncedToServer = false,
                    Sentiment = chat.Sentiment,
                    TranslationSeconds = chat.Duration.TotalSeconds
                };

                await _dataservice.AddItemAsync<Transcription>(transcription);
            }
        }

        public async Task CreateSessionTags(int sessionId, int organizationId)
        {
            if (SessionTags != null && SessionTags.Any())
            {
                var sessionTags = new List<DataService.Models.SessionTag>();

                foreach (var sessionTag in SessionTags)
                {
                    sessionTags.Add(new DataService.Models.SessionTag
                    {
                        OrganizationId = organizationId,
                        SessionId = sessionId,
                        TagValue = sessionTag.TagValue,
                        OrganizationTagId = sessionTag.OrganizationTagId
                    });
                }

                await _dataservice.CreateSessionTags(sessionTags, sessionId);
            }
        }

        Queue<long> offsetQueue = new Queue<long>();
        long offset;

        private async void PlayAudio(byte[] bytes)
        {
            try
            {
                Stream stream = new MemoryStream(bytes);
                AudioQueue.Enqueue(stream);

                while (!AudioQueue.IsEmpty)
                {
                    HaloColor = Color.FromHex("#3284C2");

                    AudioQueue.TryPeek(out var queued);

                    if (!IsPlaying)
                    {
                        AudioQueue.TryDequeue(out _);
                    }

                    _translator.Mute();
                    CrossMediaManager.Current.MediaItemFinished += Current_MediaItemFinished;
                    IsPlaying = true;
                    await CrossMediaManager.Current.Play(queued, $"{Guid.NewGuid()}.wav");
                    await Task.Delay(2000); // Delay to avoid sounding like one sentence
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        private async void Current_MediaItemFinished(object sender, MediaManager.Media.MediaItemEventArgs e)
        {
            HaloColor = Color.Transparent;
            CrossMediaManager.Current.MediaItemFinished -= Current_MediaItemFinished;
            IsPlaying = false;

            await Task.Delay(2000); // Delay to prevent microphone from listening to speaker
            _translator.UnMute();
        }

        /// <summary>
        /// Empty AudioQueue
        /// </summary>
        private void ClearAudioResponseQueue()
        {
            if (AudioQueue != null && AudioQueue.Any())
            {
                while (AudioQueue.TryDequeue(out _)) { }
            }
        }

        /// <summary>
        /// Method to add chat items 
        /// </summary>
        /// <param name="translationResult"></param>
        private async void HandleChatMessage(TranslationResultText translationResult)
        {
            // TODO
            // An error occurs in this method:
            // **System.InvalidOperationException:** 'Cannot change ObservableCollection during a CollectionChanged event.'
            try
            {
                SessionBillableSeconds += translationResult.Duration.TotalSeconds;
                var date = DateTime.UtcNow.ToLocalTime();

                var chat = new TranslationResultText
                {
                    SourceLanguageCode = translationResult.SourceLanguageCode,
                    TargetLanguageCode = translationResult.TargetLanguageCode,
                    OriginalText = translationResult.OriginalText,
                    TranslatedText = translationResult.TranslatedText,
                    DateString = date.ToString("HH:mm:ss"),
                    Date = date,
                    Sentiment = "Neutral",
                    SentimentEmoji = _sentimentAnalysis.SentimentEmoji("Neutral"),
                    Duration = translationResult.Duration,
                    OffsetInTicks = translationResult.OffsetInTicks,
                    IsComplete = translationResult.IsComplete,
                    IsPerson1 = translationResult.IsPerson1
                };

                if (CannotBeAutoDetected)
                {
                    if (!_translator.Reversed)
                    {
                        chat.IsPerson1 = true;
                        chat.Person = "Person 1";
                    }
                    else
                    {
                        chat.IsPerson1 = false;
                        chat.Person = "Person 2";
                    }
                }
                else
                {
                    if (translationResult.IsPerson1)
                    {
                        chat.IsPerson1 = true;
                        chat.Person = "Person 1";
                        GetSecondaryLanguage(chat.TargetLanguageCode);
                    }
                    else
                    {
                        chat.IsPerson1 = false;
                        chat.Person = "Person 2";
                    }
                }

                var chats = ChatList.ToList();
                if (chat.IsComplete && chats.Any(c => c.IsComplete == false))
                {
                    chats.RemoveAll(c => c.IsComplete = false);
                }

                ChatList = new ObservableRangeCollection<TranslationResultText>(chats);

                if (ChatList.Any(s => s.OffsetInTicks == translationResult.OffsetInTicks))
                {
                    var exisitingChatItem = ChatList.FirstOrDefault(s => s.OffsetInTicks == translationResult.OffsetInTicks);

                    if (exisitingChatItem != null)
                    {
                        exisitingChatItem.OriginalText = translationResult.OriginalText;
                        exisitingChatItem.TranslatedText = translationResult.TranslatedText;
                        exisitingChatItem.IsPerson1 = translationResult.IsPerson1;
                        exisitingChatItem.IsComplete = translationResult.IsComplete;
                    }
                }
                else if (chat != null && !string.IsNullOrEmpty(chat.TranslatedText))
                {
                    ChatList.Add(chat);

                    MessagingCenter.Instance.Send(ChatList.LastOrDefault(), "ScrollToMessage");

                    await GetSentiment(chat.OriginalText, chat.Guid).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        private void GetSecondaryLanguage(string languageCode)
        {
            if (_autodetectSecondaryLanguages.Count > 1)
            {
                if (!IsSecondaryLanguageVisible)
                {
                    TryGetSecondaryLanguage(languageCode);
                }
                else if (SecondaryLanguage != null && SecondaryLanguage.Code != languageCode)
                {
                    TryGetSecondaryLanguage(languageCode);
                }
            }
        }

        private void TryGetSecondaryLanguage(string languageCode)
        {
            var language = _languages.FirstOrDefault(s => s.Code == languageCode || s.Code.Contains(languageCode));

            if (language != null)
            {
                SecondaryLanguage = language;
                DisplaySecondaryLanguage = language;
                IsSecondaryLanguageVisible = true;
            }
        }

        public async Task GetSentiment(string inputText, Guid guid)
        {
            var sentiment = await _sentimentAnalysis.GetSentiment(inputText);

            var chat = ChatList.FirstOrDefault(s => s.Guid == guid);

            if (chat != null)
            {
                chat.SentimentEmoji = _sentimentAnalysis.SentimentEmoji(sentiment);
                chat.Sentiment = sentiment;
            }
        }

        /// <summary>
        /// Method to open Languages Popup
        /// </summary>
        /// <param name="isSelectingPrimaryLanguage">True if selecting primary language</param>
        private async Task OpenLanguagesPopup(bool isSelectingPrimaryLanguage)
        {
            IsSelectingPrimaryLanguage = isSelectingPrimaryLanguage;

            await PopupNavigation.Instance.PushAsync(new LanguagesPopup { BindingContext = this });
        }

        /// <summary>
        /// Method to close Popup
        /// </summary>
        private async Task ClosePopup()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        /// <summary>
        /// Method to Select a Language
        /// </summary>
        private async Task SelectLanguage(Language language)
        {
            await ClosePopup();

            if (IsSelectingPrimaryLanguage)
            {
                PrimaryLanguage = language;
            }
            else
            {
                SecondaryLanguage = language;
            }
        }

        private async void AudioNotDetectedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _translator.Mute();
            _audioNotDetectedTimer.Stop();
            await PopupNavigation.Instance.PushAsync(new AudioNotDetected { BindingContext = this });
            _countDownTimer.Start();
            _timeCounterSeconds = 60;
        }

        private async Task ContinueTranslating()
        {
            _translator.UnMute();
            await ClosePopup();
            _audioNotDetectedTimer.Start();
            _countDownTimer.Stop();
        }

        private async void CountDownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timeCounterSeconds--;

            TimeCounter = _timeCounterSeconds / 60 + ":" + ((_timeCounterSeconds % 60) >= 10 ? (_timeCounterSeconds % 60).ToString() : "0" + _timeCounterSeconds % 60);

            if (_timeCounterSeconds == 0)
            {
                _countDownTimer.Stop();
                await ClosePopup();
                await StopTranslating();
            }
        }

        private void TranslationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _translationTimeCounterSeconds++;
            TimeCounter = TimeSpan.FromSeconds(_translationTimeCounterSeconds).ToString("mm\\:ss");
            var minutes = _translationTimeCounterSeconds / 60;

            if ((_maxUserSessionTime != null || _maxUserSessionTime != 0))
            {
                if (minutes >= _maxUserSessionTime)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        _translationTimeCounterSeconds = 0;
                        //_translationTimer.Elapsed -= TranslationTimer_Elapsed;
                        Dialogs.HandleDialogMessage(Dialogs.DialogMessage.Defined, "You have reached your maximum time limit per session. Ending session.", seconds: 5000);
                        await Task.Delay(5000);
                        IsTranslating = true;
                        await StartStopTranslation();
                    });
                }
            }
        }

        private async Task ImmersiveRead()
        {
            List<ImmersiveReaderRequest> request = new List<ImmersiveReaderRequest>();

            if (ChatList.Count != 0)
            {
                foreach (var item in ChatList)
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

        private async Task ImmersiveRead(List<ImmersiveReaderRequest> request)
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

                    await Application.Current.MainPage.Navigation.PushAsync(new ImmersiveReader());
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

        private async void SearchLanguage(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                Languages = await _languagesService.GetSupportedLanguages();
                return;
            }

            List<Language> filteredLanguages = new List<Language>();

            for (int i = 0; i < Languages.Count; i++)
            {
                var language = Languages[i];
                if (
                    language.Code.ToLower().Contains(searchText.ToLower())
                    || language.DisplayName.ToLower().Contains(searchText.ToLower())
                    || language.Name.ToLower().Contains(searchText.ToLower())
                    || language.CountryName.ToLower().Contains(searchText.ToLower())
                    || language.EnglishName.ToLower().Contains(searchText.ToLower())
                    || language.CountryNativeName.ToLower().Contains(searchText.ToLower())
                    )
                {
                    filteredLanguages.Add(language);
                }
            }

            Languages = new List<Language>(filteredLanguages);
        }

        private void UpdateMicStatus()
        {
            if (IsMicrophoneMute)
            {
                //Mute actual microphone
                _microphoneService.MuteMicrophone();
                MuteBtnIcon = FontAwesomeIcons.MicrophoneSlash;
                MuteBtnText = "Mic Off";
            }
            else
            {
                //Unmute actual microphone
                _microphoneService.UnMuteMicrophone();
                MuteBtnIcon = FontAwesomeIcons.Microphone;
                MuteBtnText = "Mic On";
            }
        }

        private void MuteUnmuteMicrophone()
        {
            if (IsMicrophoneMute)
            {
                IsMicrophoneMute = false;
            }
            else
            {
                IsMicrophoneMute = true;
            }
        }

        private async Task GetOrganizationQuestions()
        {
            var existingQuestions = await _dataservice.GetOrgQuestionsAsync();

            if (existingQuestions != null && existingQuestions.Any())
            {
                if (OrganizationQuestions == null || !OrganizationQuestions.Any())
                {
                    OrganizationQuestions = new ObservableRangeCollection<UserQuestions>();

                    for (int i = 0; i < existingQuestions.Count; i++)
                    {
                        existingQuestions[i].Index = i + 1;
                        OrganizationQuestions.Add(existingQuestions[i]);
                    }
                }
            }
        }

        private async Task LaunchOrganizationSettings()
        {
            await PopupNavigation.Instance.PushAsync(new OrganizationQuestions { BindingContext = this });
        }

        private async Task SelectOrganizationQuestion(UserQuestions userQuestion)
        {
            await ClosePopup();

            if (SecondaryLanguage != null)
            {
                var language = Languages.FirstOrDefault(s => s.Code == userQuestion.LanguageCode);

                if (language != null)
                {
                    _appAnalytics.CaptureCustomEvent("Organization questions launched",
                   new Dictionary<string, string> {
                        {"User", App.CurrentUser?.Email },
                        {"Organisation", App.CurrentUser?.Organization },
                        {"Action", "Predefined org question selected" }
                   });
                    var primaryLanguage = new CoreLanguage
                    {
                        Code = language.Code,
                        DisplayName = language.DisplayName,
                        Name = language.Name,
                        UseNeuralVoice = language.UseNeuralVoice,
                        VoiceName = language.VoiceName,
                        Voice = new Voice { Code = language.Voice, IsNeuralVoice = true }
                    };

                    var secondaryLanguage = new CoreLanguage
                    {
                        Code = SecondaryLanguage.Code,
                        DisplayName = SecondaryLanguage.DisplayName,
                        Name = SecondaryLanguage.Name,
                        UseNeuralVoice = SecondaryLanguage.UseNeuralVoice,
                        VoiceName = SecondaryLanguage.VoiceName,
                        Voice = new Voice { Code = SecondaryLanguage.Voice, IsNeuralVoice = true }
                    };

                    await _microsoftTextToSpeechProvider.Translate(Constants.CognitiveServicesApiKey, Constants.CognitiveServicesRegion, primaryLanguage, userQuestion.Question, secondaryLanguage);
                }
            }
        }

        private async void MicrosoftTextToSpeechProvider_OnTranscriptionResultReady(TranslationResult translationResult)
        {
            try
            {
                SessionBillableSeconds += translationResult.Duration.TotalSeconds;
                var date = DateTime.UtcNow.ToLocalTime();

                var chat = new TranslationResultText
                {
                    SourceLanguageCode = translationResult.SourceLanguageCode,
                    TargetLanguageCode = translationResult.TargetLanguageCode,
                    OriginalText = translationResult.OriginalText,
                    TranslatedText = translationResult.TranslatedText,
                    DateString = date.ToString("HH:mm:ss"),
                    Date = date,
                    Sentiment = "Neutral",
                    SentimentEmoji = _sentimentAnalysis.SentimentEmoji("Neutral"),
                    Duration = translationResult.Duration,
                    OffsetInTicks = translationResult.OffsetInTicks,
                    IsComplete = true,
                    IsPerson1 = translationResult.IsPerson1
                };

                chat.IsPerson1 = true;
                chat.Person = "Person 1";

                if (chat != null && !string.IsNullOrEmpty(chat.TranslatedText))
                {
                    ChatList.Add(chat);

                    MessagingCenter.Instance.Send(ChatList.LastOrDefault(), "ScrollToMessage");

                    await GetSentiment(chat.OriginalText, chat.Guid).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        private async void MicrosoftTextToSpeechProvider_OnTranslationSpeechReady(TranslationResult obj)
        {
            try
            {
                if (obj != null && obj.AudioResult != null)
                {
                    _translator.Mute();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Code to run on the main thread
                        PlayAudio(obj.AudioResult);
                    });

                    _translator.WriteToFile(obj.AudioResult);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _appCrashlytics.Attachments());
            }
        }

        /// <summary>
        /// Command to start/end translation
        /// </summary>
        ICommand _firstTranslateCommand = null;

        public ICommand FirstTranslateCommand
        {
            get
            {
                return _firstTranslateCommand ?? (_firstTranslateCommand =
                                          new Command(async () => await AnimateAndStartTranslation()));
            }
        }

        /// <summary>
        /// Command to start/end translation
        /// </summary>
        ICommand _translateCommand = null;

        public ICommand TranslateCommand
        {
            get
            {
                return _translateCommand ?? (_translateCommand =
                                          new Command(async () => await StartStopTranslation()));
            }
        }

        /// <summary>
        /// Command to start/end translation
        /// </summary>
        ICommand _switchSpeakerCommand = null;

        public ICommand SwitchSpeakerCommand
        {
            get
            {
                return _switchSpeakerCommand ?? (_switchSpeakerCommand =
                                          new Command(async () => await SwitchSpeakers()));
            }
        }

        /// <summary>
        /// Command to open Languages Popup
        /// </summary>
        ICommand _openLanguagesPopupCommand = null;

        public ICommand OpenLanguagesPopupCommand
        {
            get
            {
                return _openLanguagesPopupCommand ?? (_openLanguagesPopupCommand =
                                          new Command(async (object obj) => await OpenLanguagesPopup(Convert.ToBoolean(obj))));
            }
        }

        /// <summary>
        /// Command to close Languages Popup
        /// </summary>
        ICommand _closeLanguagesPopupCommand = null;

        public ICommand CloseLanguagesPopupCommand
        {
            get
            {
                return _closeLanguagesPopupCommand ?? (_closeLanguagesPopupCommand =
                                          new Command(async () => await ClosePopup()));
            }
        }

        /// <summary>
        /// Command to close Questions Popup
        /// </summary>
        ICommand _closeOrgQuestionsPopupCommand = null;

        public ICommand CloseOrgQuestionsPopupCommand
        {
            get
            {
                return _closeOrgQuestionsPopupCommand ?? (_closeOrgQuestionsPopupCommand =
                                          new Command(async () => await ClosePopup()));
            }
        }

        /// <summary>
        /// Command to continue translating after audio not detected
        /// </summary>
        ICommand _continueTranslatingCommand = null;

        public ICommand ContinueTranslatingCommand
        {
            get
            {
                return _continueTranslatingCommand ?? (_continueTranslatingCommand =
                                          new Command(async () => await ContinueTranslating()));
            }
        }

        /// <summary>
        /// Command to Select a Language
        /// </summary>
        ICommand _selectLanguageCommand = null;

        public ICommand SelectLanguageCommand
        {
            get
            {
                return _selectLanguageCommand ?? (_selectLanguageCommand =
                                          new Command(async (object obj) => await SelectLanguage((Language)obj)));
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


        ICommand _editSessionMetadataCommand = null;

        public ICommand EditSessionMetadataCommand
        {
            get
            {
                return _editSessionMetadataCommand ?? (_editSessionMetadataCommand =
                                          new Command(async (object obj) => await EditSessionMetadata()));
            }
        }

        /// <summary>
        /// Command to mute/unmute microphone
        /// </summary>
        ICommand _muteUnmuteCommand = null;

        public ICommand MuteUnmuteCommand
        {
            get
            {
                return _muteUnmuteCommand ?? (_muteUnmuteCommand =
                                          new Command(() => MuteUnmuteMicrophone()));
            }
        }

        #endregion
    }
}
