using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.IO;
using System.Threading.Tasks;

namespace ABC_Retailers.Services
{
    //storage service for the file share methods to create the file container upload the file, download the file, delete the file and list the file,
    //https://www.youtube.com/watch?v=SIBqRVJyQF0 
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

        //upload files
        public async Task UploadFileAsync(string fileName, Stream fileStream)
        {
            var fileClient = _shareClient.GetDirectoryClient(_directoryName).GetFileClient(fileName);
            await fileClient.CreateAsync(fileStream.Length);
            await fileClient.UploadAsync(fileStream);
        }

        //downlaod files
        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var fileClient = _shareClient.GetDirectoryClient(_directoryName).GetFileClient(fileName);
            var downloadResponse = await fileClient.DownloadAsync();
            return downloadResponse.Value.Content;
        }
        //delete files
        public async Task DeleteFileAsync(string fileName)
        {
            var directoryClient = _shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.DeleteIfExistsAsync();
        }

        //display files
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
