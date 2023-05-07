using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataSync.Interfaces
{
    public interface IPushDataService
    {
        void BeginDataSync();
        void CancelDataSync();
        Task<bool> UploadSession(Session session);
    }
}
