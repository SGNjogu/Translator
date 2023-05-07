using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Translation.AppSettings;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.DataSync.Interfaces;
using Translation.Helpers;
using Translation.Hmac;
using Translation.Interface;
using Translation.Services.Languages;
using Translation.Utils;
using Xamarin.Forms;

namespace Translation.DataSync.Services
{
    public class PullDataService : IPullDataService
    {
        private readonly IDataService _dataService;
        private readonly IAppCrashlytics _crashlytics;
        private readonly ILanguagesService _languagesService;
        private BackgroundWorker BackgroundWorkerClient;


        public PullDataService(IDataService dataServiceSession, IAppCrashlytics crashlytics, ILanguagesService languagesService)
        {
            _dataService = dataServiceSession;
            _crashlytics = crashlytics;
            _languagesService = languagesService;
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
            catch(Exception ex)
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

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Do something when background work is completed
            if (e.Cancelled)
            {
                Debug.WriteLine("DATAPULL WAS CANCELLED");
            }
            else
            {
                Debug.WriteLine("DATAPULL UP TO DATE");
            }
        }

        public async Task SyncDatabase()
        {
            try
            {
                var settings = await Settings.CurrentUser();
                var languages = await _languagesService.GetSupportedLanguages();
                var sessionsCount = await _dataService.GetSessionCountAsync();

                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync($"{Constants.UserSessionsEndpoint}?organizationId={settings.OrganizationId}&userId={settings.UserIntID}&paginate=false");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var sessionsList = await JsonConverter.ReturnObjectFromJsonString<List<Session>>(content);

                    if (sessionsCount == 0)
                    {
                        foreach (var session in sessionsList)
                        {
                            var rawStart = DateTimeOffset.FromUnixTimeSeconds(session.StartTime).ToLocalTime();
                            var rawEnd = DateTimeOffset.FromUnixTimeSeconds(session.EndTime).ToLocalTime();

                            var localSession = await _dataService.AddItemAsync<Session>(new Session
                            {
                                CustomerId = settings.UserIntID,
                                TargetLangIso = session.TargetLangIso,
                                SourceLangISO = session.SourceLangISO,
                                SyncedToServer = true,
                                StartTime = session.StartTime,
                                RecordDate = rawStart.ToString("d"),
                                RawStartTime = rawStart.ToString("t"),
                                EndTime = session.EndTime,
                                RawEndTime = rawEnd.ToString("t"),
                                BillableSeconds = session.BillableSeconds,
                                SessionNumber = session.SessionNumber,
                                SessionName = session.SessionName,
                                CustomTags = session.CustomTags
                            });

                            var sourceLanguage = languages.FirstOrDefault(s => s.Code == session.SourceLangISO);
                            var targetLanguage = languages.FirstOrDefault(s => s.Code == session.TargetLangIso);

                            if (sourceLanguage != null)
                            {
                                localSession.SourceLanguage = sourceLanguage.Name;
                            }

                            if (targetLanguage != null)
                            {
                                localSession.TargeLanguage = targetLanguage.Name;
                            }

                            await SyncTranscriptions(session.ID, localSession.ID);
                            await UpdateSessionLocally(localSession);
                        }
                    }
                    else if (sessionsList.Count > sessionsCount)
                    {
                        var localSessions = await _dataService.GetSessionsAsync();

                        foreach (var session in sessionsList)
                        {
                            var exisingSession = localSessions.FirstOrDefault(s => s.StartTime == session.StartTime);

                            if (exisingSession == null)
                            {
                                var localSession = await _dataService.AddItemAsync<Session>(session);
                                await SyncTranscriptions(session.ID, localSession.ID);
                                await SyncSessionTags(session.ID, localSession.ID);
                                await UpdateSessionLocally(localSession);
                            }
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

        async Task SyncTranscriptions(int sessionId, int localSessionId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync($"{Constants.SessionTranscriptionsEndpoint}/{sessionId}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var transcriptions = await JsonConverter.ReturnObjectFromJsonString<List<Transcription>>(content);

                    foreach (var transcription in transcriptions)
                    {
                        await _dataService.AddItemAsync<Transcription>(new Transcription
                        {
                            ChatTime = transcription.ChatTime,
                            ChatUser = transcription.ChatUser,
                            OriginalText = transcription.OriginalText,
                            SessionId = localSessionId,
                            SyncedToServer = true,
                            TranslatedText = transcription.TranslatedText,
                            Sentiment = transcription.Sentiment,
                            TranslationSeconds = transcription.TranslationSeconds
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task SyncSessionTags(int sessionId, int localSessionId)
        {
            try
            {
                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync($"{Constants.SessionTagsGetEndpoint}/{sessionId}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var sessionTags = await JsonConverter.ReturnObjectFromJsonString<List<SessionTag>>(content);

                    foreach (var sessionTag in sessionTags)
                    {
                        await _dataService.AddItemAsync<SessionTag>(new SessionTag
                        {
                            SessionId = localSessionId,
                            SyncedToServer = true,
                            OrganizationId = sessionTag.OrganizationId,
                            OrganizationTagId = sessionTag.OrganizationTagId,
                            TagValue = sessionTag.TagValue
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }

        private async Task UpdateSessionLocally(Session session)
        {
            try
            {
                session.SyncedToServer = true;

                if (string.IsNullOrEmpty(session.SourceLanguage) || string.IsNullOrEmpty(session.TargeLanguage))
                {
                    var languages = await _languagesService.GetSupportedLanguages();

                    var sourceLanguage = languages.FirstOrDefault(s => s.Code == session.SourceLangISO);
                    var targetLanguage = languages.FirstOrDefault(s => s.Code == session.TargetLangIso);

                    if (sourceLanguage != null)
                    {
                        session.SourceLanguage = sourceLanguage.Name;
                    }

                    if (targetLanguage != null)
                    {
                        session.TargeLanguage = targetLanguage.Name;
                    }
                }

                var updated = await _dataService.UpdateItemAsync<Session>(session);
                MessagingCenter.Instance.Send(updated, "AddSession");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Crashes.TrackError(ex, attachments: await _crashlytics.Attachments());
            }
        }
    }
}
