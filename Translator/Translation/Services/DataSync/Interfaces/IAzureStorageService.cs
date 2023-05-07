using System.Threading.Tasks;

namespace Translation.DataSync.Interfaces
{
    public interface IAzureStorageService
    {
        Task<bool> CheckIfBlobExists(string blobName, string azureStorageConnectionString, string containerName);
        Task<bool> UploadFile(string filePath, string fileName, string azureStorageConnectionString, string containerName);
    }
}