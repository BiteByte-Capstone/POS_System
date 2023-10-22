using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_System.Models
{
    public class OrderedItem
    {
        public OrderedItem() { }
        public OrderedItem(int order_id, string item_name, int Quantity, double ItemPrice)
        {
            this.order_id = order_id;
            this.item_name = item_name;
            this.Quantity = Quantity;
            this.ItemPrice = ItemPrice;
        }

        public int order_id { get; set; }  
        public int item_id { get; set; } // Add ItemId property

        public string item_name { get; set; }
        public int Quantity { get; set; }
        public double ItemPrice { get; set; }
    }
}

