
using System.Collections.Generic;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        public async Task<List<UserQuestions>> GetOrgQuestionsAsync()
        {
            return await Dataservice.Table<UserQuestions>().ToListAsync();
        }
    }
}
