using Microsoft.AspNetCore.Mvc;
using ABC_Retailers.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Files.Shares.Models;

namespace ABC_Retailers.Controllers
{
    //https://stackoverflow.com/questions/29788540/how-to-share-files-between-multiple-asp-net-mvc-applications 
    //controller for passing files to the Azure File Share
    //https://stackoverflow.com/questions/29788540/how-to-share-files-between-multiple-asp-net-mvc-applications 
    public class FileController : Controller
    {
        private readonly FileStorageService _fileStorageService;
        private readonly FunctionService _functionService;

        public FileController(FileStorageService fileStorageService, FunctionService functionService)
        {
            _fileStorageService = fileStorageService;
            _functionService = functionService;
        }
        //https://www.geeksforgeeks.org/microsoft-azure/ 
        public async Task<IActionResult> Index()
        {
            //retrieve the list of files from the Azure File Share
            List<ShareFileItem> files = await _fileStorageService.ListFilesAsync();

            //convert ShareFileItem objects to a list of file names (strings)
            var fileNames = files.Select(file => file.Name).ToList();

            //pass the list of file names to the view
            return View(fileNames);
        }

        [HttpGet]
        public IActionResult NewFile()
        {
            return View();
        }

        //method for uploading a new file
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null)
            {
                using (var stream = file.OpenReadStream())
                {
                    await _functionService.UploadFileAsync(file.FileName, stream);
                }
            }
            //return to the file share index page after uploading 
            return RedirectToAction("Index");
        }

        //method for downloading the file from the web app
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var stream = await _fileStorageService.DownloadFileAsync(fileName);
            return File(stream, "application/octet-stream", fileName);
        }

        public async Task<IActionResult> DeleteFile(string fileName)
        {
            await _fileStorageService.DeleteFileAsync(fileName);
            return RedirectToAction("Index");
        }
    }
}
