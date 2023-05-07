using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        /// <summary>
        /// Method to create release notes
        /// </summary>
        /// <returns>Created Release Notes</returns>
        public async Task<List<ReleaseNote>> CreateReleaseNotes(IEnumerable<ReleaseNote> releaseNotes)
        {
            var notes = await Dataservice.Table<ReleaseNote>().CountAsync();

            if (notes > 1)
            {
                await Dataservice.DeleteAllAsync<ReleaseNote>();
            }

            foreach (var releaseNote in releaseNotes)
            {
                await AddItemAsync<ReleaseNote>(releaseNote);
            }

            var newNotes = await GetReleaseNotes();

            return newNotes;
        }

        /// <summary>
        /// Method to get all release notes
        /// </summary>
        /// <returns>List of Release Notes</returns>
        public async Task<List<ReleaseNote>> GetReleaseNotes()
        {
            var releaseNotes = await Dataservice.Table<ReleaseNote>().ToListAsync();
            return releaseNotes;
        }
    }
}
