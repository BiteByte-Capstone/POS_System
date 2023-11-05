using System;
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
    public partial class PaymentPage : Window
    {
        private ObservableCollection<OrderedItem> _orderedItems;

        //getter change later!!!!!
        private string _tableNumber;
        private string _orderType;
        private long _orderId;
        private string _status;
        private bool _hasUnpaidOrders = true;
        private string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

        String paymentMethod;

        public PaymentPage()
        {
            InitializeComponent();
            changeTextBox.Text = "0.0";
            tipsTextbox.Text = "0.0";
            customerPayTextBox.Focus();
        }

        public PaymentPage(ObservableCollection<OrderedItem> orderedItems, string tableNumber, string orderType, long orderId, string status, bool hasUnpaidOrders) : this()
        {
            _tableNumber = tableNumber;
            _orderedItems = orderedItems;
            _orderType = orderType;
            _orderId = orderId;
            _status = status;
            _hasUnpaidOrders = hasUnpaidOrders;

            tableNumTextbox.Text = _tableNumber;
            orderIdTextbox.Text = _orderId.ToString();
            typeTextBox.Text = _orderType.ToString();

            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            totalAmtTextBox.Text = CalculateTotalOrderAmount().ToString("C", cultureInfo);
            DisplayBalance();
            DisplayTax();
            foreach (OrderedItem ordered in orderedItems)
            {
                string message = $"Order ID: {ordered.order_id}\n" +
                                 $"Item ID: {ordered.item_id}\n" +
                                 $"Item Name: {ordered.item_name}\n" +
                                 $"Quantity: {ordered.Quantity}\n" +
                                 $"Item Price: {ordered.ItemPrice:C}\n" +  // Display as currency
                                 $"Is Existing Item: {ordered.IsExistItem}" +
                                 $"Customer ID: {ordered.customerID}";

                MessageBox.Show(message);
            }
            InitializeEventHandlers();


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
            /*DisplayCustomerPayment();*/
            DisplayTips();
        }



        //Calculate Total Order amount 
        private double CalculateTotalOrderAmount()
        {
            double totalAmount = 0;
            foreach (var orderedItem in _orderedItems)
            {
                if (orderedItem.IsExistItem == true)
                {
                    totalAmount += orderedItem.ItemPrice;
                }
                
            }
            return totalAmount;
        }


        //Button Session
        //Save button (send data to payment database and reset table) 
        private void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            string message = $"orderID: {_orderId}" +
                 $"\npayment method: {paymentMethod}" +
                 $"\ntotal order amount: {CalculateTotalOrderAmount()}" +
                 $"\nGST: {CalculateTaxAmount()}" +
                 $"\ntotal customer payment: {GetCustomerPayment()}" +
                 $"\ntotal order balance: {CalculateOrderTotalBalance()}" +
                 $"\ncustomer change amount: {CalculateChangeAmount()}" +
                 $"\ntip: {CalculateTipAmount()}";

            MessageBoxResult result = MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);


            if (result == MessageBoxResult.Yes)
            {

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    string paymentSql = "INSERT INTO `payment` " +
                                        "(order_id, order_type, payment_method, base_amount, GST, total_amount, gross_amount, customer_change_amount, tip, payment_timestamp)" +
                                        "VALUES (@order_id, @order_type,@payment_method, @base_amount, @GST, @total_amount, @gross_amount, @customer_change_amount, @tip, @payment_timestamp);";

                    MySqlCommand paymentCmd = new MySqlCommand(paymentSql, conn);

                    paymentCmd.Parameters.AddWithValue("@order_id", _orderId);
                        paymentCmd.Parameters.AddWithValue("@order_type", _orderType);
                        paymentCmd.Parameters.AddWithValue("@payment_method", paymentMethod);
                    paymentCmd.Parameters.AddWithValue("@base_amount", CalculateTotalOrderAmount());
                    paymentCmd.Parameters.AddWithValue("@GST", CalculateTaxAmount());
                    paymentCmd.Parameters.AddWithValue("@total_amount", GetCustomerPayment());
                    paymentCmd.Parameters.AddWithValue("@gross_amount", CalculateOrderTotalBalance());
                    paymentCmd.Parameters.AddWithValue("@customer_change_amount", CalculateChangeAmount());
                    paymentCmd.Parameters.AddWithValue("@tip", CalculateTipAmount());
                    paymentCmd.Parameters.AddWithValue("@payment_timestamp", DateTime.Now);

                    paymentCmd.ExecuteNonQuery();

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
                    this.Close();
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
            }
            else
            {
                return;
            }
        }



        //Cancel button (back to menu page with existing order)
        private void CancelButton(object sender, RoutedEventArgs e)
        {
            MenuPage menuPage = new MenuPage(_tableNumber, _orderType, _status, true);
            menuPage.Show();
            this.Close();
        }

        //cash button (payment type = cash)
        private void cashBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "Cash";
            cashBtn.Background = Brushes.White;
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));

        }

        //visa button (payment type = visa)
        private void visaBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "Visa";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = Brushes.White;
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
        }

        //Master card button (payment type = MC)
        private void mcBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "Mastercard";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = Brushes.White;
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            

        }

        //Amex button (payment type = amex)
        private void amexBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "Amex";
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
            double totalBalance = 0;
            double totalOrderAmount = CalculateTotalOrderAmount();
            double totalTaxAmount = CalculateTaxAmount();
            return totalBalance = totalOrderAmount + totalTaxAmount;
        }


        //***

        //**tip and change will auto change based on customer pay textbox change.
        private void InitializeEventHandlers()
        {
            // Attach the event handler for the TextChanged event of the tipsTextbox
            /*tipsTextbox.TextChanged += TipsTextbox_TextChanged;*/
            customerPayTextBox.TextChanged += CustomerPayTextBox_TextChanged;
        }

/*        private void CustomerPayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(customerPayTextBox.Text))
            {
                customerPayTextBox.Text = "0";
            }
            DisplayCustomerPayment();
            DisplayTips();
            

        }*/
        //***

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
