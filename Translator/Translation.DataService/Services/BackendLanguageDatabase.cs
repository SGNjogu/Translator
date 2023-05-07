using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        /// <summary>
        /// Method to get all Backend Languages
        /// </summary>
        /// <returns>List of codes for backend languages</returns>
        public async Task<List<string>> GetBackendLanguagesAsync()
        {
            try
            {
                List<string> backendLanguageCodes = new List<string>();
                var languages = await Dataservice.Table<BackendLanguage>().ToListAsync();
                foreach (var language in languages)
                {
                    backendLanguageCodes.Add(language.Code);
                }
                return backendLanguageCodes;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
