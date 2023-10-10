using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_System.Models
{
    public class Order : INotifyPropertyChanged
    {
        private bool _paid;

        public int Id { get; set; }
        public int tableNumber { get; set; }
        public DateTime timeStamp { get; set; }
        public double price { get; set; }



        public string IsPaid
        {
            get => _paid ? "y" : "n";
            set
            {
                bool newValue = value == "y";
                if (_paid != newValue)
                {
                    _paid = newValue;
                }
            }
        }

        // This property provides a more intuitive way to check payment status in code
        public bool Paid
        {
            get => _paid;
            set => _paid = value;
        }

        public Order() { }


        public event PropertyChangedEventHandler? PropertyChanged;



    }
}
