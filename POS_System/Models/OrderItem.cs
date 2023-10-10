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

        public int order_id { get; set; }  
        public int item_id { get; set; } // Add ItemId property
        public int Quantity { get; set; }
        public double ItemPrice { get; set; }
    }
}

