using Android.Content;
using Android.Media;
using Plugin.CurrentActivity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Translation.Droid.Services;
using Translation.Interface;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioDevices))]
namespace Translation.Droid.Services
{
    public class AudioDevices : IAudioDevices
    {
        public AudioManager _audioManager;
        public AudioDevices()
        {

        }

        public async Task<List<AudioDeviceInfo>> GetAndroidInputDevices()
        {
            List<AudioDeviceInfo> inputDevicesList = new List<AudioDeviceInfo>();

            try
            {
                await Task.Run(() =>
                {

                    if (_audioManager == null)
                    {
                        var context = CrossCurrentActivity.Current.AppContext;
                        _audioManager = (AudioManager)context.GetSystemService(Context.AudioService);
                    }

                    var inputDevices = _audioManager.GetDevices(GetDevicesTargets.Inputs);

                    var bluetoothInputDevices = inputDevices.Where(t => t.Type == AudioDeviceType.BluetoothSco || t.Type == AudioDeviceType.BluetoothA2dp).ToList();
                    if (bluetoothInputDevices.Any())
                    {
                        foreach (var device in bluetoothInputDevices)
                        {
                            inputDevicesList.Add(device);
                        }
                    }

                    var mainInputDevices = inputDevices.Where(t => t.Type == AudioDeviceType.BuiltinMic).ToList();
                    if (mainInputDevices.Any())
                    {
                        foreach (var device in mainInputDevices)
                        {
                            inputDevicesList.Add(device);
                        }
                    }

                    var cableInputDevices = inputDevices.Where(t => t.Type == AudioDeviceType.WiredHeadphones || t.Type == AudioDeviceType.WiredHeadset).ToList();
                    if (cableInputDevices.Any())
                    {
                        foreach (var device in cableInputDevices)
                        {
                            inputDevicesList.Add(device);
                        }
                    }

                    var usbInputDevices = inputDevices.Where(t => t.Type == AudioDeviceType.UsbDevice || t.Type == AudioDeviceType.UsbHeadset || t.Type == AudioDeviceType.UsbAccessory).ToList();
                    if (usbInputDevices.Any())
                    {
                        foreach (var device in usbInputDevices)
                        {
                            inputDevicesList.Add(device);
                        }
                    }
                });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return inputDevicesList;
        }

        public async Task<List<AudioDeviceInfo>> GetAndroidOutputDevices()
        {
            List<AudioDeviceInfo> outputDevicesList = new List<AudioDeviceInfo>();

            try
            {
                await Task.Run(() =>
                {
                    if (_audioManager == null)
                    {
                        var context = CrossCurrentActivity.Current.AppContext;
                        _audioManager = (AudioManager)context.GetSystemService(Context.AudioService);
                    }

                    var outputDevices = _audioManager.GetDevices(GetDevicesTargets.Outputs);

                    var bluetoothOutputDevices = outputDevices.Where(t => t.Type == AudioDeviceType.BluetoothSco || t.Type == AudioDeviceType.BluetoothA2dp).ToList();
                    if (bluetoothOutputDevices.Any())
                    {
                        foreach (var device in bluetoothOutputDevices)
                        {
                            outputDevicesList.Add(device);
                        }
                    }

                    var mainOutputDevices = outputDevices.Where(t => t.Type == AudioDeviceType.BuiltinSpeaker).ToList();
                    if (mainOutputDevices.Any())
                    {
                        foreach (var device in mainOutputDevices)
                        {
                            outputDevicesList.Add(device);
                        }
                    }

                    var cableOutputDevices = outputDevices.Where(t => t.Type == AudioDeviceType.WiredHeadphones || t.Type == AudioDeviceType.WiredHeadset).ToList();
                    if (cableOutputDevices.Any())
                    {
                        foreach (var device in cableOutputDevices)
                        {
                            outputDevicesList.Add(device);
                        }
                    }

                    var usbOutputDevices = outputDevices.Where(t => t.Type == AudioDeviceType.UsbDevice || t.Type == AudioDeviceType.UsbHeadset || t.Type == AudioDeviceType.UsbAccessory).ToList();
                    if (usbOutputDevices.Any())
                    {
                        foreach (var device in usbOutputDevices)
                        {
                            outputDevicesList.Add(device);
                        }
                    }
                });
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return outputDevicesList;
        }
    }
}
