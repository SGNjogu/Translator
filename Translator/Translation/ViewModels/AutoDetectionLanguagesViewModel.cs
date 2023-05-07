using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using MediaManager;

using MvvmHelpers;

using Translation.Core.Interfaces;
using Translation.Helpers;
using Translation.Messages;
using Translation.Models;
using Translation.Services.Languages;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class AutoDetectionLanguagesViewModel : BaseViewModel
    {
        private Country _selectedCountry;
        public Country SelectedCountry
        {
            get { return _selectedCountry; }
            set
            {
                _selectedCountry = value;
                OnPropertyChanged();
            }
        }

        private Language _selectedLanguage;
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged();
                _ = SelectLanguage(SelectedLanguage);
            }
        }

        private string _selectedLanguageText;
        public string SelectedLanguageText
        {
            get { return _selectedLanguageText; }
            set
            {
                _selectedLanguageText = value;
                OnPropertyChanged();
            }
        }

        private bool _isActivityIndicatorRunning = false;
        public bool IsActivityIndicatorRunning
        {
            get { return _isActivityIndicatorRunning; }
            set
            {
                _isActivityIndicatorRunning = value;
                OnPropertyChanged();
            }
        }

        private readonly IMicrosoftTextToTextTranslator _microsoftTextToTextTranslator;
        private readonly IMicrosoftStandardVoiceSynthesizer _microsoftStandardVoiceSynthesizer;
        private readonly ILanguagesService _languagesService;

        public AutoDetectionLanguagesViewModel(
            IMicrosoftTextToTextTranslator microsoftTextToTextTranslator,
            IMicrosoftStandardVoiceSynthesizer microsoftStandardVoiceSynthesizer,
            ILanguagesService languagesService
            )
        {
            _microsoftTextToTextTranslator = microsoftTextToTextTranslator;
            _microsoftStandardVoiceSynthesizer = microsoftStandardVoiceSynthesizer;
            _languagesService = languagesService;

            if(_microsoftStandardVoiceSynthesizer != null)
                _microsoftStandardVoiceSynthesizer.TranslationSpeechReady += OnSpeechReady;
            MessagingCenter.Subscribe<AutoDetectionCountryMessage>(this, "SelectedCountry", (sender) =>
            {
                SelectedCountry = sender.Country;
            });
            MessagingCenter.Subscribe<Language>(this, "AutoDetectionSlectedLanguage", (sender) =>
            {
                SelectedLanguage = sender;
            });
            MessagingCenter.Subscribe<string>(this, "AutoDetectionLanguagesSelectFirstLanguage", (sender) =>
            {
                SelectFirstLanguage();
            });
        }

        private void SelectFirstLanguage()
        {
            if(SelectedCountry != null)
                SelectedLanguage = SelectedCountry.Languages[0];
        }

        private async void OnSpeechReady(Core.Domain.TranslationResult obj)
        {
            try
            {
                if (obj != null && obj.AudioResult != null)
                {
                    await PlayAudio(obj.AudioResult);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task PlayAudio(byte[] bytes)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    IsActivityIndicatorRunning = false;
                    Stream stream = new MemoryStream(bytes);
                    await CrossMediaManager.Current.Play(stream, $"{Guid.NewGuid()}.wav");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }

        private async Task SelectLanguage(Language language)
        {
            if (language != null)
            {
                await MainThread.InvokeOnMainThreadAsync(async ()=>
                {
                    try
                    {
                        SelectedCountry.Languages.Select(c => { c.IsSelected = false; return c; }).ToList();
                        SelectedCountry.Languages.FirstOrDefault(s => s.Code == language.Code).IsSelected = true;

                        string selectedLanguageText = $"Is {language.EnglishName} your Language?";
                        IsActivityIndicatorRunning = true;

                        string translatedText = await _microsoftTextToTextTranslator.TranslateTextToText
                            (
                            Constants.CognitiveServicesApiKey,
                            Constants.CognitiveServicesRegion,
                            "en",
                            selectedLanguageText,
                            language.Code.Substring(0, 2));

                        await _microsoftStandardVoiceSynthesizer.SynthesizeText
                            (
                            language.Code,
                            translatedText,
                            Constants.CognitiveServicesApiKey,
                            Constants.CognitiveServicesRegion
                            );

                        IsActivityIndicatorRunning = false;

                        SelectedLanguageText = translatedText;

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }
        }

        private void SelectLanguageAndNavigate()
        {
            if(SelectedLanguage != null)
            {
                if (LanguageCanAutoDetect(SelectedLanguage))
                {
                    MessagingCenter.Instance.Send(new AutoDetectionMessage { LanguagesCanAutoDetect = true }, "LanguagesCanAutoDetect");
                    MessagingCenter.Instance.Send(new List<Language> { SelectedLanguage }, "UpdateAutoDetectTranslationLanguages");
                    MessagingCenter.Instance.Send(new QuickStartSetupMessage { ShowQuickStartSetup = false }, "ShowQuickStartSetup");
                }
                else
                {
                    MessagingCenter.Instance.Send(new AutoDetectionMessage { LanguagesCanAutoDetect = false }, "LanguagesCanAutoDetect");
                    MessagingCenter.Instance.Send(SelectedLanguage, "UpdateLanguageTwoSetup");
                    MessagingCenter.Instance.Send(new QuickStartSetupMessage { ShowQuickStartSetup = true }, "ShowQuickStartSetup");
                }
                MessagingCenter.Instance.Send("", "MoveToNextItem");
            }
        }

        private bool LanguageCanAutoDetect(Language language)
        {
            if (!_languagesService.GetAutoDetectSupportedLanguages().Any(s => s.Code == language.Code))
                return false;

            return true;
        }

        private void DeselectLanguage()
        {
            var languageIndex = SelectedCountry.Languages.IndexOf(SelectedLanguage);
            // move to next item
            if (languageIndex < SelectedCountry.Languages.Count - 1)
            {
                languageIndex++;
            }
            else
            {
                languageIndex = 0;
            }

            SelectedLanguage = SelectedCountry.Languages.ElementAt(languageIndex);
        }

        private async Task PlayAudio()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    IsActivityIndicatorRunning = true;

                    await _microsoftStandardVoiceSynthesizer.SynthesizeText
                        (
                        SelectedLanguage.Code,
                        SelectedLanguageText,
                        Constants.CognitiveServicesApiKey,
                        Constants.CognitiveServicesRegion
                        );
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                IsActivityIndicatorRunning = false;
            });
        }

        ICommand _selectLanguageCommand = null;

        public ICommand SelectLanguageCommand
        {
            get
            {
                return _selectLanguageCommand ?? (_selectLanguageCommand =
                                          new Command(() => SelectLanguageAndNavigate()));
            }
        }

        ICommand _deselectLanguageCommand = null;

        public ICommand DeselectLanguageCommand
        {
            get
            {
                return _deselectLanguageCommand ?? (_deselectLanguageCommand =
                                          new Command(() => DeselectLanguage()));
            }
        }

        ICommand _playAudioCommand = null;

        public ICommand PlayAudioCommand
        {
            get
            {
                return _playAudioCommand ?? (_playAudioCommand =
                                          new Command(async () => await PlayAudio()));
            }
        }
    }
}
