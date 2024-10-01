using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Services;

namespace ABCRetailersFunctions.Functions
{
    //function for uploading blob
    //https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-function-app-portal\
    //https://www.youtube.com/watch?v=kdRVComKwOc
    public class BlobStorageFunction
    {
        private readonly ILogger<BlobStorageFunction> _logger;
        private readonly BlobStorage _blobStorage;
        private readonly FileStorageService fileStorageService;

        public BlobStorageFunction(ILogger<BlobStorageFunction> logger, BlobStorage blobStorage, FileStorageService fileStorageService)
        {
            _logger = logger;
            _blobStorage = blobStorage;
            this.fileStorageService = fileStorageService;
        }

        [Function("UploadBlobAsync")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "abc-blobs")] HttpRequest req)
        {
            // Check if the request has a form
            if (!req.Headers.TryGetValue("Content-Type", out var contentType) ||
                !contentType.ToString().Contains("multipart/form-data"))
            {
                return new BadRequestObjectResult("Invalid Content-Type header.");
            }

            var form = await req.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null)
            {
                return new BadRequestObjectResult("No image uploaded.");
            }

            using (var stream = file.OpenReadStream())
            {
                await _blobStorage.UploadBlobAsync(file.FileName, stream);
            }

            return new OkObjectResult("Image uploaded successfully.");
        }
    }
    
}
