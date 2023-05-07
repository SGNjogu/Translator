using System.Threading.Tasks;
using Translation.Core.Domain;

namespace Translation.Interface
{
    public interface IAudioFileSaver
    {
        Task SaveFile();
        void WriteToFile(string filePath, AudioDataInput audioDataInput);
    }
}