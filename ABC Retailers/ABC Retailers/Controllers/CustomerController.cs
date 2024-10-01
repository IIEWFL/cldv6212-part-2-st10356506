using Microsoft.AspNetCore.Mvc;
using ABC_Retailers.Models;
using ABC_Retailers.Services;
using System.Threading.Tasks;

namespace ABC_Retailers.Controllers
{
    //customer controller will control the crud operations for the customer table and image blob
    //https://youtu.be/vq3pzXv6kG0?si=v2eQ9YPgu1wucB0Y 
    public class CustomerController : Controller
    {
        //https://learn.microsoft.com/en-us/visualstudio/azure/vs-azure-tools-connected-services-storage?view=vs-2022#:~:text=Open%20your%20project%20in%20Visual,Dependency%20page%2C%20select%20Azure%20Storage. 
        //https://stackoverflow.com/questions/60617190/how-do-i-store-a-picture-in-azure-blob-storage-in-asp-net-mvc-application 

        private readonly TableStorageService _tableStorageService;
        private readonly BlobStorage _blobStorage;
        private readonly FunctionService _functionService;

        public CustomerController(TableStorageService tableStorageService, BlobStorage blobStorage, FunctionService functionService)
        {
            _tableStorageService = tableStorageService;
            _blobStorage = blobStorage;
            _functionService = functionService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<CustomerTable> customers = await _tableStorageService.GetAllCustomersAsync();
            return View(customers);
        }

        //this will display the form to add a new customer
        [HttpGet]
        public IActionResult NewCustomer()
        {
            return View();
        }

        // Method for adding a new customer 
        [HttpPost]
        public async Task<IActionResult> AddCustomer(CustomerTable customer, IFormFile image)
        {
            var newCustomer = await _functionService.AddCustomer(customer, image);

            return RedirectToAction("Index");
        }
    }
}