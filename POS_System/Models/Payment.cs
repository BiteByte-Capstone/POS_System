using System;
using System.ComponentModel;

namespace POS_System.Models
{
    public class Payment : INotifyPropertyChanged
    {
        private int _numberOfCompletedPayment;
        private int _customerID;
        private int _paymentID;
        private long _orderID;
        private string _orderType;
        private string _paymentMethod;
        private double _baseAmount;
        private double _GST;
        private double _customerPaymentTotalAmount;
        private double _grossAmount;
        private double _customerChangeAmount;
        private double _tip;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int numberOfCompletedPayment
        {
            get => _numberOfCompletedPayment;
            set
            {
                if (_numberOfCompletedPayment != value)
                {
                    _numberOfCompletedPayment = value;
                    OnPropertyChanged(nameof(numberOfCompletedPayment));
                }
            }
        }


        public int customerID
        {
            get => _customerID;
            set
            {
                if (_customerID != value)
                {
                    _customerID = value;
                    OnPropertyChanged(nameof(customerID));
                }
            }
        }

        public int paymentID
        {
            get => _paymentID;
            set
            {
                if (_paymentID != value)
                {
                    _paymentID = value;
                    OnPropertyChanged(nameof(paymentID));
                }
            }
        }

        public long orderID
        {
            get => _orderID;
            set
            {
                if (_orderID != value)
                {
                    _orderID = value;
                    OnPropertyChanged(nameof(orderID));
                }
            }
        }

        public string orderType
        {
            get => _orderType;
            set
            {
                if (_orderType != value)
                {
                    _orderType = value;
                    OnPropertyChanged(nameof(orderType));
                }
            }
        }

        public string paymentMethod
        {
            get => _paymentMethod;
            set
            {
                if (_paymentMethod != value)
                {
                    _paymentMethod = value;
                    OnPropertyChanged(nameof(paymentMethod));
                }
            }
        }

        public double baseAmount
        {
            get => _baseAmount;
            set
            {
                if (_baseAmount != value)
                {
                    _baseAmount = value;
                    OnPropertyChanged(nameof(baseAmount));
                }
            }
        }

        public double GST
        {
            get => _GST;
            set
            {
                if (_GST != value)
                {
                    _GST = value;
                    OnPropertyChanged(nameof(GST));
                }
            }
        }

        public double customerPaymentTotalAmount
        {
            get => _customerPaymentTotalAmount;
            set
            {
                if (_customerPaymentTotalAmount != value)
                {
                    _customerPaymentTotalAmount = value;
                    OnPropertyChanged(nameof(customerPaymentTotalAmount));
                }
            }
        }

        public double grossAmount
        {
            get => _grossAmount;
            set
            {
                if (_grossAmount != value)
                {
                    _grossAmount = value;
                    OnPropertyChanged(nameof(grossAmount));
                }
            }
        }

        public double customerChangeAmount
        {
            get => _customerChangeAmount;
            set
            {
                if (_customerChangeAmount != value)
                {
                    _customerChangeAmount = value;
                    OnPropertyChanged(nameof(customerChangeAmount));
                }
            }
        }

        public double tip
        {
            get => _tip;
            set
            {
                if (_tip != value)
                {
                    _tip = value;
                    OnPropertyChanged(nameof(tip));
                }
            }
        }



    }
}
