using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Files.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ABCRetailersFunctions.Services
{
    //service to store files 
    //https://stackoverflow.com/questions/29788540/how-to-share-files-between-multiple-asp-net-mvc-applications 
//https://www.c-sharpcorner.com/article/azure-storage-crud-operations-in-mvc-using-c-sharp-azure-table-storage-part-one/ 


    public class FileStorageService
    {
        private readonly ShareClient _shareClient;
        private readonly string _directoryName;

        public FileStorageService(string storageConnectionString, string shareName, string directoryName)
        {
            var shareServiceClient = new ShareServiceClient(storageConnectionString);
            _shareClient = shareServiceClient.GetShareClient(shareName);
            _directoryName = directoryName;

            _shareClient.CreateIfNotExists();
            var directoryClient = _shareClient.GetDirectoryClient(directoryName);
            directoryClient.CreateIfNotExists();
        }


        public async Task UploadFileAsync(string fileName, Stream fileStream)
        {
            var fileClient = _shareClient.GetDirectoryClient(_directoryName).GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var fileClient = _shareClient.GetDirectoryClient(_directoryName).GetFileClient(fileName);
            var downloadResponse = await fileClient.DownloadAsync();
            return downloadResponse.Value.Content;
        }
        public async Task DeleteFileAsync(string fileName)
        {
            var directoryClient = _shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }

        public async Task<List<ShareFileItem>> ListFilesAsync()
        {
            var directoryClient = _shareClient.GetDirectoryClient(_directoryName);
            var fileItems = new List<ShareFileItem>();

            await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (item.IsDirectory == false)
                {
                    fileItems.Add(item);
                }
            }
            return fileItems;
        }
    }
}
