using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_System.Models
{
    public class Payment
    {
        public int paymentID { get; set; }
        public int orderID { get; set; }
        public int orderType { get; set; }
        public int paymentMethod {  get; set; }
        public double baseAmount { get; set; }
        public double GST {  get; set; }
        public double totalAmount { get; set; }
        public int grossAmount { get; set; }
        public double customerChangeAmount {  get; set; }
        public double tip {  get; set; }


        public Payment() { }

        public Payment(int paymentID, int orderID, int orderType, int paymentMethod, double baseAmount, double gst, double totalAmount, int grossAmount, double customerChangeAmount, double tip)
        {
            this.paymentID = paymentID;
            this.orderID = orderID;
            this.orderType = orderType;
            this.paymentMethod = paymentMethod;
            this.baseAmount = baseAmount;
            this.GST = gst;
            this.totalAmount = totalAmount;
            this.grossAmount = grossAmount;
            this.customerChangeAmount = customerChangeAmount;
            this.tip = tip;
        }
    }
}
