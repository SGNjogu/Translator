using MvvmHelpers;
using System.Collections.Generic;
using System.Linq;
using Translation.Core.Domain;
using Translation.Core.Interfaces;
using Xamarin.Forms;

namespace Translation.ViewModels
{
    public class QuickStartDeviceViewModel : BaseViewModel
    {
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
                    SelectAudioDevice(SelectedAudioDevice);
                }
            }
        }

        private List<AudioDevice> _audioDevices;

        private readonly IAudioDeviceService _audioDeviceService;

        public List<AudioDevice> AudioDevices
        {
            get { return _audioDevices; }
            set
            {
                _audioDevices = value;
                OnPropertyChanged();
            }
        }

        public QuickStartDeviceViewModel(IAudioDeviceService audioDeviceService)
        {
            _audioDeviceService = audioDeviceService;
            MessagingCenter.Subscribe<AudioDevice>(this, "QuickStartSelectedAudioDevice", (sender) =>
            {
                SelectAudioDevice(sender);
            });
            LoadAudioDevices();
        }

        private async void LoadAudioDevices()
        {
            AudioDevices = await _audioDeviceService.GetIODevices();
            if (AudioDevices.Any())
                SelectedAudioDevice = AudioDevices[0];
        }

        private void SelectAudioDevice(AudioDevice audioDevice)
        {
            if (audioDevice != null)
            {
                AudioDevices.Select(c => { c.IsSelected = false; return c; }).ToList();
                AudioDevices.FirstOrDefault(s => s.OutputDevice.Address == audioDevice.OutputDevice.Address).IsSelected = true;
                MessagingCenter.Instance.Send(SelectedAudioDevice, "ChangeIODevice");
            }
        }
    }
}
