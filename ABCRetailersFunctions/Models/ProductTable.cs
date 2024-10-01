﻿using Azure.Data.Tables;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetailersFunctions.Models
{
    //product model contains the product variables
    //https://www.c-sharpcorner.com/article/azure-storage-crud-operations-in-mvc-using-c-sharp-azure-table-storage-part-one/ 
    public class ProductTable : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public double Price { get; set; }
        public int Availability { get; set; }
        public string ImageURL { get; set; }
        public ProductTable() { }

        public ProductTable(string rowKey, string name, string description, double price, int availability, string imageUrl)
        {
            PartitionKey = "Product";
            RowKey = rowKey;
            ProductName = name;
            ProductDescription = description;
            Price = price;
            Availability = availability;
            ImageURL = imageUrl;
        }

    }
}
