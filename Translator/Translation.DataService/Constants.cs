using System;
using System.IO;

namespace Translation.DataService
{
    public static class Constants
    {
        public const SQLite.SQLiteOpenFlags Flags =
           // open the database in read/write mode
           SQLite.SQLiteOpenFlags.ReadWrite |
           // create the database if it doesn't exist
           SQLite.SQLiteOpenFlags.Create |
           // enable multi-threaded database access
           SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(basePath, "Speechly.db");
            }
        }
    }
}
