using ABC_Retailers.Controllers;
using ABC_Retailers.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace ABC_Retailers.Services
{
    //bridge between mvc app and web app
    //https://stackoverflow.com/questions/63959928/host-an-asp-net-core-mvc-application-in-an-azure-function
    // https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-vs
    public class FunctionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _functionBaseUrl;
        private readonly ILogger<FunctionService> _logger;

        //https://www.youtube.com/watch?v=9HT9NzqYob0

        public FunctionService(HttpClient httpClient, IConfiguration configuration, ILogger<FunctionService> logger)
        {
            _httpClient = httpClient;
            _functionBaseUrl = configuration["AzureFunctionsBaseUrl"] ?? throw new InvalidOperationException("Azure Functions base URL is missing");
            _logger = logger;
        }

        // Blob storage function
        public async Task<string> UploadBlobAsync(Stream blobStream, string blobName)
        {
            var content = new StreamContent(blobStream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/abc-blobs?name={blobName}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to upload blob: {response.ReasonPhrase}");
            }

       
            return await response.Content.ReadAsStringAsync();  
        }

        // File storage function
        public async Task UploadFileAsync(string fileName, Stream fileStream)
        {
            var httpClient = new HttpClient();
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", fileName);

            var response = await httpClient.PostAsync("http://localhost:7024/api/abc-files", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to upload file: {response.ReasonPhrase}");
            }
        }


        // Queue storage function
        public async Task SendMessageAsync(QueueMessage queueMessage)
        {
            // Check if the queue message is null
            if (queueMessage == null)
            {
                throw new ArgumentNullException(nameof(queueMessage), "Queue message cannot be null.");
            }

            // Send the queue message to your Azure Function 
            var response = await _httpClient.PostAsJsonAsync($"{_functionBaseUrl}/api/abc-queues", queueMessage);

            // Ensure the response indicates success
            if (!response.IsSuccessStatusCode)
            {
                // Log the error message from the response
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error sending message: {response.StatusCode} - {errorMessage}");
            }

            // Log success message
            _logger.LogInformation("Message sent to the queue successfully.");
        }


        // Table storage function
        public async Task<CustomerTable> AddCustomer(CustomerTable customer, IFormFile? image)
        {
            // Create form data content
            var content = new MultipartFormDataContent
            {
                { new StringContent(customer.Name ?? string.Empty), "Name" },
                { new StringContent(customer.Surname ?? string.Empty), "Surname" },
                { new StringContent(customer.Email ?? string.Empty), "Email" },
                { new StringContent(customer.Age.ToString()), "Age" }
            };

            // Add image file if provided
            if (image != null)
            {
                var imageContent = new StreamContent(image.OpenReadStream());
                content.Add(imageContent, "image", image.FileName);
            }

            // Send HTTP POST request to the Azure Function
            var response = await _httpClient.PostAsync($"{_functionBaseUrl}/api/abc-tables", content);

            // success check and return
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CustomerTable>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            // Log and throw an exception in case of failure
            _logger.LogError("Failed to add customer.");
            throw new Exception("Failed to add customer.");
        }
    }

}
