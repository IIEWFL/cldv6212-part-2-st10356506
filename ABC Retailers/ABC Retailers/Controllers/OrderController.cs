using Microsoft.AspNetCore.Mvc;
using ABC_Retailers.Services;
using ABC_Retailers.Models;

namespace ABC_Retailers.Controllers
{
    //controller for managing orders 
    //i added a logger to monitor the web apps response to commands
    public class OrderController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueStorageService _queueStorageService;
        private readonly ILogger<OrderController> _logger;
        private readonly FunctionService _functionService;

        public OrderController(TableStorageService tableStorageService, QueueStorageService queueStorageService, ILogger<OrderController> logger, FunctionService functionService)
        {
            _tableStorageService = tableStorageService;
            _queueStorageService = queueStorageService;
            _logger = logger;
            _functionService = functionService;
        }

        //displays the new order page

        [HttpGet]
        public IActionResult Index()
        {
            return View("NewOrder");
        }
        //method for recieving the new order
        [HttpGet]
        public IActionResult NewOrder(string productName, string description, double price)
        {
            var order = new OrderModel
            {
                ProductName = productName,
                ProductDescription = description,
                Price = price,
                Quantity = 1
            };
            _logger.LogInformation($"Product Description received: {order.ProductDescription}");

            return View(order);
        }
        //method for purchasing the product with error handling
        [HttpPost]
        public async Task<IActionResult> PurchaseProduct(string productName, int quantity)
        {
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Quantity must be greater than 0.";
                return RedirectToAction("Index");
            }

            _logger.LogInformation($"Attempting to purchase product: {productName}");

            string rowKey;
            try
            {
                //get the GUID for the product name
                rowKey = await _tableStorageService.GetProductRowKeyByNameAsync(productName);

                if (rowKey == null)
                {
                    _logger.LogWarning($"Product '{productName}' not found.");
                    return View("Error", new ErrorViewModel { RequestId = "Product not available." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving product by name '{productName}': {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = "An error occurred while retrieving the product." });
            }

            ProductTable product;
            try
            {
                //retrieve the product using the GUID
                product = await _tableStorageService.GetProductAsync("Product", rowKey);

                if (product == null)
                {
                    _logger.LogWarning($"Product '{productName}' not found.");
                    return View("Error", new ErrorViewModel { RequestId = "Product not available." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving product with GUID '{rowKey}': {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = "An error occurred while retrieving the product." });
            }

            if (product.Availability < quantity)
            {
                _logger.LogWarning($"Product '{productName}' out of stock.");
        
                return View("Error", new ErrorViewModel { RequestId = $"Insufficient stock. Only {product.Availability} available." });
               
            }

            try
            {
                //deduct the selected quantity from inventory
                product.Availability -= quantity;
                await _tableStorageService.UpdateProductAsync(product);

                var queueMessage = new QueueMessage
                {
                    ProductName = productName,
                    Quantity = quantity
                };

                //send a message to the queue
                string message = $"Transaction successful, Product: {productName} ordered. Quantity: {quantity}. Remaining stock: {product.Availability}.";
                await _functionService.SendMessageAsync(queueMessage);

                _logger.LogInformation($"Order for '{productName}' processed. Quantity: {quantity}. Remaining stock: {product.Availability}.");

               // redirect to the order success page after every successful order
                return View("OrderSuccess");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing order for product '{productName}': {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = "An error occurred while processing the order." });
            }
        }

        //update the product inventory after the order
        [HttpPost]
        public async Task<IActionResult> UpdateInventory(string productId, int quantity)
        {
            var message = $"Inventory updated: Product ID {productId}, New Quantity: {quantity}";
            await _queueStorageService.EnqueueMessageAsync(message);

            return RedirectToAction("Index");
        }

    }
}
