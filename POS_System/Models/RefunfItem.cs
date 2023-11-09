using System;

namespace POS_System.Models
{
    public class RefundItem
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
    }
}
