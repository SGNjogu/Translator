using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Translation.Models
{

    public enum VoiceType
    {
        StandardVoice,
        NeuralVoice
    }

    public class Language : BaseViewModel, ICloneable
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged();
            }
        }

        private string _englishName;
        public string EnglishName
        {
            get { return _englishName; }
            set
            {
                _englishName = value;
                OnPropertyChanged();
            }
        }

        private string _code;
        public string Code
        {
            get { return _code; }
            set
            {
                _code = value;
                OnPropertyChanged();
            }
        }

        private string _displayCode;
        public string DisplayCode
        {
            get { return _displayCode; }
            set
            {
                _displayCode = value;
                OnPropertyChanged();
            }
        }

        private string _countryCode;
        public string CountryCode
        {
            get { return _countryCode; }
            set
            {
                _countryCode = value;
                OnPropertyChanged();
            }
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

        private string _voiceName;
        public string VoiceName
        {
            get { return _voiceName; }
            set
            {
                _voiceName = value;
                OnPropertyChanged();
            }
        }

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

        [JsonIgnore]
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

        public bool UseNeuralVoice { get; set; } = true;

        private void PreferNeuralVoice()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Voice))
                {
                    VoiceName = Voice;
                    UseNeuralVoice = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void PreferStandardVoice()
        {
            try
            {
                //All voices are now neurak, and this shouldn't be called at all.
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void SetPreferredVoice(VoiceType voiceType)
        {
            try
            {
                switch (voiceType)
                {
                    case VoiceType.StandardVoice:
                        PreferStandardVoice();
                        break;
                    case VoiceType.NeuralVoice:
                        PreferNeuralVoice();
                        break;
                    default:
                        PreferNeuralVoice();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private string _voice;
        public string Voice
        {
            get { return _voice; }
            set
            {
                _voice = value;
                OnPropertyChanged();
                SetPreferredVoice(VoiceType.NeuralVoice);
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return $"Name: {Name} Code: {Code} Voice: {Voice}";
        }
    }
}
