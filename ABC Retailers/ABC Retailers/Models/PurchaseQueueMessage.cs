using System;

namespace ABC_Retailers.Models
{
    public class PurchaseQueueMessage
        //stores the variables for the queues 
    {
        public string ProductID { get; set; }
        public int Quantity { get; set; }
    }
}
