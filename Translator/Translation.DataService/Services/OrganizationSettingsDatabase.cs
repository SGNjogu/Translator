using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database
    {
        /// <summary>
        /// Method to get all Organization Settings Records
        /// </summary>
        /// <returns>All OrganizationSettings</returns>
        public async Task<List<OrganizationSettings>> GetOrganizationSettingsAsync()
        {
            return await Dataservice.Table<OrganizationSettings>().ToListAsync();
        }

        /// <summary>
        /// Method to get one OrganizationSettings
        /// </summary>
        /// <returns>Single Organization Settings</returns>
        public async Task<OrganizationSettings> GetOneOrganizationSettingsAsync()
        {
            return await Dataservice.Table<OrganizationSettings>().FirstOrDefaultAsync();
        }

        /// <summary>
        /// Method to get organization tags for the current user's organization
        /// </summary>
        /// <returns>All Tags</returns>
        public async Task<List<OrganizationTag>> GetOrganizationTagsAsync()
        {
            return await Dataservice.Table<OrganizationTag>().ToListAsync();
        }
    }
}
