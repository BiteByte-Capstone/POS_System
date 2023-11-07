using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Concurrent;
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
        private string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        private string _tableNumber;
        private string _orderType;
        private long _orderId;
        private string _status;
        private bool _hasUnpaidOrders = true;
        private int _numberOfBill;
        private string paymentMethod;
        private double totalItemPriceForCustomer;
        PaymentPage paymentPage = new PaymentPage();
        


        private ObservableCollection<OrderedItem> _orderedItems = new ObservableCollection<OrderedItem>();
        private ObservableCollection<OrderedItem> _customerOrderedItems = new ObservableCollection<OrderedItem>();
        private List<Payment> _paymentListPaymentWindow { get; set; }
        private ConcurrentDictionary<int, Payment> _paymentList = PaymentPage._Payments;
        private int _settledPayment;

        public PaymentWindow()
        {
            InitializeComponent();
            _paymentListPaymentWindow = new List<Payment>();
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

        public PaymentWindow(List<Payment> paymentList, int settledPayment)
        {
            InitializeComponent();
            _paymentListPaymentWindow = paymentList;
            _settledPayment += settledPayment;
            if (settledPayment == _numberOfBill)
            {
                
                MessageBox.Show("from window and settledPAyment = " + settledPayment + " _numberOFBill = "+_numberOfBill);
            } else
            {
                _paymentListPaymentWindow.AddRange(paymentList);
            }

        }

        //Method for show how many button on top based on number of bills.
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

        
        
        private void PaymentPage_PaymentCompleted()
        {
            // Code to change the button goes here
            // For example, disable the button:
            // yourButton.IsEnabled = false;
        }


        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                List<Payment> paymentList = _paymentList.Values.ToList();
                SavePaymentToDatabase(_paymentList);
            }
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
                         
                    
                    PaymentBaseOnCustomerID= new PaymentPage(_customerOrderedItems, _tableNumber, _orderType, _orderId, _status, false, customerNumber, _numberOfBill);
                }
                else if (order.customerID == 0)
                {
                    PaymentBaseOnCustomerID = new PaymentPage(_orderedItems, _tableNumber, _orderType, _orderId, _status, false, customerNumber, _numberOfBill);

                }


            }
            return PaymentBaseOnCustomerID;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SavePaymentToDatabase(ConcurrentDictionary<int, Payment> paymentDictionary)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Begin a transaction to ensure all inserts are treated as a single unit of work
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        foreach (KeyValuePair<int, Payment> kvp in paymentDictionary)
                        {
                            Payment payment = kvp.Value;
                            string paymentSql = "INSERT INTO `payment` " +
                            "(order_id, order_type, payment_method, base_amount, GST, total_amount, gross_amount, customer_change_amount, tip, payment_timestamp)" +
                            "VALUES (@order_id, @order_type, @payment_method, @base_amount, @GST, @total_amount, @gross_amount, @customer_change_amount, @tip, @payment_timestamp);";

                            MySqlCommand paymentCmd = new MySqlCommand(paymentSql, conn)
                            {
                                Transaction = transaction // Assign the transaction
                            };

                            paymentCmd.Parameters.AddWithValue("@order_id", payment.orderID);
                            paymentCmd.Parameters.AddWithValue("@order_type", payment.orderType);
                            paymentCmd.Parameters.AddWithValue("@payment_method", payment.paymentMethod);
                            paymentCmd.Parameters.AddWithValue("@base_amount", payment.baseAmount);
                            paymentCmd.Parameters.AddWithValue("@GST", payment.GST);
                            paymentCmd.Parameters.AddWithValue("@total_amount", payment.totalAmount);
                            paymentCmd.Parameters.AddWithValue("@gross_amount", payment.grossAmount);
                            paymentCmd.Parameters.AddWithValue("@customer_change_amount", payment.customerChangeAmount);
                            paymentCmd.Parameters.AddWithValue("@tip", payment.tip);
                            paymentCmd.Parameters.AddWithValue("@payment_timestamp", DateTime.Now);

                            paymentCmd.ExecuteNonQuery();
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }

                    MessageBox.Show("Payments sent successfully!");
                    string removeOrderedItemlistSql = "DELETE FROM ordered_itemlist WHERE order_id = @orderId;";
                    MySqlCommand removeOrderCmd = new MySqlCommand(removeOrderedItemlistSql, conn);
                    removeOrderCmd.Parameters.AddWithValue("@orderId", _orderId);
                    removeOrderCmd.ExecuteNonQuery();

                    string isPaidSql = "UPDATE `order` SET paid = @paid WHERE order_id = @orderId; ";
                    MySqlCommand isPaidCmd = new MySqlCommand(isPaidSql, conn);
                    isPaidCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                    isPaidCmd.Parameters.AddWithValue("@paid", "y");
                    isPaidCmd.Parameters.AddWithValue("@orderId", _orderId);
                    isPaidCmd.ExecuteNonQuery();
                    // You may want to clear the dictionary after successful save
                    paymentDictionary.Clear();

                    // Additional logic to remove ordered items and update order status...
                    // Be sure to wrap these in the same transaction if they need to be atomic with the payment inserts

                    // ... rest of the code ...

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                    // Rollback the transaction on error
                    // transaction.Rollback(); (Add this inside a try-catch if within the try block)
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending order: " + ex.ToString());
                    // Rollback the transaction on error
                    // transaction.Rollback(); (Add this inside a try-catch if within the try block)
                }
            }
        }


/*        //(method for send payment to database) back up!!!!
        private void SavePaymentToDatabase(ConcurrentDictionary<int, Payment> paymentDictionary)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {

                    }
                        foreach (Payment payment in _paymentList)
                    {
                        string paymentSql = "INSERT INTO `payment` " +
                        "(order_id, order_type, payment_method, base_amount, GST, total_amount, gross_amount, customer_change_amount, tip, payment_timestamp)" +
                        "VALUES (@order_id, @order_type,@payment_method, @base_amount, @GST, @total_amount, @gross_amount, @customer_change_amount, @tip, @payment_timestamp);";

                        MySqlCommand paymentCmd = new MySqlCommand(paymentSql, conn);



                        paymentCmd.Parameters.AddWithValue("@order_id", payment.orderID);
                        paymentCmd.Parameters.AddWithValue("@order_type", payment.orderType);
                        paymentCmd.Parameters.AddWithValue("@payment_method", payment.paymentMethod);
                        paymentCmd.Parameters.AddWithValue("@base_amount", payment.baseAmount);
                        paymentCmd.Parameters.AddWithValue("@GST", payment.GST);
                        paymentCmd.Parameters.AddWithValue("@total_amount", payment.totalAmount);
                        paymentCmd.Parameters.AddWithValue("@gross_amount", payment.grossAmount);
                        paymentCmd.Parameters.AddWithValue("@customer_change_amount", payment.customerChangeAmount);
                        paymentCmd.Parameters.AddWithValue("@tip", payment.tip);
                        paymentCmd.Parameters.AddWithValue("@payment_timestamp", DateTime.Now);

                        paymentCmd.ExecuteNonQuery();
                    }


                    MessageBox.Show("Payment sent successfully!");

                    string removeOrderedItemlistSql = "DELETE FROM ordered_itemlist WHERE order_id = @orderId;";
                    MySqlCommand removeOrderCmd = new MySqlCommand(removeOrderedItemlistSql, conn);
                    removeOrderCmd.Parameters.AddWithValue("@orderId", _orderId);
                    removeOrderCmd.ExecuteNonQuery();

                    string isPaidSql = "UPDATE `order` SET paid = @paid WHERE order_id = @orderId; ";
                    MySqlCommand isPaidCmd = new MySqlCommand(isPaidSql, conn);
                    isPaidCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                    isPaidCmd.Parameters.AddWithValue("@paid", "y");
                    isPaidCmd.Parameters.AddWithValue("@orderId", _orderId);
                    isPaidCmd.ExecuteNonQuery();




                    TablePage tablePage = new TablePage();
                    tablePage.Show();

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error sending order: " + ex.ToString());
                }
            }

        }*/


    }
}
