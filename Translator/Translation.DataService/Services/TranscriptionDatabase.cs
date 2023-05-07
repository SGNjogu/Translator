using Translation.DataService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        /// <summary>
        /// Method to get all Transcriptions
        /// </summary>
        /// <returns>All Transcriptions</returns>
        public async Task<List<Transcription>> GetTranscriptionsAsync()
        {
            return await Dataservice.Table<Transcription>().ToListAsync();
        }

        /// <summary>
        /// Method to get one Transcription
        /// </summary>
        /// <returns>Single Transcription</returns>
        public async Task<Transcription> GetOneTranscriptionAsync(int transcriptionId)
        {
            return await Dataservice.Table<Transcription>().Where(s => s.ID == transcriptionId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Method to get transcriptions of a Session
        /// </summary>
        public async Task<List<Transcription>> GetSessionTranscriptions(int sessionId)
        {
            return await Dataservice.Table<Transcription>().Where(s => s.SessionId == sessionId).ToListAsync();
        }
    }
}
