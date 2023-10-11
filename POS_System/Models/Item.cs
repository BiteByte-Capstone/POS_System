using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS.Models
{
    public class Item : INotifyPropertyChanged
    {
        public int item_id { get;  set; }       
        public string item_description { get;  set; }
        public string item_category { get;  set; }
        private string _name;
        public string item_name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(item_name));
            }
        }

        private double _price;
        public double item_price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged(nameof(item_price));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public Item(int id, string name, double price, string description, string category)
        {
            item_id = id;
            item_name = name;
            item_price = price;
            item_description = description;
            item_category = category;
        }

        public Item() { }

        

        /*        //For future if we need to modify item
                public void UpdatePrice(double newPrice)
                {
                    // You can add any validation or additional logic here.
                    Price = newPrice;
                }*/
    }
}

