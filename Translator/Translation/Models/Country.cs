using System;
using System.Collections.ObjectModel;

using MvvmHelpers;

using Newtonsoft.Json;

using Xamarin.Forms;

namespace Translation.Models
{
    public class Country : BaseViewModel
    {
        [JsonIgnore]
        private FileImageSource _flag;
        public FileImageSource Flag
        {
            get { return _flag; }
            set
            {
                _flag = value;
                OnPropertyChanged();
            }
        }

        private string _countryCode;
        public string CountryCode
        {
            get { return _countryCode; }
            set { SetProperty(ref _countryCode, value); }
        }

        private string _countryName;
        public string CountryName
        {
            get { return _countryName; }
            set
            {
                _countryName = value;
                OnPropertyChanged();
            }
        }

        private string _countryNativeName;
        public string CountryNativeName
        {
            get { return _countryNativeName; }
            set
            {
                _countryNativeName = value;
                OnPropertyChanged();
            }
        }

        private Color _selectedColor;
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set
            {
                _selectedColor = value;
                OnPropertyChanged();
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                Select();
            }
        }

        private void Select()
        {
            if (IsSelected)
                SelectedColor = Color.FromHex("#b624c1");
            else
                SelectedColor = Color.Transparent;
        }

        private ObservableCollection<Language> _languages;
        public ObservableCollection<Language> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }
    }
}
