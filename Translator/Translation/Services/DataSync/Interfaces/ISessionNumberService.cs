using System.Threading.Tasks;
using Translation.Models;

namespace Translation.Services.DataSync.Interfaces
{
    public interface ISessionNumberService
    {
        Task<SessionNumber> GetSessionNumber();
    }
}