using System.Threading.Tasks;

namespace Translation.DataSync.Interfaces
{
    public interface IPullDataService
    {
        void BeginDataSync();
        void CancelDataSync();
        Task SyncDatabase();
    }
}
