using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.Core.Domain;

namespace Translation.Core.Interfaces
{
    public interface IAudioDeviceService
    {
        Task<List<InputDevice>> GetInputDevices();
        Task<List<OutputDevice>> GetOutputDevices();
        Task<List<AudioDevice>> GetIODevices();
    }
}
