using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using POS_System.Models;

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for PaymentPage.xaml
    /// </summary>
    public partial class PaymentPage : Window
    {


        private ObservableCollection<OrderedItem> _orderedItems;
        private string _tableNumber;
        private string _orderType;
        private long _orderId;
        private string _status;
        private bool _hasUnpaidOrders=true;

        public PaymentPage()
        {
            InitializeComponent();

        }



        public PaymentPage(ObservableCollection<OrderedItem> orderedItems, string tableNumber, string orderType, long orderId, string status, bool hasUnpaidOrders) : this()
        {
            _tableNumber = tableNumber;
            _orderedItems = orderedItems;
            _orderType = orderType;
            _orderId = orderId;
            _status = status;
            _hasUnpaidOrders = hasUnpaidOrders;

            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            toalAmtTextBox.Text = CalculateTotalOrderAmount().ToString("C", cultureInfo);
            
            
        }

        private double CalculateTotalOrderAmount()
        {
            double totalAmount = 0;
            foreach (var item in _orderedItems)
            {
                totalAmount += item.ItemPrice;
            }
            return totalAmount;
        }

        private double CalculateTip(double totalOrderAmount)
        {
            double tip = 0;
            double customerPaidAmount = double.Parse(customerPayTextBox.Text);
            if (decimal.TryParse(customerPayTextBox.Text, out decimal amount))
            {
                // Successfully parsed the amount.
                // Now you can use the 'amount' variable.
                if (CalculateTotalOrderAmount() > 0)
                {
                   tip = customerPaidAmount - CalculateTotalOrderAmount();
                }
            }
            else
            {
                // The input was not a valid decimal number.
                MessageBox.Show("Please enter a valid amount.");
            }

 
            return tip;
        }




        private void CancelButton(object sender, RoutedEventArgs e)
        {
            MenuPage menuPage = new MenuPage(_tableNumber, _orderType, _status, true);
            menuPage.Show();
            this.Close();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            
            tipsTextbox.Text = CalculateTip(CalculateTotalOrderAmount()).ToString();
        }
    }
}
