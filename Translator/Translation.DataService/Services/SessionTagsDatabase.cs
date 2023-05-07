using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        public async Task<List<SessionTag>> CreateSessionTags(List<SessionTag> sessionTags, int sessionId)
        {
            if (sessionTags == null)
                throw new Exception($"List of session tags is empty");

            var existingSession = await GetOneSessionAsync(sessionId);

            if (existingSession != null)
            {
                if (sessionTags.Any())
                {
                    foreach (var sessionTag in sessionTags)
                    {
                        await AddItemAsync<SessionTag>(sessionTag);
                    }

                    return sessionTags;
                }
            }

            throw new Exception($"Session with id {sessionId} does not exist. Tags not created.");
        }

        public async Task<List<SessionTag>> GetSessionTags(int sessionId)
        {
            return await Dataservice.Table<SessionTag>().Where(i => i.SessionId == sessionId).ToListAsync();
        }

        public async Task DeleteSessionTagAsync(SessionTag sessionTag)
        {
            await Dataservice.DeleteAsync(sessionTag);
        }
    }
}
