using Azure.Data.Tables;
using Azure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABCRetailersFunctions.Models;

namespace ABCRetailersFunctions.Services
{
    //service to store to tables 
    //https://learn.microsoft.com/en-us/visualstudio/azure/vs-azure-tools-connected-services-storage?view=vs-2022#:~:text=Open%20your%20project%20in%20Visual,Dependency%20page%2C%20select%20Azure%20Storage. 
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
            if (string.IsNullOrEmpty(customer.PartitionKey))
            {
                customer.PartitionKey = Guid.NewGuid().ToString();
            }
            if (string.IsNullOrEmpty(customer.RowKey))
            {
                customer.RowKey = Guid.NewGuid().ToString();
            }

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
