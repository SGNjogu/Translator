using Microsoft.AppCenter.Crashes;
using System.Threading.Tasks;

namespace Translation.Interface
{
    public interface IAppCrashlytics
    {
        Task<ErrorAttachmentLog[]> Attachments();
    }
}
