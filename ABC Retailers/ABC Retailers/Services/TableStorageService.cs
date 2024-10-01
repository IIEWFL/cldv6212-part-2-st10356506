using Azure.Data.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABC_Retailers.Models;
using Azure;

namespace ABC_Retailers.Services
{
    //table storage service containing the methods for processing the customer and products table
    //https://www.c-sharpcorner.com/article/azure-storage-crud-operations-in-mvc-using-c-sharp-azure-table-storage-part-one/ 
    public class TableStorageService
    {
        private readonly TableClient _customerTableClient;
        private readonly TableClient _productTableClient;
        private readonly ILogger<TableStorageService> _logger;

        public TableStorageService(string storageConnectionString, ILogger<TableStorageService> logger)
        {
            var serviceClient = new TableServiceClient(storageConnectionString);
            _customerTableClient = serviceClient.GetTableClient("Customers");
            _productTableClient = serviceClient.GetTableClient("Products");

            _customerTableClient.CreateIfNotExists();
            _productTableClient.CreateIfNotExists();
            _logger = logger;
        }

        //method for getting the customers from the customer table
        public async Task<List<CustomerTable>> GetAllCustomersAsync()
        {
            var customers = new List<CustomerTable>();
            await foreach (var customer in _customerTableClient.QueryAsync<CustomerTable>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        //method for getting the products from the product table

        public async Task<List<ProductTable>> GetAllProductsAsync()
        {
            var products = new List<ProductTable>();
            await foreach (var product in _productTableClient.QueryAsync<ProductTable>())
            {
                products.Add(product);
            }
            return products;
        }

        //method for getting product partionkey and rowkey for the queues
        public async Task<ProductTable> GetProductAsync(string partitionKey, string rowKey)
        {
            _logger.LogInformation($"Fetching product PartitionKey: {partitionKey} and RowKey: {rowKey}");
            try
            {
                var response = await _productTableClient.GetEntityAsync<ProductTable>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "ResourceNotFound")
            {
                _logger.LogWarning($"Product with PartitionKey '{partitionKey}' and RowKey '{rowKey}' not found.");
                throw; //throw the exception to be handled by the caller
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"RequestFailedException in GetProductAsync: {ex.Message}");
                throw; //throw the exception to be handled by the caller
            }
        }

        //method for getting the product row key name
        public async Task<string> GetProductRowKeyByNameAsync(string productName)
        {
            try
            {
                string filter = TableClient.CreateQueryFilter($"ProductName eq {productName}");

                var results = _productTableClient.QueryAsync<ProductTable>(filter);
                await foreach (var product in results)
                {
                    return product.RowKey; //return the GUID for the product
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"RequestFailedException in GetProductRowKeyByNameAsync: {ex.Message}");
               //handling the errors using the logger
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in GetProductRowKeyByNameAsync: {ex.Message}");
               
            }

            return null; //if no product is found, return null
        }


        //method for adding customers to the customer table
        public async Task AddCustomers(CustomerTable customer)
        {
            await _customerTableClient.AddEntityAsync(customer);
        }

        //method for adding products to the product table
        public async Task AddProducts(ProductTable product)
        {
            await _productTableClient.AddEntityAsync(product);
        }
        //method to update a product
        public async Task UpdateProductAsync(ProductTable product)
        {
            try
            {
                await _productTableClient.UpdateEntityAsync(product, product.ETag, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Failed to update product. RowKey: {product.RowKey}, Error: {ex.Message}");
                throw;
            }
        }

    }
}
