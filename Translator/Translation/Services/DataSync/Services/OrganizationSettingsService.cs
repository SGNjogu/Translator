using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.Hmac;
using Translation.Services.DataSync.Interfaces;
using Xamarin.Forms;

namespace Translation.Services.DataSync.Services
{
    public class OrganizationSettingsService : IOrganizationSettingsService
    {
        private readonly IDataService _dataService;

        public OrganizationSettingsService(IDataService dataServiceSession)
        {
            _dataService = dataServiceSession;
        }

        public async Task UpdateOrganizationSettings()
        {
            try
            {
                var currentUser = await AppSettings.Settings.CurrentUser();
                if (currentUser == null)
                    return;

                if (currentUser.IsLoggedIn)
                {
                    var organizationSettings = await _dataService.GetOrganizationSettingsAsync();

                    //Update organization settings
                    var updatedOrganizationSettings = await FetchOrganizationSettings(currentUser.OrganizationId).ConfigureAwait(true);

                    if (updatedOrganizationSettings != null)
                    {
                        await _dataService.DeleteAllItemsAsync<OrganizationSettings>().ConfigureAwait(true);
                        var updateTask = await _dataService.AddItemAsync<OrganizationSettings>(new OrganizationSettings
                        {
                            OrganizationId = updatedOrganizationSettings.OrganizationId,
                            TranslationMinutesLimit = updatedOrganizationSettings.TranslationMinutesLimit,
                            AllowExplicitContent = updatedOrganizationSettings.AllowExplicitContent,
                            CopyPasteEnabled = updatedOrganizationSettings.CopyPasteEnabled,
                            ExportEnabled = updatedOrganizationSettings.ExportEnabled,
                            HistoryPlaybackEnabled = updatedOrganizationSettings.HistoryPlaybackEnabled,
                            HistoryAudioPlaybackEnabled = updatedOrganizationSettings.HistoryAudioPlaybackEnabled,
                            AutoUpdateDesktopApp = updatedOrganizationSettings.AutoUpdateDesktopApp,
                            AutoUpdateIOTApp = updatedOrganizationSettings.AutoUpdateIOTApp,
                            LanguageId = updatedOrganizationSettings.LanguageId,
                            LanguageCode = updatedOrganizationSettings.LanguageCode,
                            EnableSessionTags = updatedOrganizationSettings.EnableSessionTags
                        }).ConfigureAwait(true);
                    }

                    //Update Default Tags
                    var organizationTags = await FetchOrganizationTags(currentUser.OrganizationId).ConfigureAwait(true);

                    if (organizationTags != null && organizationTags.Any())
                    {
                        await _dataService.DeleteAllItemsAsync<OrganizationTag>().ConfigureAwait(true);

                        foreach (var tag in organizationTags)
                        {
                            await _dataService.AddItemAsync<OrganizationTag>(tag);
                        }
                    }

                    //Update Custom Tags
                    var organizationCustomTags = await GetOrganizationCustomTags(currentUser.OrganizationId).ConfigureAwait(true);

                    if (organizationCustomTags != null && organizationCustomTags.Any())
                    {
                        await _dataService.DeleteAllItemsAsync<CustomTag>().ConfigureAwait(true);

                        foreach (var tag in organizationCustomTags)
                        {
                            await _dataService.AddItemAsync<CustomTag>(tag);
                        }
                    }

                    // Update Organizations Questions
                    var organizationQuestions = await GetUserQuestions(currentUser.UserIntID);

                    if (organizationQuestions != null && organizationQuestions.Any())
                    {
                        await _dataService.DeleteAllItemsAsync<UserQuestions>().ConfigureAwait(true);

                        foreach (var question in organizationQuestions)
                        {
                            await _dataService.AddItemAsync<UserQuestions>(question);
                        }
                    }

                    //Update User Languages
                    var userLanguages = await GetUserLanguages(currentUser.UserIntID);

                    if(userLanguages != null && userLanguages.Any())
                    {
                        await _dataService.DeleteAllItemsAsync<BackendLanguage>().ConfigureAwait(true);

                        foreach (var language in userLanguages)
                        {
                            await _dataService.AddItemAsync<BackendLanguage>(language);
                        }
                    }

                    MessagingCenter.Instance.Send("", "UpdateLanguageLists");
                    MessagingCenter.Instance.Send("", "UpdateOrganizationSettings");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task<OrganizationSettings> FetchOrganizationSettings(int organizationId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();

                HttpResponseMessage response = await client.GetAsync($"{Constants.OrganizationSettingsEndpoint}/{organizationId}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<OrganizationSettings>(content);
                }

                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<OrganizationSettings> GetOrganizationSettings()
        {
            var currentOrganizationSettings = await _dataService.GetOneOrganizationSettingsAsync();
            return currentOrganizationSettings;
        }

        private async Task<List<OrganizationTag>> FetchOrganizationTags(int organizationId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();

                HttpResponseMessage response = await client.GetAsync($"{Constants.OrganizationTagsEndpoint}/{organizationId}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<OrganizationTag>>(content);
                }

                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<List<CustomTag>> GetOrganizationCustomTags(int organizationId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync($"{Constants.CustomTagListEndpoint}/{organizationId}/CustomTags");
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<CustomTag>>(content);
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<List<UserQuestions>> GetUserQuestions(int userId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync($"{Constants.UsersEndpoint}/{userId}/Questions");
                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseObj = JObject.Parse(content);
                    var userQuestions = responseObj[Constants.UserQuestions].ToObject<List<UserQuestions>>();
                    var organizationQuestions = responseObj[Constants.OrganizationQuestions].ToObject<List<UserQuestions>>();
                    userQuestions.AddRange(organizationQuestions);
                    return userQuestions;
                }
                throw new Exception(content);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        private async Task<List<BackendLanguage>> GetUserLanguages(int? userId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();

                HttpResponseMessage response = await client.GetAsync($"{Constants.UserLanguageEndpoint}{userId}/Languages");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<BackendLanguage>>(content);
                }

                throw new Exception(content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
