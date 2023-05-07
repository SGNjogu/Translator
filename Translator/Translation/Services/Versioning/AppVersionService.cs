using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;
using Translation.Hmac;
using Translation.Models;
using Translation.Utils;
using Xamarin.Essentials;

namespace Translation.Services.Versioning
{
    public class AppVersionService : IAppVersionService
    {
        private readonly IDataService _dataService;

        public AppVersionService(IDataService dataService)
        {
            _dataService = dataService;
        }

        // AppTypes
        //public enum AppType
        //{​
        //    Android,
        //    IOS,
        //    UWP,
        //    WPF
        //}​

        public async Task<AppVersion> FetchAppVersion()
        {
            try
            {
                int appType = 0;
                var platform = DeviceInfo.Platform;

                if (platform == DevicePlatform.Android)
                    appType = 0;
                else if (platform == DevicePlatform.iOS)
                    appType = 1;

                // check if on the latest version
                HttpClient client = HttpClientProvider.Create();
                HttpResponseMessage response = await client.GetAsync($"{Constants.AppVersionsEndpoint}/{appType}");

                var content = await response.Content.ReadAsStringAsync();

                var versions = await JsonConverter.ReturnObjectFromJsonString<List<AppVersion>>(content);

                if (versions.Any())
                {
                    AppVersion newVersion = versions.FirstOrDefault();

                    string[] newReleaseNotes = null;
                    DateTime releaseDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(newVersion.ReleaseNotes))
                    {
                        newReleaseNotes = newVersion.ReleaseNotes.Split(',');
                        releaseDate = newVersion.ReleaseDate;
                    }

                    // get the current version
                    AppVersion currentVersion = await GetCurrentVersion(versions, newVersion.IsForcedUpdate);

                    // compare versions
                    newVersion = IsLatestAppVersion(currentVersion, newVersion);

                    if (newReleaseNotes != null && newReleaseNotes.Any())
                    {
                        newVersion.ReleaseNotesList = new List<string>(newReleaseNotes);
                        newVersion.ReleaseDate = releaseDate;
                    }

                    return newVersion;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<AppVersion> GetCurrentVersion(List<AppVersion> appVersions, bool isForcedUpdate)
        {
            // get the current version
            AppVersion currentVersion = new AppVersion { Version = VersionTracking.CurrentVersion, IsForcedUpdate = isForcedUpdate };

            var releaseNotes = await _dataService.GetReleaseNotes();
            var releaseNotesAvailable = releaseNotes != null && releaseNotes.Any();

            if (!releaseNotesAvailable)
            {
                var currentReleaseNotes = appVersions.FirstOrDefault(s => s.Version == currentVersion.Version);

                if (currentReleaseNotes != null && !string.IsNullOrEmpty(currentReleaseNotes.ReleaseNotes))
                {
                    List<ReleaseNote> notes = new List<ReleaseNote>();
                    var newReleaseNotes = currentReleaseNotes.ReleaseNotes.Split(',');
                    foreach (var item in newReleaseNotes)
                    {
                        notes.Add(new ReleaseNote { DateReleased = currentReleaseNotes.ReleaseDate, Note = item });
                    }
                    await _dataService.CreateReleaseNotes(notes);
                }
            }
            else
            {
                currentVersion.ReleaseNotesList = releaseNotes.Select(s => s.Note).ToList();
            }

            return currentVersion;
        }

        private AppVersion IsLatestAppVersion(AppVersion currentVersion, AppVersion newVersion)
        {
            Version currentVersionNumber;
            Version newVersionNumber;
            Version.TryParse(currentVersion.Version, out currentVersionNumber);
            Version.TryParse(newVersion.Version, out newVersionNumber);

            if (currentVersionNumber != null && newVersionNumber != null)
            {
                var result = VersionComparer(newVersionNumber, currentVersionNumber);
                if (result >= 3)// newVersion is greater
                {
                    currentVersion.IsLatestVersion = false;
                    currentVersion.IsUnsurpotedVersion = true;
                }
                else if (result > 0) // newVersion is greater
                    currentVersion.IsLatestVersion = false;
                else if (result < 0) // currentVersionNumber is greater
                    currentVersion.IsLatestVersion = true;
                else // currentVersionNumber newVersion are equal
                    currentVersion.IsLatestVersion = true;
            }

            return currentVersion;
        }

        private int VersionComparer(Version newVersion, Version currentVersion)
        {
            if (newVersion.Major != currentVersion.Major)
                return newVersion.Major - currentVersion.Major;
            if (newVersion.Minor != currentVersion.Minor)
                return newVersion.Minor - currentVersion.Minor;
            if (newVersion.Build != currentVersion.Build)
                return newVersion.Build - currentVersion.Build;
            if (newVersion.Revision != currentVersion.Revision)
                return newVersion.Revision - currentVersion.Revision;
            return 0; // equal
        }
    }
}
