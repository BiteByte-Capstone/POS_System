using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_System.Models
{
    public class OrderedItem
    {
        public int ItemId { get; set; } // Add ItemId property
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public double ItemPrice { get; set; }
    }
}

