using Android.Content;
using Android.Media;
using Plugin.CurrentActivity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Translation.Core.Domain;
using Translation.Core.Interfaces;
using Translation.Droid.Services.AudioDevices;
using Translation.Utils;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioDeviceService))]
namespace Translation.Droid.Services.AudioDevices
{
    public class AudioDeviceService : IAudioDeviceService
    {
        private AudioManager _audioManager;

        public async Task<List<InputDevice>> GetInputDevices()
        {
            List<InputDevice> inputDevicesList = new List<InputDevice>();

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
                            inputDevicesList.Add(CreateInputDevice(device));
                        }
                    }

                    var mainInputDevices = inputDevices.Where(t => t.Type == AudioDeviceType.BuiltinMic).ToList();
                    if (mainInputDevices.Any())
                    {
                        foreach (var device in mainInputDevices)
                        {
                            inputDevicesList.Add(CreateInputDevice(device));
                        }
                    }

                    var cableInputDevices = inputDevices.Where(t => t.Type == AudioDeviceType.WiredHeadphones || t.Type == AudioDeviceType.WiredHeadset).ToList();
                    if (cableInputDevices.Any())
                    {
                        foreach (var device in cableInputDevices)
                        {
                            inputDevicesList.Add(CreateInputDevice(device));
                        }
                    }

                    var usbInputDevices = inputDevices.Where(t => t.Type == AudioDeviceType.UsbDevice || t.Type == AudioDeviceType.UsbHeadset || t.Type == AudioDeviceType.UsbAccessory).ToList();
                    if (usbInputDevices.Any())
                    {
                        foreach (var device in usbInputDevices)
                        {
                            inputDevicesList.Add(CreateInputDevice(device));
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

        private InputDevice CreateInputDevice(AudioDeviceInfo device)
        {
            return new InputDevice { Address = device.Address, ProductName = device.ProductName, Type = EnumsConverter.ConvertToString(device.Type) };
        }

        private OutputDevice CreateOutputDevice(AudioDeviceInfo device)
        {
            return new OutputDevice { Address = device.Address, ProductName = device.ProductName, Type = EnumsConverter.ConvertToString(device.Type) };
        }

        public async Task<List<OutputDevice>> GetOutputDevices()
        {
            List<OutputDevice> outputDevicesList = new List<OutputDevice>();

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
                            outputDevicesList.Add(CreateOutputDevice(device));
                        }
                    }

                    var mainOutputDevices = outputDevices.Where(t => t.Type == AudioDeviceType.BuiltinSpeaker).ToList();
                    if (mainOutputDevices.Any())
                    {
                        foreach (var device in mainOutputDevices)
                        {
                            outputDevicesList.Add(CreateOutputDevice(device));
                        }
                    }

                    var cableOutputDevices = outputDevices.Where(t => t.Type == AudioDeviceType.WiredHeadphones || t.Type == AudioDeviceType.WiredHeadset).ToList();
                    if (cableOutputDevices.Any())
                    {
                        foreach (var device in cableOutputDevices)
                        {
                            outputDevicesList.Add(CreateOutputDevice(device));
                        }
                    }

                    var usbOutputDevices = outputDevices.Where(t => t.Type == AudioDeviceType.UsbDevice || t.Type == AudioDeviceType.UsbHeadset || t.Type == AudioDeviceType.UsbAccessory).ToList();
                    if (usbOutputDevices.Any())
                    {
                        foreach (var device in usbOutputDevices)
                        {
                            outputDevicesList.Add(CreateOutputDevice(device));
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

        public async Task<List<AudioDevice>> GetIODevices()
        {
            List<AudioDevice> audioDevices = new List<AudioDevice>();

            try
            {
                var inputDevices = await GetInputDevices();
                var outputDevices = await GetOutputDevices();

                foreach (var device in inputDevices)
                {
                    var matchingOutputDevice = outputDevices.FirstOrDefault(d => d.ProductName == device.ProductName);

                    if (matchingOutputDevice != null)
                    {
                        var ioDevice = new AudioDevice
                        {
                            InputDevice = device,
                            OutputDevice = matchingOutputDevice
                        };

                        var deviceType = EnumsConverter.ConvertToEnum<AudioDeviceType>(device.Type);

                        if (EnumsConverter.ConvertToEnum<AudioDeviceType>(device.Type) == AudioDeviceType.BuiltinMic)
                        {
                            ioDevice.ParentName = "Phone Mic & Speaker";
                            ioDevice.IsSpeaker = true;
                        }
                        else if (deviceType == AudioDeviceType.BluetoothSco)
                        {
                            ioDevice.ParentName = "Bluetooth Mic & Speaker";
                            ioDevice.IsBluetooth = true;
                            ioDevice.InputDevice.IsBluetooth = true;
                            ioDevice.OutputDevice.IsBluetooth = true;
                        }
                        else if (deviceType == AudioDeviceType.WiredHeadset)
                        {
                            ioDevice.ParentName = "Headset Mic & Speaker";
                            ioDevice.IsHeadset = true;
                        }
                        else if (deviceType == AudioDeviceType.UsbHeadset)
                        {
                            ioDevice.ParentName = "USB Device Mic & Speaker";
                            ioDevice.IsUSBDevice = true;
                        }
                        else
                        {
                            ioDevice.ParentName = device.ProductName;
                        }

                        audioDevices.Add(ioDevice);

                        outputDevices.Remove(matchingOutputDevice);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return audioDevices;
        }
    }
}
