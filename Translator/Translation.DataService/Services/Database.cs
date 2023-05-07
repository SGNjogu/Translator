using SQLite;
using System;
using System.Threading.Tasks;
using Translation.DataService.Interfaces;
using Translation.DataService.Models;

namespace Translation.DataService.Services
{
    public partial class Database : IDataService
    {
        #region properties

        SQLiteAsyncConnection Dataservice => LazyInitializer.Value;
        bool initialized = false;

        #endregion

        #region Methods

        /// <summary>
        /// Tries to initialize database lazily
        /// </summary>
        readonly Lazy<SQLiteAsyncConnection> LazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        });

        /// <summary>
        /// Method to Initialize Database
        /// </summary>
        public async Task InitializeAsync()
        {
            // Initialization
            if (!initialized)
            {
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(Session)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(Transcription)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(PlaybackUsage)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(OrganizationSettings)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(OrganizationTag)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(SessionTag)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(ReleaseNote)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(CustomTag)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(UsageLimit)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(RecentLanguage)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(UserQuestions)).ConfigureAwait(false);
                await Dataservice.CreateTablesAsync(CreateFlags.None, typeof(BackendLanguage)).ConfigureAwait(false);

                initialized = true;
            }
        }

        #endregion
    }
}
