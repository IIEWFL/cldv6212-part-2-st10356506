using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Services;
using ABCRetailersFunctions.Models;

namespace ABCRetailersFunctions.Functions
{
    //function to add queue to azure 
    //https://www.youtube.com/watch?v=B1CUJeKwpdw
    public class QueueStorageFunction
    {
        private readonly ILogger<QueueStorageFunction> _logger;
        private readonly QueueStorageService _queueStorageService;

        public QueueStorageFunction(ILogger<QueueStorageFunction> logger, QueueStorageService queueStorageService)
        {
            _logger = logger;
            _queueStorageService = queueStorageService;
        }

        [Function("TransactionQueue")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "abc-queues")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Error sending message to queue");
            }

            // Send the message to the Azure Queue
            await _queueStorageService.SendMessageAsync(requestBody);

            _logger.LogInformation("Transaction added to queue successfully.");
            return new OkObjectResult("Transaction added to queue successfully.");
        }
    }
}
