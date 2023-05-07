using MvvmHelpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Threading.Tasks;

using Translation.AppSettings;
using Translation.DataService.Interfaces;
using Translation.Helpers;
using Translation.Messages;
using Translation.Models;
using Translation.Services.Languages;
using Translation.Utils;

using Xamarin.Forms;


namespace Translation.ViewModels
{
    public class AutoDetectionFlagsViewModel : BaseViewModel
    {
        private ObservableCollection<Country> _countries;
        public ObservableCollection<Country> Countries
        {
            get { return _countries; }
            set
            {
                _countries = value;
                OnPropertyChanged();
            }
        }

        private Country _selectedCountry;
        public Country SelectedCountry
        {
            get { return _selectedCountry; }
            set
            {
                _selectedCountry = value;
                OnPropertyChanged();
                if (SelectedCountry != null)
                {
                    SelectCountry(SelectedCountry);
                }
            }
        }

        private List<Country> _originalCountries { get; set; }

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

        public AutoDetectionFlagsViewModel(IDataService dataService, ILanguagesService languagesService)
        {
            _dataService = dataService;
            _languagesService = languagesService;
            MessagingCenter.Subscribe<string>(this, "CountrySearchText", (sender) =>
            {
                SearchCountry(sender);
            });
            MessagingCenter.Subscribe<string>(this, "UpdateLanguageLists", (sender) =>
            {
                InitializeCountries();
            });
            InitializeCountries();
        }

        private async void InitializeCountries()
        {
            _originalCountries = await _languagesService.GetCountries();
            Countries = new ObservableCollection<Country>(_originalCountries);
        }

        private void SelectCountry(Country country)
        {
            if (country != null)
            {
                Countries.Select(c => { c.IsSelected = false; return c; }).ToList();
                Countries.FirstOrDefault(s => s.CountryCode == country.CountryCode).IsSelected = true;
                bool languagesCanAutoDetect = LanguagesCanAutoDetect(country.Languages);
                MessagingCenter.Instance.Send(new AutoDetectionMessage { LanguagesCanAutoDetect = languagesCanAutoDetect }, "LanguagesCanAutoDetect");

                if (languagesCanAutoDetect)
                {
                    MessagingCenter.Instance.Send(country.Languages.ToList(), "UpdateAutoDetectTranslationLanguages");
                    MessagingCenter.Instance.Send(new QuickStartSetupMessage { ShowQuickStartSetup = false }, "ShowQuickStartSetup");
                }
                else
                {
                    MessagingCenter.Instance.Send(new AutoDetectionCountryMessage { Country = country }, "SelectedCountry");
                    MessagingCenter.Instance.Send(new QuickStartSetupMessage { ShowQuickStartSetup = true }, "ShowQuickStartSetup");
                }
            }
        }

        private bool LanguagesCanAutoDetect(IEnumerable<Language> languages)
        {
            foreach(var language in languages)
            {
                if (!_languagesService.GetAutoDetectSupportedLanguages().Any(s => s.Code == language.Code))
                    return false;
            }

            return true;
        }

        private void SearchCountry(string searchText)
        {
            try
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    Countries = new ObservableCollection<Country>(_originalCountries);
                    return;
                }

                List<Country> filteredLanguages = new List<Country>();

                for (int i = 0; i < Countries.Count; i++)
                {
                    var country = Countries[i];
                    if (
                        country.CountryName.ToLower().Contains(searchText.ToLower()) ||
                        country.CountryNativeName.ToLower().Contains(searchText.ToLower()) ||
                        country.CountryCode.ToLower().Contains(searchText.ToLower())
                        )
                    {
                        filteredLanguages.Add(country);
                    }
                }

                Countries = new ObservableCollection<Country>(filteredLanguages);

                ListIsEmpty = !Countries.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
