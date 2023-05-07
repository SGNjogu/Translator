using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Translation.AppSettings;
using Translation.Helpers;
using Translation.Messages;
using Translation.Models;
using Translation.Services.Languages;
using Translation.Utils;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class QuickStartLanguageTwoViewModel : BaseViewModel
    {
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

        private Language _selectedLanguage;
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged();
                if (SelectedLanguage != null)
                {
                    SelectLanguage(SelectedLanguage);
                }
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

        private List<Language> _originalLanguages { get; set; }

        private bool _listIsEmpty;
        public bool ListIsEmpty
        {
            get { return _listIsEmpty; }
            set
            {
                _listIsEmpty = value;
                OnPropertyChanged();
            }
        }

        private readonly ILanguagesService _languagesService;

        public QuickStartLanguageTwoViewModel(ILanguagesService languagesService)
        {
            _languagesService = languagesService;
            MessagingCenter.Subscribe<string>(this, "QuickStartBringLanguageTwoIntoView", (sender) =>
            {
                BringIntoView();
            });
            MessagingCenter.Subscribe<string>(this, "LanguageTwoSearchText", (sender) =>
            {
                SearchLanguage(sender);
            });
            MessagingCenter.Subscribe<string>(this, "UpdateLanguageLists", async (sender) =>
            {
                await ReloadLanguages();
            });

            Languages = new List<Language>();
            _originalLanguages = new List<Language>();
            InitializeLanguages();
        }

        private async Task ReloadLanguages()
        {
            await Task.Delay(1000);
            InitializeLanguages();
        }

        private async void InitializeLanguages()
        {
            _originalLanguages = await _languagesService.GetSupportedLanguages();
            if (_originalLanguages == null || !_originalLanguages.Any())
                return;

            Languages = new List<Language>(_originalLanguages);

            var defaultLanguages = await _languagesService.GetDefaultLanguages();
            var defaultLanguageOverridden = Settings.IsDefaultLanguageOverridden();
            var defaultTargetLanguage = defaultLanguages[EnumsConverter.ConvertToString(Settings.Setting.DefaultTargetLanguage)];
            LanguageTwo = Languages.Where(s => s.Code.Equals(defaultTargetLanguage)).FirstOrDefault();
            SelectedLanguage = LanguageTwo;
        }

        private void SelectLanguage(Language language)
        {
            if (language != null)
            {
                Languages.Select(c => { c.IsSelected = false; return c; }).ToList();
                Languages.FirstOrDefault(s => s.Code == language.Code).IsSelected = true;

                if (LanguageCanAutoDetect(SelectedLanguage))
                {
                    MessagingCenter.Instance.Send(new List<Language> { SelectedLanguage }, "UpdateAutoDetectTranslationLanguages");
                }

                MessagingCenter.Instance.Send(SelectedLanguage, "UpdateLanguageTwoSetup");
            }
        }

        private bool LanguageCanAutoDetect(Language language)
        {
            if (!_languagesService.GetAutoDetectSupportedLanguages().Any(s => s.Code == language.Code))
                return false;

            return true;
        }

        private void SearchLanguage(string searchText)
        {
            try
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    Languages = new List<Language>(_originalLanguages);
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

                ListIsEmpty = !Languages.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void BringIntoView()
        {
            if (SelectedLanguage != null)
            {
                Languages.Select(c => { c.IsSelected = false; return c; }).ToList();
                Languages.FirstOrDefault(s => s.Code == SelectedLanguage.Code).IsSelected = true;
                MessagingCenter.Instance.Send((object)Languages.IndexOf(Languages.FirstOrDefault(s => s.Code == SelectedLanguage.Code)), "BringLanguageTwoIntoView");
                MessagingCenter.Instance.Send(SelectedLanguage, "UpdateLanguageTwoSetup");
            }
        }
    }
}
