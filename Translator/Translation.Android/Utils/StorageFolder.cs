
using Android.OS;
using Translation.Droid.Utils;
using Translation.Interface;
using Xamarin.Forms;

[assembly: Dependency(typeof(StorageFolder))]
namespace Translation.Droid.Utils
{
    public class StorageFolder : IStorageFolder
    {
        public string GetDocumentsPath()
        {
            return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDocuments).AbsolutePath;
        }

        public string GetVideosPath()
        {
            return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryMovies).AbsolutePath;
        }

        public string GetMusicPath()
        {
            return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryMusic).AbsolutePath;
        }

        public string GetDownloadsPath()
        {
            return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath;
        }

        public string GetPicturesPath()
        {
            return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures).AbsolutePath;
        }
    }
}