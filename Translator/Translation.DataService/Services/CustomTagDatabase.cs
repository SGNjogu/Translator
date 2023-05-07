using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        public async Task<List<CustomTag>> GetCustomTagsAsync()
        {
            return await Dataservice.Table<CustomTag>().ToListAsync();
        }
    }
}
