using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Translation.DataSync.Interfaces;

namespace Translation.DataSync.Services
{
    public class AzureStorageService : IAzureStorageService
    {
        public async Task<bool> UploadFile(string filePath, string fileName, string azureStorageConnectionString, string containerName)
        {
            var blobServiceClient = new BlobServiceClient(azureStorageConnectionString);
            bool isUploaded = false;
            FileStream uploadFileStream = null;
            try
            {
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);
                if (File.Exists(filePath))
                {
                    uploadFileStream = File.OpenRead(filePath);
                    var result = await blobClient.UploadAsync(uploadFileStream, false);
                    var status = result.GetRawResponse().Status;
                    uploadFileStream.Close();

                    if (status == 201)
                    {
                        isUploaded = true;
                    }
                }
                else
                {
                    throw new Exception($"The file {filePath} does not exist.");
                }
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode == "BlobAlreadyExists")
                {
                    isUploaded = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (uploadFileStream != null)
                {
                    uploadFileStream.Close();
                }
            }

            return isUploaded;
        }

        public async Task<bool> CheckIfBlobExists(string blobName, string azureStorageConnectionString, string containerName)
        {
            bool exists;
            try
            {
                var blobServiceClient = new BlobServiceClient(azureStorageConnectionString);
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
                exists = await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return exists;
        }
    }
}