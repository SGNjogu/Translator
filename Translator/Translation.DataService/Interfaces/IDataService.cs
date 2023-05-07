using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Interfaces
{
    public interface IDataService
    {
        Task InitializeAsync();
        Task<T> AddItemAsync<T>(object obj);
        Task<T> UpdateItemAsync<T>(object obj);
        Task<T> DeleteItemAsync<T>(object obj);
        Task DeleteAllItemsAsync<T>();
        Task<List<Session>> GetSessionsAsync();
        Task<Session> GetOneSessionAsync(int sessionId);
        Task<List<Transcription>> GetTranscriptionsAsync();
        Task<Transcription> GetOneTranscriptionAsync(int transcriptionId);
        Task<List<Transcription>> GetSessionTranscriptions(int sessionId);
        Task<int> GetSessionCountAsync();
        Task<List<PlaybackUsage>> GetPlaybackUsageAsync();
        Task<List<OrganizationSettings>> GetOrganizationSettingsAsync();
        Task<OrganizationSettings> GetOneOrganizationSettingsAsync();
        Task<List<SessionTag>> CreateSessionTags(List<SessionTag> sessionTags, int sessionId);
        Task<List<SessionTag>> GetSessionTags(int sessionId);
        Task DeleteSessionTagAsync(SessionTag sessionTag);
        Task<List<OrganizationTag>> GetOrganizationTagsAsync();
        Task<List<ReleaseNote>> CreateReleaseNotes(IEnumerable<ReleaseNote> releaseNotes);
        Task<List<ReleaseNote>> GetReleaseNotes();
        Task<List<CustomTag>> GetCustomTagsAsync();
        Task<UsageLimit> CreateUsageLimit(UsageLimit usageLimit);
        Task<UsageLimit> GetUsageLimit();
        Task AddRecentangauages(string languageCode);
        Task<List<RecentLanguage>> GetRecentLanguages();
        Task<List<UserQuestions>> GetOrgQuestionsAsync();
        Task<List<string>> GetBackendLanguagesAsync();

    }
}
