using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.Hmac;
using Translation.Models;

namespace Translation.Services.Usage
{
    public class UsageTracking : IUsageTracking
    {
        private readonly IDataService _dataService;

        public UsageTracking(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<OrganizationUsageLimit> GetOrganizationUsageLimit(int organizationId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync($"{Constants.UsageTrackingEndpoint}/{organizationId}");

                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<OrganizationUsageLimit>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<UserUsageLimit> GetUserUsageLimit(int userId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync($"{Constants.UsersEndpoint}/{userId}/Limits");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserUsageLimit>(content);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound && content.Contains("does not have limits"))
                {
                    return null;
                }
                else
                {
                    throw new Exception(content);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task GetUsageLimits(int userId, int organizationId)
        {
            try
            {
                var organizationLimit = await GetOrganizationUsageLimit(organizationId);
                var userUsageLimit = await GetUserUsageLimit(userId);

                var usageLimit = new UsageLimit();

                if (organizationLimit != null)
                {
                    usageLimit.OrganizationBillingType = organizationLimit.BillingType;
                    usageLimit.OrganizationLicensingType = organizationLimit.LicensingType;
                    usageLimit.OrganizationStorageLimitExceeded = organizationLimit.StorageLimitExceeded;
                    usageLimit.OrganizationTranslationLimitExceeded = organizationLimit.TranslationLimitExceeded;
                }

                if (userUsageLimit != null)
                {
                    usageLimit.UserMaxSessionTime = userUsageLimit.MaxSessionTime;
                    usageLimit.UserStorageBytes = userUsageLimit.StorageBytes;
                    usageLimit.UserStorageTimeframe = userUsageLimit.StorageTimeframe;
                    usageLimit.UserTranslationMinutes = userUsageLimit.TranslationMinutes;
                    usageLimit.UserTranslationTimeframe = userUsageLimit.TranslationTimeframe;
                }

                await _dataService.CreateUsageLimit(usageLimit);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
