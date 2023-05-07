using System.Threading.Tasks;
using Translation.Models;

namespace Translation.Services.Versioning
{
    public interface IAppVersionService
    {
        Task<AppVersion> FetchAppVersion();
    }
}