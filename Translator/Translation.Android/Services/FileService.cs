using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Translation.Droid.Services;
using Translation.Services.Download;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileService))]
namespace Translation.Droid.Services
{

    public class FileService : IFileService
    {
        public string GetStorageFolderPath()
        {
            return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;

        }
    }
}