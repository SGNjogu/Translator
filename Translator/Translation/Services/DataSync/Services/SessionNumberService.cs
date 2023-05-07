using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Translation.Hmac;
using Translation.Models;
using Translation.Services.DataSync.Interfaces;

namespace Translation.Services.DataSync.Services
{
    public class SessionNumberService : ISessionNumberService
    {
        /// <summary>
        /// Gets a custom session number for the session
        /// </summary>
        /// <returns></returns>
        public async Task<SessionNumber> GetSessionNumber()
        {
            SessionNumber sessionNumber = new SessionNumber() { ReferenceNumber = "N/A" };

            try
            {
                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync(Constants.SessionNumberEndpoint);
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    sessionNumber = JsonConvert.DeserializeObject<SessionNumber>(content);
                }
            }
            catch (Exception ex) { throw ex; }

            return sessionNumber;
        }
    }
}
