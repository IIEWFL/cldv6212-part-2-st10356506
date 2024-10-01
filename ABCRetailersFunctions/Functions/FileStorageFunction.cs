using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Services;
using ABCRetailersFunctions.Models;

namespace ABCRetailersFunctions.Functions
{
    //function for uploading a file to azure 
    //https://www.youtube.com/watch?v=B1CUJeKwpdw
    public class FileStorageFunction
    {
        private readonly ILogger<FileStorageFunction> _logger;
        private readonly FileStorageService _fileStorageService;

        public FileStorageFunction(ILogger<FileStorageFunction> logger, FileStorageService fileStorageService)
        {
            _logger = logger;
            _fileStorageService = fileStorageService;
        }

        [Function("UploadFile")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "abc-files")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            if (!req.HasFormContentType)
            {
                return new BadRequestObjectResult("Invalid content type. Please upload a file.");
            }

            var formCollection = await req.ReadFormAsync();
            var file = formCollection.Files["file"];

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("Please upload a valid file.");
            }

            // Generate a unique file name
            var fileName = $"{Guid.NewGuid()}-{file.FileName}";

            // Upload the file to Azure File Share
            using (var stream = file.OpenReadStream())
            {
                await _fileStorageService.UploadFileAsync(fileName, stream);
            }

            _logger.LogInformation($"File {fileName} uploaded successfully to Azure File Share.");
            return new OkObjectResult($"File {fileName} uploaded successfully.");
        }
    }
}

