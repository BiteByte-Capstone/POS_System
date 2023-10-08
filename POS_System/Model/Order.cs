using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_System.Model
{
    [Table("order")]
    public class Order
    {

        [System.ComponentModel.DataAnnotations.Key]
        public int orderID { get; set; }
        public int tableNumber { get; set; }
        public DateTime orderTimeStamp { get; set; }

        public Decimal totalAmount { get; set; }
    }
}
