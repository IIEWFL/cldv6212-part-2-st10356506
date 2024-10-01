using Microsoft.AspNetCore.Mvc;
using ABC_Retailers.Services;
using System.Threading.Tasks;

namespace ABC_Retailers.Controllers
{
    //controller to manage the queues 
    //https://learn.microsoft.com/en-us/azure/storage/queues/storage-tutorial-queues 
    public class QueueController : Controller
    {
        private readonly QueueStorageService _queueStorageService;

        public QueueController(QueueStorageService queueStorageService)
        {
            _queueStorageService = queueStorageService;
        }

        //sends the queue message to azure 
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            await _queueStorageService.SendMessageAsync(message);
            return RedirectToAction("Index");
        }
        //recieves the message 
        public async Task<IActionResult> ReceiveMessage()
        {
            var message = await _queueStorageService.DequeueMessageAsync();
            return View(message);
        }
    }
}