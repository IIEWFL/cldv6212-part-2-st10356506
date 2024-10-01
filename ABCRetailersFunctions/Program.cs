using ABCRetailersFunctions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        // Register services with required parameters
        services.AddSingleton(sp => new TableStorageService(storageConnectionString, sp.GetRequiredService<ILogger<TableStorageService>>()));
        services.AddSingleton(sp => new BlobStorage(storageConnectionString, "new-images"));
        services.AddSingleton(sp => new QueueStorageService(storageConnectionString, "log-messages", sp.GetRequiredService<ILogger<QueueStorageService>>()));
        services.AddSingleton(sp => new FileStorageService(storageConnectionString, "abc-fileshare", "file-directory"));


    })
    .Build();

host.Run();
