using ABC_Retailers.Services;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);
//https://learn.microsoft.com/en-us/azure/azure-sql/database/connect-query-vscode?view=azuresql 
//read connection string from appsettings.json
var storageConnectionString = builder.Configuration.GetValue<string>("Azure:StorageConnectionString");

//adding services to the container.
builder.Services.AddControllersWithViews();

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

// Add Azure services 
//https://stackoverflow.com/questions/74066764/visual-studio-c-sharp-connecting-azure-blob-and-fileshare-storage 
//https://www.c-sharpcorner.com/article/azure-storage-crud-operations-in-mvc-using-c-sharp-azure-table-storage-part-one/ 
//singleton to only create containers once
builder.Services.AddSingleton(new TableStorageService(storageConnectionString, loggerFactory.CreateLogger<TableStorageService>()));
builder.Services.AddSingleton(new BlobStorage(storageConnectionString, "images"));  
builder.Services.AddHostedService<QueueBackgroundService>();
builder.Services.AddSingleton(new QueueStorageService(storageConnectionString, "log-messages", loggerFactory.CreateLogger<QueueStorageService>()));
builder.Services.AddSingleton(new FileStorageService(storageConnectionString, "abc-fileshare", "file-directory"));

//register function service 
builder.Services.AddHttpClient<FunctionService>();
builder.Services.AddSingleton<FunctionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
