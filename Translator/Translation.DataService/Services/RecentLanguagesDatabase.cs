using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        public async Task<List<RecentLanguage>> GetRecentLanguages()
        {
            var rencetLanguages = await Dataservice.Table<RecentLanguage>().ToListAsync();
            return rencetLanguages.Take(5).OrderBy(s => s.ID).ToList();
        }

        public async Task AddRecentangauages(string languageCode)
        {
            var recentLanguages = await GetRecentLanguages();

            if (recentLanguages.Count == 5)
            {
                await DeleteItemAsync<RecentLanguage>(recentLanguages.Last());
            }
            else
            {
                var recentLanguage = new RecentLanguage { LanguageCode = languageCode };
                await AddItemAsync<RecentLanguage>(recentLanguage);
            }
        }
    }
}
