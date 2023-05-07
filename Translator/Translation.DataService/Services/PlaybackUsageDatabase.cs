using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        /// <summary>
        /// Method to get all PlaybackUsage
        /// </summary>
        /// <returns>All PlaybackUsage</returns>
        public async Task<List<PlaybackUsage>> GetPlaybackUsageAsync()
        {
            return await Dataservice.Table<PlaybackUsage>().ToListAsync();
        }
    }
}
