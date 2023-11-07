using Org.BouncyCastle.Asn1.X509;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for PaymentWindow.xaml
    /// </summary>
    public partial class PaymentWindow : Window
    {
        private string _tableNumber;
        private string _orderType;
        private long _orderId;
        private string _status;
        private bool _hasUnpaidOrders = true;
        private int _numberOfBill;
        private string paymentMethod;
        private double totalItemPriceForCustomer;


        private ObservableCollection<OrderedItem> _orderedItems = new ObservableCollection<OrderedItem>();
        private ObservableCollection<OrderedItem> _customerOrderedItems = new ObservableCollection<OrderedItem>();
        public PaymentWindow()
        {
            InitializeComponent();
            
        }




        public PaymentWindow(ObservableCollection<OrderedItem> orderedItems, string tableNumber, string orderType, long orderId, string status, bool hasUnpaidOrders, int numberOfBill) : this()
        {
            _tableNumber = tableNumber;
            _orderedItems = orderedItems;
            _orderType = orderType;
            _orderId = orderId;
            _status = status;
            _hasUnpaidOrders = hasUnpaidOrders;
            _numberOfBill = numberOfBill;
            MessageBox.Show("Split number:" + _numberOfBill);
            ShowPaymentPageButton(_numberOfBill);
            
            
        }


        private void ShowPaymentPageButton(int numberOfBill)
        {
            DisplayCustomerButton_Panel.Children.Clear();
            int customerNumber = 1;
            int splitNumber = _numberOfBill;
            do
            {
                Button paymentPageButton = new Button();
                paymentPageButton.Content = "Customer#" + customerNumber;
                paymentPageButton.Tag = customerNumber;
                paymentPageButton.Click += paymentPageButton_Click;
                DisplayCustomerButton_Panel.Children.Add(paymentPageButton);

                customerNumber++;
                splitNumber--; 
            }
            while (splitNumber > 0);
        }

        private void paymentPageButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (sender is Button button)
            {
                _customerOrderedItems.Clear();
                CustomerNumberDisplay_TextBlock.Text = button.Content.ToString();
                string customerID = button.Tag.ToString();
                PaymentPageFrame.Navigate(LoadCustomerPaymentPage(customerID));

            }
        }


        //testing return payment page
        private PaymentPage LoadCustomerPaymentPage(string customerID)
        {
            PaymentPage PaymentBaseOnCustomerID = new PaymentPage();
            int customerNumber = int.Parse(customerID);

            foreach (var order in _orderedItems)
            {
                if (order.customerID == customerNumber)
                {

                    OrderedItem ForEachCustomer = new OrderedItem()
                    {
                        order_id = order.customerID,
                        item_id = order.item_id,
                        item_name = order.item_name,
                        Quantity = order.Quantity,
                        ItemPrice = order.ItemPrice,
                        origialItemPrice = order.origialItemPrice,
                        IsExistItem = true,
                        customerID = order.customerID


                    };
                    _customerOrderedItems.Add(ForEachCustomer);
                         
                    
                    PaymentBaseOnCustomerID= new PaymentPage(_customerOrderedItems, _tableNumber, _orderType, _orderId, _status, false, customerNumber);
                }
                else if (order.customerID == 0)
                {
                    PaymentBaseOnCustomerID = new PaymentPage(_orderedItems, _tableNumber, _orderType, _orderId, _status, false, customerNumber);

                }


            }
            return PaymentBaseOnCustomerID;
        }
    }
}
