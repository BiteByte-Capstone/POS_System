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
        public int Id { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private double _price;
        public double Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public Item(int id, string name, double price, string description, string category)
        {
            Id = id;
            Name = name;
            Price = price;
            Description = description;
            Category = category;
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