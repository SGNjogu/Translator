using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.Services.DataSync.Interfaces
{
    public interface IOrganizationSettingsService
    {
        Task<OrganizationSettings> GetOrganizationSettings();
        Task UpdateOrganizationSettings();
    }
}