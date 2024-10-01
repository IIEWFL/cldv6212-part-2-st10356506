using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetailersFunctions.Models
{
    public class PurchaseQueueMessage
    //stores the variables for the queues 
    {
        public string ProductID { get; set; }
        public int Quantity { get; set; }
    }
}
