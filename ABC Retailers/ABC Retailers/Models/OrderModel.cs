using Azure;
using System.Collections.Concurrent;

namespace ABC_Retailers.Models
{
    //order model contains the variables for the product order
    public class OrderModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

            public string? ProductName { get; set; }
            public string? ProductDescription { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }
            public OrderModel() { }

        public OrderModel(string rowKey, string name, string description, double price, int quantity)
        {
            PartitionKey = "Product";
            RowKey = rowKey;
            ProductName = name;
            ProductDescription = description;
            Price = price;
            Quantity = quantity;
        }

    }

}
