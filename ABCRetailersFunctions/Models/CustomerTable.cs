using Azure.Data.Tables;
using Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetailersFunctions.Models
{
    //variables for table storage 
    public class CustomerTable : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string ImageURL { get; set; }

        public CustomerTable() { }

        public CustomerTable(string partitionKey, string name, string surname, string email, int age, string imageURL)
        {
            PartitionKey = partitionKey;
            RowKey = Guid.NewGuid().ToString();
            Name = name;
            Surname = surname;
            Email = email;
            Age = age;
            ImageURL = imageURL;
        }
    }
}
