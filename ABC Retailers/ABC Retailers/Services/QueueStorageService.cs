using Azure.Storage.Queues;
using System.Text.Json;
using System.Threading.Tasks;

namespace ABC_Retailers.Services
{
    public class QueueStorageService
    //https://learn.microsoft.com/en-us/azure/storage/queues/storage-tutorial-queues 
    {
        private readonly QueueClient _queueClient;
        private readonly ILogger<QueueStorageService> _logger;

        public QueueStorageService(string storageConnectionString, string queueName, ILogger<QueueStorageService> logger)
        {
            var queueServiceClient = new QueueServiceClient(storageConnectionString);
            _queueClient = queueServiceClient.GetQueueClient(queueName);
            _queueClient.CreateIfNotExists();
            _logger = logger;
        }
        public async Task<string> ReceiveMessageAsync()
        {
            // Receive up to 10 messages from the queue
            var response = await _queueClient.ReceiveMessagesAsync(maxMessages: 10);
            foreach (var message in response.Value)
            {
                // Process the message
                string messageText = message.MessageText;

                // Delete the message from the queue after processing
                await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

                return messageText;
            }

            return null; // Return null if no messages are available
        }
        public async Task SendMessageAsync(string message)
        {
            if (_queueClient.Exists())
            {
                string messageBody = JsonSerializer.Serialize(message);
                await _queueClient.SendMessageAsync(messageBody);
                _logger.LogInformation($"Message '{messageBody}' has been sent to the queue.");
            }
            else
            {
                _logger.LogError($"Queue '{_queueClient.Name}' does not exist.");
            }
        
        }

        public async Task<string> PeekMessageAsync()
        {
            var response = await _queueClient.PeekMessageAsync();
            return response.Value.MessageText;
        }

        public async Task<string> DequeueMessageAsync()
        {
            var response = await _queueClient.ReceiveMessagesAsync();
            var message = response.Value.FirstOrDefault();
            if (message != null)
            {
                await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                return message.MessageText;
            }
            return null;
        }
        public async Task EnqueueMessageAsync(string message)
        {
            if (_queueClient.Exists())
            {
                await _queueClient.SendMessageAsync(message);
            }
        }
    }
}
