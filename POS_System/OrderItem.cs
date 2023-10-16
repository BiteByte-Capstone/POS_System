using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_System.Models
{
    public class OrderedItem
    {
        public int OrderIdTextField { get; set; }
        public string TableNumberTextBox { get; set; }

        // Other properties related to the ordered item
        // public string ItemName { get; set; }
        //  public double ItemPrice { get; set; }
        //  public int Quantity { get; set; }

        public OrderedItem(int OrderIdTextField, string TableNumberTextBox)
        {
            OrderIdTextField = OrderIdTextField;
            TableNumberTextBox = TableNumberTextBox;
        }   
    }
}

