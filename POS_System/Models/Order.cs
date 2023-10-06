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

        public int Id { get; set; }
        public int tableNumber { get; set; }
        public DateTime timeStamp { get; set; }
        public double price { get; set; }

        private string databaseValue;


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

        public Order() { }


        public event PropertyChangedEventHandler? PropertyChanged;



    }
}
