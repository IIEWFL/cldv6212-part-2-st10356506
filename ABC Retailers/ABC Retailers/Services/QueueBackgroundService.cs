using ABC_Retailers.Services;
using Azure.Storage.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

//queue background service for processing the queues
//https://stackoverflow.com/questions/1317641/queue-based-background-processing-in-asp-net-mvc-web-application 
public class QueueBackgroundService : BackgroundService
{
    private readonly QueueStorageService _queueStorageService;
    private readonly TableStorageService _tableStorageService;
    private readonly ILogger<QueueBackgroundService> _logger;

    public QueueBackgroundService(QueueStorageService queueStorageService, TableStorageService tableStorageService, ILogger<QueueBackgroundService> logger)
    {
        _queueStorageService = queueStorageService;
        _tableStorageService = tableStorageService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
    
    }
    //method for processing the queue
    public async Task ProcessMessageAsync(string message, CancellationToken stoppingToken)
    {
        if (message.Contains("order", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation($"Updating inventory for order: {message}");

            string productId = ExtractProductIdFromMessage(message);
            int quantity = ExtractQuantityFromMessage(message);

            await UpdateInventoryAsync(productId, quantity);
        }
        else
        {
            _logger.LogWarning($"Unknown message type: {message}");
        }
    }
    //method for extracting the product id
    public string ExtractProductIdFromMessage(string message)
    {
     
        var parts = message.Split(" ");
        return parts.Length > 1 ? parts[1] : string.Empty;
    }
    //method for extracting product quantity
    public int ExtractQuantityFromMessage(string message)
    {
        var parts = message.Split(" ");
        return parts.Length > 3 && int.TryParse(parts[3], out int quantity) ? quantity : 0;
    }

    //method for updating the inventory
    public async Task UpdateInventoryAsync(string productId, int quantity)
    {
        var product = await _tableStorageService.GetProductAsync("ProductsPartition", productId);
        if (product != null)
        {
            product.Availability = quantity;
            await _tableStorageService.UpdateProductAsync(product);
            _logger.LogInformation($"Inventory updated for Product ID: {productId}, New Quantity: {quantity}");
        }
        else
        {
            _logger.LogWarning($"Product with ID {productId} not found.");
        }
    }
}
