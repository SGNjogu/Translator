using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        public async Task<UsageLimit> CreateUsageLimit(UsageLimit usageLimit)
        {
            var limit = await Dataservice.Table<UsageLimit>().FirstOrDefaultAsync();

            if (limit != null)
            {
                await Dataservice.DeleteAllAsync<UsageLimit>();
            }

            return await AddItemAsync<UsageLimit>(usageLimit);
        }

        public async Task<UsageLimit> GetUsageLimit()
        {
            return await Dataservice.Table<UsageLimit>().FirstOrDefaultAsync();
        }
    }
}
