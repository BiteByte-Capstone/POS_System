using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_System.Models
{
    class Order
    {
        public int Id { get; set; }
        public int tableNumber { get; set; }
        public DateTime timeStamp { get; set; }
        public double price { get; set; }

private string databaseValue;  // field to store the 'n' or 'y' value

public bool IsPaid
{
    get
    {
        return databaseValue == "y";
    }

    set
    {
        databaseValue = value ? "y" : "n";
    }
}


    }
}
