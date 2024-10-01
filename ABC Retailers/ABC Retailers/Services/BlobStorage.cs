using ABC_Retailers.Models;
using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;

//contains the blob storage methods for creating the storage container, uploading the images and downloading the images
//https://www.youtube.com/watch?v=SIBqRVJyQF0 
//https://stackoverflow.com/questions/60617190/how-do-i-store-a-picture-in-azure-blob-storage-in-asp-net-mvc-application 

namespace ABC_Retailers.Services
{
    public class BlobStorage
    {
        private readonly BlobContainerClient _blobContainerClient;

        public BlobStorage(string storageConnectionString, string storageContainerName)
        {
            var blobServiceClient = new BlobServiceClient(storageConnectionString);
            _blobContainerClient = blobServiceClient.GetBlobContainerClient(storageContainerName);
            _blobContainerClient.CreateIfNotExists();
            
        }

        public async Task <string> UploadBlobAsync(string blobName, Stream content)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content);
            return blobClient.Uri.ToString();
        }

        public async Task<Stream> DownloadBlobAsync(string blobName)
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
    }
}
