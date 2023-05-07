using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.AppSettings;
using Translation.Models;

namespace Translation.Services.Languages
{
    public interface ILanguagesService
    {
        List<Language> GetAutoDetectSupportedLanguages();
        Task<List<Country>> GetCountries();
        Task<Dictionary<string, string>> GetDefaultLanguages();
        Task<List<Language>> GetSupportedLanguages();
        void SetDefaultLanguage(string languageCode, Settings.Setting setting);
    }
}