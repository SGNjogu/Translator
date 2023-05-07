using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Translation.Interface;
using Translation.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Translation.Utils
{
    public enum ExternalFolder
    {
        Videos,
        Pictures,
        Documents,
        Downloads
    }

    public static class FileAccessUtility
    {
        public static async Task<string> PermanentlySaveFile(string folder, string fileName, byte[] fileBytes)
        {
            return await Task.Run(() => 
            {
                string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), folder);

                // Check if the folder exist or not
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filePath = Path.Combine(folderPath, fileName);

                // Try to write the file bytes to the specified location.
                try
                {
                    File.WriteAllBytes(filePath, fileBytes);
                    return filePath;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            });
        }

        public static async Task<string> SaveFileExternally(ExternalFolder folder, string fileName, byte[] fileBytes)
        {
            return await Task.Run(() => 
            {
                string folderPath = string.Empty;

                switch (folder)
                {
                    case ExternalFolder.Videos:
                        folderPath = DependencyService.Get<IStorageFolder>().GetVideosPath();
                        break;
                    case ExternalFolder.Pictures:
                        folderPath = DependencyService.Get<IStorageFolder>().GetPicturesPath();
                        break;
                    case ExternalFolder.Documents:
                        folderPath = DependencyService.Get<IStorageFolder>().GetDocumentsPath();
                        break;
                    case ExternalFolder.Downloads:
                        folderPath = DependencyService.Get<IStorageFolder>().GetDownloadsPath();
                        break;
                    default:
                        break;
                }

                // Check if the folder exist or not
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filePath = Path.Combine(folderPath, fileName);

                // Try to write the file bytes to the specified location.
                try
                {
                    File.WriteAllBytes(filePath, fileBytes);
                    return filePath;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            });
        }

        public static async Task<bool> CheckStoragePermissions()
        {
            var storagePermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (storagePermissionStatus == PermissionStatus.Granted)
            {
                return true;
            }
            else if (storagePermissionStatus != PermissionStatus.Granted)
            {
                var requestedPermissionStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();

                if (requestedPermissionStatus != PermissionStatus.Granted)
                {
                    return false;
                }
                else if (requestedPermissionStatus == PermissionStatus.Granted)
                {
                    return true;
                }
            }

            return false;
        }

        public static string ReturnFilePath(string fileName, string folder = "Recordings")
		{
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), folder);

            // Check if the folder exist or not
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return Path.Combine(folderPath, fileName);
		}

        public static string TemporarilySaveFile(string fileName, byte[] fileBytes)
        {
            var tmpdir = Path.GetTempPath();
            string folderPath = tmpdir;

            // Check if the folder exist or not
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filePath = Path.Combine(folderPath, fileName);

            // Try to write the file bytes to the specified location.
            try
            {
                File.WriteAllBytes(filePath, fileBytes);
                return filePath;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void DeleteDirectory(string folderName)
        {
            string imageFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), folderName);
            if (Directory.Exists(imageFolderPath))
            {
                Directory.Delete(imageFolderPath, true);
            }
        }

        public static void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static byte[] ReturnBytesFromStream(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
