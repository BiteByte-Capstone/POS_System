using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using POS.Models;
using POS_System.Models;

namespace POS_System.Pages
{
    public partial class PaymentPage : Page
    {
        private ObservableCollection<OrderedItem> _orderedItems = new ObservableCollection<OrderedItem>();
        public static ConcurrentDictionary<int, Payment> _eachPaymentDictionary { get; } = new ConcurrentDictionary<int, Payment>();

        private string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        //getter change later!!!!!
        private string _tableNumber;
        private string _orderType;
        private long _orderId;
        private string _status;
        private int _customerID;
        private int _numberOfBill;
        private bool _hasUnpaidOrders = true;
        private int settledPayment;
        private PaymentWindow _parentWindow;
        private MenuPage _menuPage;
        private string _paymentMethod;



        // Define the event based on the delegate
        public event EventHandler PaymentCompleted;

        public PaymentPage()
        {
            InitializeComponent();
            changeTextBox.Text = "0.0";
            tipsTextbox.Text = "0.0";
            customerPayTextBox.Focus();
        }



        public PaymentPage(MenuPage menuPage,PaymentWindow parentWindow, ObservableCollection<OrderedItem> orderedItems, string tableNumber, string orderType, long orderId, string status, bool hasUnpaidOrders, int customerID, int numberOfBill) : this()
        {
            _tableNumber = tableNumber;
            _orderedItems = orderedItems;
            _orderType = orderType;
            _orderId = orderId;
            _status = status;
            _hasUnpaidOrders = hasUnpaidOrders;
            _customerID = customerID;
            _numberOfBill = numberOfBill;
            _parentWindow = parentWindow;
            _menuPage = menuPage;

            tableNumTextbox.Text = _tableNumber;
            orderIdTextbox.Text = _orderId.ToString();
            typeTextBox.Text = _orderType.ToString();

            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            totalAmtTextBox.Text = CalculateTotalOrderAmount().ToString("C", cultureInfo);

            foreach (OrderedItem per_customer_payment in _orderedItems)
            {
                string message = $"Order ID: {per_customer_payment.order_id}\n" +
                                 $"Item ID: {per_customer_payment.item_id}\n" +
                                 $"Item Name: {per_customer_payment.item_name}\n" +
                                 $"Quantity: {per_customer_payment.Quantity}\n" +
                                 $"Item Price: {per_customer_payment.ItemPrice:C}\n" +  // Display as currency
                                 $"Is Existing Item: {per_customer_payment.IsExistItem}\n" +
                                 $"Customer ID: {per_customer_payment.customerID}";

                MessageBox.Show(message);
            }


            DisplayBalance();
            DisplayTax();

            


        }


        // Method to raise the event
        protected virtual void OnPaymentCompleted()
        {
            PaymentCompleted?.Invoke(this, EventArgs.Empty);
        }

    


    // Add a private field to store the shadow value:
    private long shadowValue = 0;

        private void CustomerPayTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
                return;
            }

            long newShadowValue = shadowValue * 10 + int.Parse(e.Text);
            if (newShadowValue < 10000000) // This limits the maximum input to $999.99
            {
                shadowValue = newShadowValue;
                customerPayTextBox.Text = "$" + (shadowValue / 100.0).ToString("0.00");
                customerPayTextBox.CaretIndex = customerPayTextBox.Text.Length; // Set cursor to the end
            }
            e.Handled = true;
        }

        private void CustomerPayTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && shadowValue > 0)
            {
                shadowValue /= 10;
                customerPayTextBox.Text = "$" + (shadowValue / 100.0).ToString("0.00");
                customerPayTextBox.CaretIndex = customerPayTextBox.Text.Length; // Set cursor to the end
                e.Handled = true;
            }
        }

        private void CustomerPayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(customerPayTextBox.Text))
            {
                double value;
                if (double.TryParse(customerPayTextBox.Text, out value))
                {
                    customerPayTextBox.TextChanged -= CustomerPayTextBox_TextChanged;
                    customerPayTextBox.Text = value.ToString("F2");
                    customerPayTextBox.TextChanged += CustomerPayTextBox_TextChanged;
                }
            }
            
            DisplayTips();
        }



        //Calculate Total Order amount 
        private double CalculateTotalOrderAmount()
        {
            double totalAmount = 0;
            foreach (var orderedItem in _orderedItems)
            {
/*                if (orderedItem.IsExistItem == true)   //// not sure why i add it in origial
                {*/
                    totalAmount += orderedItem.ItemPrice;
/*                }*/
                
            }
            return totalAmount;
        }


        //Button Session
        //Save button (send data to payment database and reset table) 
        private void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            string message = $"orderID: {_orderId}" +
                 $"\npayment method: {_paymentMethod}" +
                 $"\ntotal order amount: {CalculateTotalOrderAmount()}" +
                 $"\nGST: {CalculateTaxAmount()}" +
                 $"\ntotal customer payment: {GetCustomerPayment()}" +
                 $"\ntotal order balance: {CalculateOrderTotalBalance()}" +
                 $"\ncustomer change amount: {CalculateChangeAmount()}" +
                 $"\ntip: {CalculateTipAmount()}";

            MessageBoxResult result = MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);


            if (result == MessageBoxResult.Yes)
            {

                
               
                if (_numberOfBill == 0)
                {
                    AddPaymentList();
                    SavePaymentToDatabase(_eachPaymentDictionary);
                    _parentWindow.Close();
                    _menuPage.Close();

                    //show the table page but not working 
                    TablePage tablePage = new TablePage();
                    tablePage.ShowDialog();
                }
                else if (_numberOfBill>0)
                {
                    MessageBox.Show($"ok more thean one bill and this is for customer number {_customerID}");
                    AddPaymentList();
                    OnPaymentCompleted();

                }






            }
            else
            {
                return;
            }
        }

        //(method for add the payment to list)
        private void AddPaymentList()
        {
            
            Payment newPayment = new Payment
                {
                    customerID =+ _customerID,
                    paymentID = _customerID,
                    orderID = _orderId,
                    orderType = _orderType,
                    paymentMethod = _paymentMethod,
                    baseAmount = CalculateTotalOrderAmount(),
                    GST = CalculateTaxAmount(),
                    customerPaymentTotalAmount = GetCustomerPayment(),
                    grossAmount = CalculateOrderTotalBalance(),
                    customerChangeAmount = CalculateChangeAmount(),
                    tip = CalculateTipAmount()
                };

            // Add the new Payment to the list
            _eachPaymentDictionary.TryAdd(_customerID, newPayment);



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
                            paymentCmd.Parameters.AddWithValue("@total_amount", payment.customerPaymentTotalAmount);
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





        //cash button (payment type = cash)
        private void cashBtn_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "Cash";
            cashBtn.Background = Brushes.White;
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));

        }

        //visa button (payment type = visa)
        private void visaBtn_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "Visa";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = Brushes.White;
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
        }

        //Master card button (payment type = MC)
        private void mcBtn_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "Mastercard";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = Brushes.White;
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            

        }

        //Amex button (payment type = amex)
        private void amexBtn_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "Amex";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = Brushes.White;
        }
        //***

        //**reading user input session
        //read values from customer payment textbox
        private double GetCustomerPayment()
        {
            if (string.IsNullOrWhiteSpace(customerPayTextBox.Text))
            {
                customerPayTextBox.Text = "0.0";
            }

            return double.Parse(customerPayTextBox.Text.Replace("$", "").Trim());
        }

        //***

        //**Calculation session
        //Calculate Tip
        private double CalculateTipAmount()
        {
            double tipAmount = 0.0;
            return tipAmount = GetCustomerPayment() - CalculateOrderTotalBalance();

        }

        //Calculate Change
        private double CalculateChangeAmount()
        {
            double changeAmount = 0.0;
            return changeAmount = GetCustomerPayment() - CalculateOrderTotalBalance();
        }

        //Calculate Tax
        private double CalculateTaxAmount()
        {
            double totalTaxAmount = 0;
            double totalOrderAmount = CalculateTotalOrderAmount();
            double taxRate = 0.05;
            return totalTaxAmount = totalOrderAmount * taxRate;


        }

        // Calculate Order Total Balance and show in the textbox
        private double CalculateOrderTotalBalance()
        {
            
            double totalOrderAmount = CalculateTotalOrderAmount();
            double totalTaxAmount = CalculateTaxAmount();
            return totalOrderAmount + totalTaxAmount;
        }



        //**Display session (grabbing all the calculation and display on page)
        //Display tips on tips text box
        private void DisplayTips()
        {
            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            
            tipsTextbox.Text = CalculateTipAmount().ToString("C", cultureInfo);
            if (string.IsNullOrWhiteSpace(tipsTextbox.Text))
            {
                tipsTextbox.Text = "0";
            }

        }

        //Display Balance
        private void DisplayBalance()
        {
            
            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            balanceTextBox.Text = CalculateOrderTotalBalance().ToString("C", cultureInfo);
        }

        //Display Tax in textblock
        private void DisplayTax()
        {
            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            totalTaxTextBox.Text = CalculateTaxAmount().ToString("C", cultureInfo);
        }

        //Display change amount if cash
        private void DisplayChange()
        {
            changeTextBox.Text = CalculateChangeAmount().ToString();
            if (string.IsNullOrWhiteSpace(changeTextBox.Text))
            {
                changeTextBox.Text = "0";
            }
        }


        //***






    }
}
