using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.DataSync.Interfaces;
using Translation.Hmac;
using Translation.Interface;
using Translation.Services.Usage;
using Translation.Utils;
using Xamarin.Forms;

namespace Translation.DataSync.Services
{
    public class PushDataService : IPushDataService
    {
        private readonly IDataService _dataService;
        private readonly IAppCrashlytics _crashlytics;
        private readonly IAzureStorageService _azureStorageService;
        private readonly IUsageTracking _usageTracking;
        private BackgroundWorker BackgroundWorkerClient;

        public PushDataService(IDataService dataServiceSession, IAppCrashlytics crashlytics, IAzureStorageService azureStorageService, IUsageTracking usageTracking)
        {
            _dataService = dataServiceSession;
            _crashlytics = crashlytics;
            _azureStorageService = azureStorageService;
            _usageTracking = usageTracking;
            BackgroundWorkerClient = new BackgroundWorker();
            BackgroundWorkerClient.DoWork += DoWork;
            BackgroundWorkerClient.RunWorkerCompleted += RunWorkerCompleted;
        }

        public void BeginDataSync()
        {
            try
            {
                BackgroundWorkerClient.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void CancelDataSync()
        {
            try
            {
                BackgroundWorkerClient.CancelAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void DoWork(object sender, DoWorkEventArgs e)
        {
            await SyncDatabase().ConfigureAwait(false);
        }

        private async Task SyncDatabase()
        {
            try
            {
                var settings = await AppSettings.Settings.CurrentUser(); ;

                var usage = await _usageTracking.GetOrganizationUsageLimit(settings.OrganizationId);
                bool hasUnlimitedLicense = usage.LicensingType.ToLower() == "postpaid";

                if (usage.StorageLimitExceeded == true && !hasUnlimitedLicense)
                    return;

                var unsyncedSessions = await UnsyncedSessions();

                if (unsyncedSessions != null && unsyncedSessions.Any())
                {
                    foreach (var session in unsyncedSessions)
                    {
                        var sessionSynced = await UploadSession(session);
                        if (sessionSynced)
                        {
                            await UpdateSessionLocally(session);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task<bool> SyncTranscriptions(int localSessionID, int apiSessionId)
        {
            try
            {
                var unsyncedTranscriptions = await UnsyncedTranscriptions(localSessionID);

                if (unsyncedTranscriptions != null && unsyncedTranscriptions.Any())
                {
                    foreach (var transcription in unsyncedTranscriptions)
                    {
                        transcription.SessionId = apiSessionId;
                    }
                    var bulkTrancriptions = new BulkTranscriptions() { SessionId = apiSessionId, Transcriptions = unsyncedTranscriptions };
                    var uploaded = await UploadBulkTranscriptions(bulkTrancriptions, localSessionID);
                    if (uploaded == false)
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return false;
        }

        private async Task SyncSessionTags(int localSessionID, int apiSessionId)
        {
            try
            {
                var unsyncedSessionTags = await UnsyncedSessionTags(localSessionID);

                if (unsyncedSessionTags != null && unsyncedSessionTags.Any())
                {
                    foreach (var sessionTag in unsyncedSessionTags)
                    {
                        sessionTag.SessionId = apiSessionId;
                        await UploadSessionTag(sessionTag, localSessionID);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Do something when background work is completed
            if (e.Cancelled)
            {
                Debug.WriteLine("DATAPUSH WAS CANCELLED");
            }
            else
            {
                Debug.WriteLine("DATAPUSH UP TO DATE");
            }
        }

        private async Task<List<Session>> UnsyncedSessions()
        {
            try
            {
                var sessions = await _dataService.GetSessionsAsync();
                return sessions.Where(s => s.SyncedToServer == false).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return null;
        }

        private async Task<List<Transcription>> UnsyncedTranscriptions(int localSessionId)
        {
            try
            {
                var transcriptions = await _dataService.GetTranscriptionsAsync();
                return transcriptions.Where(s => s.SyncedToServer == false && s.SessionId == localSessionId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return null;
        }

        private async Task<List<SessionTag>> UnsyncedSessionTags(int localSessionId)
        {
            List<SessionTag> sessionTags = null;

            try
            {
                sessionTags = await _dataService.GetSessionTags(localSessionId);
                sessionTags = sessionTags.Where(s => s.SyncedToServer == false && s.SessionId == localSessionId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return await Task.FromResult(sessionTags);
        }

        public async Task<bool> UploadSession(Session session)
        {
            try
            {
                bool isAudioFileUploaded = await IsAudioFileUploaded($"{session.StartTime}.wav");

                if (isAudioFileUploaded)
                {
                    int sessionID = session.ID;

                    // changing session ID to 0 so that JsonConvert can ignore default values
                    session.ID = 0;

                    HttpClient client = HttpClientProvider.Create();
                    HttpResponseMessage response = await client.PostAsJsonAsync(Constants.SessionsEndpoint, session);

                    var content = await response.Content.ReadAsStringAsync();

                    session.ID = sessionID;
                    if (response.IsSuccessStatusCode)
                    {
                        int apiSessionId = GetID(content);
                        var transcriptionsSynced = await SyncTranscriptions(localSessionID: sessionID, apiSessionId: apiSessionId);
                        if (transcriptionsSynced)
                        {
                            await SyncSessionTags(localSessionID: session.ID, apiSessionId: apiSessionId);
                        }
                        else
                        {
                            return false;
                        }
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"ERROR UPLOADING SESSION: {content}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return false;
        }

        private async Task<bool> IsAudioFileUploaded(string fileName)
        {
            try
            {
                string filePath = FileAccessUtility.ReturnFilePath(fileName);

                if (File.Exists(filePath))
                {
                    bool isUploaed = await _azureStorageService.UploadFile(filePath, fileName, Constants.AzureStorageConnectionString, Constants.RecordingsContainer);

                    if (isUploaed)
                    {
                        FileAccessUtility.DeleteFile(filePath);
                        return true;
                    }

                    return false;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return false;
        }

        private async Task UpdateSessionLocally(Session session)
        {
            try
            {
                session.SyncedToServer = true;
                var updated = await _dataService.UpdateItemAsync<Session>(session);
                MessagingCenter.Instance.Send(updated, "UpdateSessionSyncStatus");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        /// <summary>
        /// Upload bulk transcriptions
        /// </summary>
        /// <param name="bulkTranscription"></param>
        /// <param name="localSessionID"></param>
        /// <returns></returns>
        private async Task<bool> UploadBulkTranscriptions(BulkTranscriptions transcriptions, int localSessionID)
        {
            try
            {
                if (transcriptions != null && localSessionID != 0)
                {
                    HttpClient client = HttpClientProvider.Create();
                    HttpResponseMessage response = await client.PostAsJsonAsync(Constants.CreateBulkTranscriptionsEndpoint, transcriptions);
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var returnedTranscriptions = JsonConvert.DeserializeObject<List<Transcription>>(content);
                        foreach (var singleTranscription in returnedTranscriptions)
                        {
                            // reassigning session ID when saving locally
                            await UpdateTranscriptionLocally(singleTranscription, localSessionID);
                        }
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"ERROR UPLOADING Transcription: {content}");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }

            return false;
        }

        public async Task UploadSessionTag(SessionTag sessionTag, int localSessionID)
        {
            try
            {
                int tagID = sessionTag.ID;

                // changing transcription ID to 0 so that JsonConvert can ignore default values
                sessionTag.ID = 0;

                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.PostAsJsonAsync(Constants.SessionTagsCreateEndpoint, sessionTag);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // reassigning session ID when saving locally
                    sessionTag.ID = tagID;
                    await UpdateSessiontagLocally(sessionTag, localSessionID);
                }
                else
                {
                    Debug.WriteLine($"ERROR UPLOADING SESSION: {content}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateTranscriptionLocally(Transcription transcription, int localSessionID)
        {
            try
            {
                transcription.SyncedToServer = true;
                transcription.SessionId = localSessionID;
                await _dataService.UpdateItemAsync<Transcription>(transcription);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateSessiontagLocally(SessionTag sessionTag, int localSessionID)
        {
            try
            {
                sessionTag.SyncedToServer = true;
                sessionTag.SessionId = localSessionID;
                await _dataService.UpdateItemAsync<SessionTag>(sessionTag);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private static int GetID(string content)
        {
            var jObject = JObject.Parse(content);

            return (int)jObject.GetValue("id");
        }

        private async Task DeletePlaybackUsageLocally(PlaybackUsage playbackUsage)
        {
            try
            {
                await _dataService.DeleteItemAsync<PlaybackUsage>(playbackUsage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }
    }
}
