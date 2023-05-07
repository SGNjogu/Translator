using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Translation.AppSettings;
using Translation.DataService.Interfaces;
using Translation.Helpers;
using Translation.Models;
using Translation.Services.Languages;
using Translation.Utils;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class QuickStartLanguageOneViewModel : BaseViewModel
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

        private readonly IDataService _dataService;
        private readonly ILanguagesService _languagesService;

        public QuickStartLanguageOneViewModel(IDataService dataService, ILanguagesService languagesService)
        {
            _dataService = dataService;
            _languagesService = languagesService;
            MessagingCenter.Subscribe<string>(this, "QuickStartBringLanguageOneIntoView", (sender) =>
            {
                BringIntoView();
            });
            MessagingCenter.Subscribe<string>(this, "LanguageOneSearchText", (sender) =>
            {
                SearchLanguage(sender);
            });
            MessagingCenter.Subscribe<string>(this, "UpdateLanguageLists", (sender) =>
            {
                _ = ReloadLanguages();
            });

            Languages = new List<Language>();
            _originalLanguages = new List<Language>();

            _ = InitializeLanguages();
        }

        private async Task ReloadLanguages()
        {
            await Task.Delay(1000);
            await InitializeLanguages();
        }

        private async Task InitializeLanguages()
        {
            try
            {
                _originalLanguages = await _languagesService.GetSupportedLanguages();
                if (_originalLanguages == null || !_originalLanguages.Any())
                    return;

                Languages = new List<Language>(_originalLanguages);

                var defaultLanguages = await _languagesService.GetDefaultLanguages().ConfigureAwait(true);
                var defaultSourceLanguage = defaultLanguages[EnumsConverter.ConvertToString(Settings.Setting.DefaultSourceLanguage)];
                var defaultLanguageOverridden = Settings.IsDefaultLanguageOverridden();

                if (defaultLanguageOverridden)
                {
                    LanguageOne = Languages.Where(s => s.Code.Equals(defaultSourceLanguage)).FirstOrDefault();
                }
                else
                {
                    var organizationSettings = await _dataService.GetOrganizationSettingsAsync();
                    if (organizationSettings.Count != 0 && !string.IsNullOrEmpty(organizationSettings[0].LanguageCode) && organizationSettings[0].LanguageCode != "string")
                    {
                        LanguageOne = Languages.Where(c => c.Code == organizationSettings[0].LanguageCode).FirstOrDefault();
                    }
                    else
                    {
                        LanguageOne = Languages.Where(s => s.Code.Equals(defaultSourceLanguage)).FirstOrDefault();
                    }
                }
                SelectedLanguage = LanguageOne;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }

        private void SelectLanguage(Language language)
        {
            if (language != null)
            {
                Languages.Select(c => { c.IsSelected = false; return c; }).ToList();
                Languages.FirstOrDefault(s => s.Code == language.Code).IsSelected = true;
                MessagingCenter.Instance.Send(language, "UpdateLanguageOne");
            }
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
                MessagingCenter.Instance.Send((object)Languages.IndexOf(Languages.FirstOrDefault(s => s.Code == SelectedLanguage.Code)), "BringLanguageOneIntoView");
                MessagingCenter.Instance.Send(SelectedLanguage, "UpdateLanguageOne");
            }
        }
    }
}
