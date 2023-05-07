using System.Threading.Tasks;
using Translation.Models;

namespace Translation.Services.Usage
{
    public interface IUsageTracking
    {
        Task<OrganizationUsageLimit> GetOrganizationUsageLimit(int organizationId);
        Task<UserUsageLimit> GetUserUsageLimit(int userId);
        Task GetUsageLimits(int userId, int organizationId);
    }
}