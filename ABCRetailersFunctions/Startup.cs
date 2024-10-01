using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ABCRetailersFunctions.Services;
using System;
using Azure.Data.Tables;
using Azure.Storage.Queues;


[assembly: FunctionsStartup(typeof(ABCRetailersFunctions.Startup))]

namespace ABCRetailersFunctions
{
    //https://www.youtube.com/watch?v=m_jrALXcrXc
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(sp =>
            CreateStorageService<TableClient>(sp, "new-customer", "table"));
            //Register BlobStorageService
            builder.Services.AddSingleton(sp =>
                CreateStorageService<BlobStorage>(sp, "new-images", "blob"));

            //Register QueueStorageService
            builder.Services.AddSingleton(sp =>
                CreateStorageService<QueueStorageService>(sp, "log-messages", "queue"));
            //Register FileShareService
            builder.Services.AddSingleton(sp =>
                CreateStorageService<FileStorageService>(sp, "abc-files", "fileshare"));

            builder.Services.AddHostedService<QueueBackgroundService>();

        }
        private T CreateStorageService<T>(IServiceProvider sp, string serviceIdentifier, string serviceType) where T : class
        {
            var logger = sp.GetRequiredService<ILogger<Startup>>();
            var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrEmpty(storageConnectionString) || string.IsNullOrEmpty(serviceIdentifier))
            {
                logger.LogError($"{serviceType} connection string or identifier is null or empty.");
                throw new InvalidOperationException("Configuration is invalid");
            }
            logger.LogInformation($"Using {serviceType} identifier: {serviceIdentifier}");

            return serviceType switch
            {
                "table" => new TableStorageService(storageConnectionString, sp.GetRequiredService<ILogger<TableStorageService>>()) as T,
                "blob" => new BlobStorage(storageConnectionString, serviceIdentifier) as T,
                "queue" => new QueueStorageService(storageConnectionString, "abc-queues", sp.GetRequiredService<ILogger<QueueStorageService>>()) as T,
                "fileshare" => new FileStorageService(storageConnectionString, "abc-fileshare", "file-directory") as T,
                _ => throw new NotImplementedException($"{serviceType} is not supported.")
            };

        }
    }

}
