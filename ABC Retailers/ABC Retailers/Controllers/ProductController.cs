using Microsoft.AspNetCore.Mvc;
using ABC_Retailers.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using ABC_Retailers.Models;
using System.IO;
using System.Collections.Concurrent;
using Azure.Storage.Queues;

namespace ABC_Retailers.Controllers
{
    //controller to add new products to the product table and blob storage
    //https://stackoverflow.com/questions/60617190/how-do-i-store-a-picture-in-azure-blob-storage-in-asp-net-mvc-application 
    public class ProductController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorage _blobStorage;
        private readonly QueueStorageService _queueStorageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(TableStorageService tableStorageService, BlobStorage blobStorage, QueueStorageService queueStorageService, ILogger<ProductController> logger)
        {
            _tableStorageService = tableStorageService;
            _blobStorage = blobStorage;
            _queueStorageService = queueStorageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            //await the async method to get the actual list of products
            IEnumerable<ProductTable> products = await _tableStorageService.GetAllProductsAsync();
            return View(products); //pass the list of products to the view
        }

        //this will display the form to add a new product
        [HttpGet]
        public IActionResult NewProduct()
        {
            return View();
        }
        //adds the new product to the web app and the storage account
        [HttpPost]
        public async Task<IActionResult> AddProduct(string name, string description, double price, int availability, IFormFile image)
        {
            string imageURL = null;
            if (image != null)
            {
                var blobName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                using (var stream = image.OpenReadStream())
                {
                    imageURL = await _blobStorage.UploadBlobAsync(blobName, stream);
                }
            }
            var product = new ProductTable(Guid.NewGuid().ToString(), name, description, price, availability, imageURL);

            await _tableStorageService.AddProducts(product);
            var message = $"Uploaded image: {image.FileName}";

            await _queueStorageService.EnqueueMessageAsync(message);

            //redirect to product index after product is added
            return RedirectToAction("Index");
        }

    }
}