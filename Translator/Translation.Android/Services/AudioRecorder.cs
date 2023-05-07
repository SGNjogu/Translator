using Android.Content;
using Android.Media;
using Plugin.CurrentActivity;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Core.Events;
using Translation.Core.Interfaces;
using Translation.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioRecorder))]
namespace Translation.Droid.Services
{
    public class AudioRecorder : IAudioRecorder
    {
        public event OnDataAvailable DataAvailable;
        private AudioRecord _audioRecorder;
        private AudioManager _audioManager;
        private byte[] _audioBuffer;
        public bool IsRecording { get; set; }
        private bool _isBluetoothConnected = false;
        private int _bluetoothScoRetryCount;

        private async Task InitalizeRecorder(InputDevice inputDevice)
        {
            if (_audioManager == null)
            {
                var context = CrossCurrentActivity.Current.AppContext;
                _audioManager = (AudioManager)context.GetSystemService(Context.AudioService);
            }

            var devices = _audioManager.GetDevices(GetDevicesTargets.Inputs);
            var selectedDevice = devices.FirstOrDefault(s => s.Address == inputDevice.Address);

            if (selectedDevice != null)
            {
                _audioBuffer = new byte[100000];
                _audioRecorder = new AudioRecord(
                  // Hardware source of recording.
#pragma warning disable CS0612 // Type or member is obsolete
                  await GetAudioSource(selectedDevice.Type),
#pragma warning restore CS0612 // Type or member is obsolete
                  // Frequency
                  16000,
                  // Mono or stereo
                  ChannelIn.Mono,
                  // Audio encoding
                  Encoding.Pcm16bit,
                  // Length of the audio clip.
                  _audioBuffer.Length
                );

                var setpreferredDevice = _audioRecorder.SetPreferredDevice(selectedDevice);

                if (setpreferredDevice)
                {
                    _audioManager.Mode = Mode.InCommunication;
                    _audioRecorder.StartRecording();
                    IsRecording = true;
                }
            }
        }

        [Obsolete]
        private async Task<AudioSource> GetAudioSource(AudioDeviceType deviceType)
        {
            _isBluetoothConnected = false;

            switch (deviceType)
            {
                case AudioDeviceType.BluetoothSco:
                    if (await SelectBluetoothDevice())
                    {
                        return AudioSource.Default;
                    }
                    else
                    {
                        return await GetAudioSource(AudioDeviceType.BuiltinMic);
                    }
                case AudioDeviceType.BuiltinMic:
                    SelectBuiltInSpeaker();
                    return AudioSource.Camcorder;
                case AudioDeviceType.UsbHeadset:
                    StopAllDevices();
                    return AudioSource.Default;
                case AudioDeviceType.WiredHeadset:
                    SelectWiredHeadset();
                    return AudioSource.Default;
                default:
                    return AudioSource.Default;
            }
        }

        public async Task StartRecording(InputDevice inputDevice)
        {
            await InitalizeRecorder(inputDevice);
            ReadAudioAsync();
        }

        private async void ReadAudioAsync()
        {
            DataAvailableEventArgs args = new DataAvailableEventArgs();

            while (IsRecording)
            {
                try
                {
                    // Keep reading the buffer while there is audio input.
                    var byteCount = await _audioRecorder.ReadAsync(_audioBuffer, 0, _audioBuffer.Length);
                    args.AudioDataInput.Bytes = _audioBuffer;
                    args.AudioDataInput.ByteCount = byteCount;
                    DataAvailable?.Invoke(this, args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }
        }

        [Obsolete]
        public void StopRecording()
        {
            try
            {
                IsRecording = false;

                if (_audioRecorder != null)
                {
                    _audioRecorder.Stop();
                    _audioRecorder.Release();
                }

                SelectBuiltInSpeaker();
                _audioManager.Mode = Mode.Normal;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [Obsolete]
        private void StopAllDevices()
        {
            StopBluetoothSco();
            StopInBuiltInSpeaker();
            StopWiredHeadset();
        }

        [Obsolete]
        private void SelectWiredHeadset()
        {
            StopBluetoothSco();
            StopInBuiltInSpeaker();
            if (_audioManager != null && !_audioManager.WiredHeadsetOn)
                _audioManager.WiredHeadsetOn = true;
        }

        [Obsolete]
        private void StopWiredHeadset()
        {
            if (_audioManager != null && _audioManager.WiredHeadsetOn)
                _audioManager.WiredHeadsetOn = false;
        }

        [Obsolete]
        private void SelectBuiltInSpeaker()
        {
            StopBluetoothSco();
            StopWiredHeadset();

            if (_audioManager != null && !_audioManager.SpeakerphoneOn)
                _audioManager.SpeakerphoneOn = true;
        }

        private void StopInBuiltInSpeaker()
        {
            if (_audioManager != null && _audioManager.SpeakerphoneOn)
                _audioManager.SpeakerphoneOn = false;
        }

        [Obsolete]
        public void StopBluetoothSco()
        {
            if (_audioManager != null && _audioManager.BluetoothScoOn)
            {
                _audioManager.StopBluetoothSco();
            }
        }

        [Obsolete]
        private async Task<bool> SelectBluetoothDevice()
        {
            StopBluetoothSco();
            StopWiredHeadset();
            StopInBuiltInSpeaker();
            _isBluetoothConnected = true;
            _bluetoothScoRetryCount = 3;
            return await StartBluetoothSco();
        }

        private async Task<bool> StartBluetoothSco()
        {
            if (_isBluetoothConnected)
            {
                if (_audioManager != null && !_audioManager.BluetoothScoOn)
                {
                    _audioManager.StartBluetoothSco();
                    await Task.Delay(2000);
                    if (!_audioManager.BluetoothScoOn && _bluetoothScoRetryCount > 0)
                    {
                        _bluetoothScoRetryCount--;
                        await StartBluetoothSco();
                    }
                    else if (_bluetoothScoRetryCount == 0)
                        return false;
                    return true;
                }
            }
            return false;
        }
    }
}