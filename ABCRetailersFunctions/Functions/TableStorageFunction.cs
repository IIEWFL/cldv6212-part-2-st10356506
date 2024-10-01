using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Services;
using ABCRetailersFunctions.Models;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;

namespace ABCRetailersFunctions.Functions
{
    //https://www.youtube.com/watch?v=B1CUJeKwpdw
    //function for creating customer to azure tables 
    public class TableStorageFunction
    {
        private readonly ILogger<TableStorageFunction> _logger;
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorage blobStorage;
        private readonly QueueStorageService queueStorageService;
           
        public TableStorageFunction(ILogger<TableStorageFunction> logger, TableStorageService tableStorageService, BlobStorage blobStorage, QueueStorageService queueStorageService)
        {
            _logger = logger;
            _tableStorageService = tableStorageService;
            this.blobStorage = blobStorage;
            this.queueStorageService = queueStorageService;
        }

        [Function("CreateCustomer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "abc-tables")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to create a new customer.");

            var form = await req.ReadFormAsync();
            int age;
            int.TryParse(form["Age"], out age); //this will set age to 0 if parsing fails

            var customer = new CustomerTable
            {
                Name = form["Name"],
                Surname = form["Surname"],
                Email = form["Email"],
                Age = age
            };

            //upload image
            if (req.Form.Files.Count > 0)
            {
                var image = req.Form.Files[0];
                using var stream = image.OpenReadStream();
                customer.ImageURL = await blobStorage.UploadBlobAsync(Guid.NewGuid().ToString(), stream);
            }
           
            _logger.LogInformation($"Received request to add customer. Name: {customer.Name}, Surname: {customer.Surname}, Email: {customer.Email}, Age: {customer.Age}");

            // Log before calling table storage service
            _logger.LogInformation("Calling table storage service to add customer.");

            await _tableStorageService.AddCustomers(customer);

            _logger.LogInformation("Customer successfully added to Table Storage.");

            return new OkObjectResult(customer);
        }
    }
}
