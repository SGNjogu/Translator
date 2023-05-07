using MvvmHelpers;
using Translation.Utils;

namespace Translation.Core.Domain
{
    public class AudioDevice : BaseViewModel
    {
        public string ParentName { get; set; }

        private bool _isSpeaker = false;
        public bool IsSpeaker
        {
            get { return _isSpeaker; }
            set
            {
                _isSpeaker = value;
                OnPropertyChanged();
                if (IsSpeaker)
                    Glyph = FontAwesomeIcons.Mobile;
            }
        }

        private bool _isBluetooth = false;
        public bool IsBluetooth
        {
            get { return _isBluetooth; }
            set
            {
                _isBluetooth = value;
                OnPropertyChanged();
                if (IsBluetooth)
                    Glyph = FontAwesomeIcons.Headset;
            }
        }

        private bool _isHeadset = false;
        public bool IsHeadset
        {
            get { return _isHeadset; }
            set
            {
                _isHeadset = value;
                OnPropertyChanged();
                if (IsHeadset)
                    Glyph = FontAwesomeIcons.Headset;
            }
        }

        private bool _isUSBDevice = false;
        public bool IsUSBDevice
        {
            get { return _isUSBDevice; }
            set
            {
                _isUSBDevice = value;
                OnPropertyChanged();
                if (IsUSBDevice)
                    Glyph = FontAwesomeIcons.Usb;
            }
        }

        public InputDevice InputDevice { get; set; }
        public OutputDevice OutputDevice { get; set; }

        private string _glyph;
        public string Glyph
        {
            get { return _glyph; }
            set
            {
                _glyph = value;
                OnPropertyChanged();
            }
        }

        private string _selectedHexColor;
        public string SelectedColor
        {
            get { return _selectedHexColor; }
            set
            {
                _selectedHexColor = value;
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
                SelectedColor = "#b624c1";
            else
                SelectedColor = "#00FFFFFF";
        }
    }
}
