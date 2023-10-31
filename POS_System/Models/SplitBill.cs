using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_System.Models
{
    public class SplitBill : OrderedItem
    {
        public int paymentId { get; set; }
        public string splitType { get; set; }
        public new int customerId { get; set; }  // 'new' keyword to hide the inherited member

        public string displayText => $"Customer #{customerId}";

        public SplitBill() { }

        public SplitBill(int paymentId, int orderId, int itemId, string itemName, int quantity, double price, int customerId, string splitType)
            : base(orderId, itemId, itemName, quantity, price, true)
        {
            this.paymentId = paymentId;
            this.splitType = splitType;
            this.customerId = customerId; // Explicitly setting customerId for SplitBill
        }
    }
}


