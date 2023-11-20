using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using POS.Models;
using POS_System.Models;

namespace POS_System.Pages
{
    public partial class PaymentPage : Page
    {
        private ObservableCollection<OrderedItem> _eachCustomerOrderedItems = new ObservableCollection<OrderedItem>();

        private List<Payment> eachPaymentList = new List<Payment>();

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
        private int _numberOfCompletedPayment = 1;


        // Define the event based on the delegate
        public event EventHandler PaymentCompleted;

        public PaymentPage()
        {
            InitializeComponent();
            changeTextBox.Text = "0.0";
            tipsTextbox.Text = "0.0";
            customerPayTextBox.Focus();
        }



        public PaymentPage(MenuPage menuPage, PaymentWindow parentWindow, ObservableCollection<OrderedItem> orderedItems, string tableNumber, string orderType, long orderId, string status, bool hasUnpaidOrders, int customerID, int numberOfBill) : this()
        {
            _tableNumber = tableNumber;
            _eachCustomerOrderedItems = orderedItems;
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
            DisplayBalance();
            DisplayTax();

        }

        //Method for payment window: after save button click it will disable the customer payment button
        protected virtual void OnPaymentCompleted()
        {
            PaymentCompleted?.Invoke(this, EventArgs.Empty);
        }



        // Methods for get customer payment: when user type the amount, it will start from cent.

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


            if (_paymentMethod != null && _paymentMethod.Equals("Cash"))
            {
                tipsTextbox.Text = "0";
                DisplayChange();
            }
            else if (_paymentMethod == null || _paymentMethod != "Cash")
            {
                changeTextBox.Text = "0";
                DisplayTips();
            }

        }



        //Calculate Total Order amount 
        private double CalculateTotalOrderAmount()
        {
            double totalAmount = 0;
            foreach (var orderedItem in _eachCustomerOrderedItems)
            {

                totalAmount += orderedItem.ItemPrice;

            }
            return totalAmount;
        }


        //Button Session
        //Save button (send data to payment database and reset table) 
        private void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_paymentMethod != null)
            {
                if (GetCustomerPayment() >= CalculateOrderTotalBalance())
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
                        int key = _eachPaymentDictionary.Count;
                        int forConditionKey = key + 1;


                        if (key < _numberOfBill || _numberOfBill == 0)
                        {
                            MessageBox.Show($"Customer ID #{_customerID} payment saved.");
                            AddPaymentList();
                            OnPaymentCompleted();



                            if (forConditionKey.Equals(_numberOfBill) || _numberOfBill == 0)
                            {


                                SavePaymentToDatabase(_eachPaymentDictionary);
                                MessageBox.Show("All customers payment completed! Thank you");
                                TablePage tablePage = new TablePage();
                                tablePage.Show();
                                _parentWindow.Close();
                                _menuPage.Close();
                            }

                        }

                    }
                    else
                    {
                        return;
                    }

                }
                else
                {
                    MessageBox.Show($"The payment must be greater than the \n\nBalance : ${CalculateOrderTotalBalance()}");

                    return;
                }
            }

            else if (_paymentMethod == null)
            {
                MessageBox.Show("Please select payment type!");
                return;
            }
        }

        //(method for add the payment to list)
        private void AddPaymentList()
        {
            foreach (OrderedItem items in _eachCustomerOrderedItems)
            {
                Payment eachCustomerPayment = new Payment
                {

                    customerID = +_customerID, 
                    paymentID = _customerID,
                    orderID = _orderId,
                    orderType = _orderType,
                    tableNumber = _tableNumber,
                    paymentMethod = _paymentMethod,
                    baseAmount = CalculateTotalOrderAmount(),
                    GST = CalculateTaxAmount(),
                    customerPaymentTotalAmount = GetCustomerPayment(),
                    grossAmount = CalculateOrderTotalBalance(),
                    customerChangeAmount = CalculateChangeAmount(),
                    tip = CalculateTipAmount(),
                    eachCustomerItems = _eachCustomerOrderedItems


                };

                int newKey = _eachPaymentDictionary.Count + 1;
                _eachPaymentDictionary.TryAdd(newKey, eachCustomerPayment);
            }
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
                    PrintAllReceipts(paymentDictionary);

                    paymentDictionary.Clear();


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

        private void PrintAllReceipts(ConcurrentDictionary<int, Payment> paymentDictionary)
        {
            foreach (var kvp in paymentDictionary)
            {
                Payment eachCustomerPayment = kvp.Value;
                PrintSettledPaymentReceipt(eachCustomerPayment);
            }
        }

        private void PrintSettledPaymentReceipt(Payment eachCustomerPayment)
        {
            try
            {

                // Calculate GST (5% of TotalAmount)
                double gstRate = 0.05;  // GST rate as 5%

                // Create a FlowDocument for each split bill
                FlowDocument flowDocument = new FlowDocument();

                // Add the "-------------------------------------------------" separator at the top
                flowDocument.Blocks.Add(new Paragraph(new Run("-------------------------------------------------")));
                // Create a paragraph for restaurant information
                Paragraph restaurantInfoParagraph = new Paragraph();
                restaurantInfoParagraph.TextAlignment = TextAlignment.Center;

                // Restaurant Name
                Run restaurantNameRun = new Run("Thai Bistro\n");
                restaurantNameRun.FontSize = 20;
                restaurantInfoParagraph.Inlines.Add(restaurantNameRun);

                // Address
                Run addressRun = new Run("233 Centre St S #102,\n Calgary, AB T2G 2B7\n");
                addressRun.FontSize = 12;
                restaurantInfoParagraph.Inlines.Add(addressRun);

                // Phone
                Run phoneRun = new Run("Phone: (403) 313-9922\n");
                phoneRun.FontSize = 12;
                restaurantInfoParagraph.Inlines.Add(phoneRun);

                // Add the restaurant info paragraph
                flowDocument.Blocks.Add(restaurantInfoParagraph);

                // Add the "-------------------------------------------------" separator at the top
                flowDocument.Blocks.Add(new Paragraph(new Run("-------------------------------------------------")));

                ///^^^^^^good^^^^^^^^^^^



                ////////order detail session!!!!

                // Create a Section for the order details
                Section orderDetailsSection = new Section();

                // Table to display order details
                Table detailsTable = new Table();
                TableRowGroup detailTableRowGroup = new TableRowGroup();
                // Add rows for order details
                detailTableRowGroup.Rows.Add(CreateTableRow("Date:", DateTime.Now.ToString("MMMM/dd/yyyy hh:mm")));
                detailTableRowGroup.Rows.Add(CreateTableRow("Table:", eachCustomerPayment.tableNumber));
                detailTableRowGroup.Rows.Add(CreateTableRow("Order ID:", eachCustomerPayment.orderID.ToString()));
                detailTableRowGroup.Rows.Add(CreateTableRow("Server:", User.id.ToString()));

                // Add a line with dashes after "Server: John"
                TableRow dashedLineRow = new TableRow();
                TableCell dashedLineCell = new TableCell();

                Paragraph dashedLineParagraph = new Paragraph(new Run("-------------------------------------------------"));
                //dashedLineParagraph.TextAlignment = TextAlignment.Center;
                dashedLineCell.ColumnSpan = 2;
                dashedLineCell.Blocks.Add(dashedLineParagraph);
                dashedLineRow.Cells.Add(dashedLineCell);
                detailTableRowGroup.Rows.Add(dashedLineRow);

                ///item session

                // Create a TableRow for displaying items and their prices
                TableRowGroup itemTableRowGroup = new TableRowGroup();
                Section itemSection = new Section();
                // Add space (empty TableRow) for the gap
                itemTableRowGroup.Rows.Add(CreateEmptyTableRow());

                // Create a nested Table within the items cell
                Table itemsTable = new Table();

                // Access the 'Items' collection and loop through it to add item rows.
                foreach (var OrderedItem in eachCustomerPayment.eachCustomerItems)
                {
                    itemTableRowGroup.Rows.Add(CreateTableRow(OrderedItem.item_name, OrderedItem.ItemPrice.ToString("C")));
                }

                // Initialize the TableRowGroup
                itemSection.Blocks.Add(itemsTable);

                // Create a new TableRow for the itemsCell and add it to the tableRowGroup
                // Add space (empty TableRow) for the gap
                itemTableRowGroup.Rows.Add(CreateEmptyTableRow());
                /////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                ///////sub total session
                Table paymentTable = new Table();
                Section paymentSection = new Section();
                TableRowGroup paymentTableRowGroup = new TableRowGroup();


                // Create a Paragraph for "Sub Total" with underline
                Paragraph subTotalParagraph = new Paragraph(new Run("Sub Total:"));
                subTotalParagraph.FontSize = 20; // Increase the font size
                subTotalParagraph.TextAlignment = TextAlignment.Right;

                //double customerTotalAmount = orderedItems.Where(item => item.customerID == customerID).Sum(item => item.ItemPrice);
                double subTotalAmount = eachCustomerPayment.baseAmount;
                Paragraph subTotalValueParagraph = new Paragraph(new Run(subTotalAmount.ToString("C")));
                paymentTableRowGroup.Rows.Add(CreateTableRowWithParagraph(subTotalParagraph, subTotalValueParagraph));

                // Create a Paragraph for "GST"
                Paragraph gstLabelParagraph = new Paragraph(new Run("GST (5%):"));
                gstLabelParagraph.FontSize = 20; // Increase the font size
                gstLabelParagraph.TextAlignment = TextAlignment.Right;

                double customerGSTAmount = eachCustomerPayment.GST;
                Paragraph gstValueParagraph = new Paragraph(new Run(customerGSTAmount.ToString("C")));
                paymentTableRowGroup.Rows.Add(CreateTableRowWithParagraph(gstLabelParagraph, gstValueParagraph));

                // Create a Paragraph for "Total Amount"
                Paragraph totalDueAmountLabelParagraph = new Paragraph(new Run("Total Due:"));
                totalDueAmountLabelParagraph.FontSize = 20; // Increase the font size
                totalDueAmountLabelParagraph.TextAlignment = TextAlignment.Right;

                double totalDueAmountWithGST = eachCustomerPayment.grossAmount;
                Paragraph totalDueAmountValueParagraph = new Paragraph(new Run(totalDueAmountWithGST.ToString("C")));
                paymentTableRowGroup.Rows.Add(CreateTableRowWithParagraph(totalDueAmountLabelParagraph, totalDueAmountValueParagraph));
                //////////////////////////////////////////////////

                detailsTable.RowGroups.Add(detailTableRowGroup);
                itemsTable.RowGroups.Add(itemTableRowGroup);
                paymentTable.RowGroups.Add(paymentTableRowGroup);

                orderDetailsSection.Blocks.Add(detailsTable);
                itemSection.Blocks.Add(itemsTable);
                paymentSection.Blocks.Add(paymentTable);

                flowDocument.Blocks.Add(orderDetailsSection);
                flowDocument.Blocks.Add(itemSection);
                flowDocument.Blocks.Add(paymentSection);
                // /*****************************
                // Create a new paragraph for the "Thank You" message
                Paragraph thankYouParagraph = new Paragraph();
                thankYouParagraph.TextAlignment = TextAlignment.Center;
                thankYouParagraph.FontSize = 16; // You can set the font size as you wish
                thankYouParagraph.Inlines.Add(new Run("Thank You for dining with us!"));
                thankYouParagraph.Margin = new Thickness(0, 10, 0, 0); // Add some space before the message if needed

                // Add a "-------------------------------------------------" separator before the "Thank You" message
                flowDocument.Blocks.Add(new Paragraph(new Run("-------------------------------------------------")));

                // Add the "Thank You" paragraph to the FlowDocument
                flowDocument.Blocks.Add(thankYouParagraph);
                flowDocument.Blocks.Add(new Paragraph(new Run("-------------------------------------------------")));

                //**********************************

                // Create a DocumentPaginator for the FlowDocument
                IDocumentPaginatorSource paginatorSource = flowDocument;
                DocumentPaginator documentPaginator = paginatorSource.DocumentPaginator;

                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintDocument(documentPaginator, $"Settled Payment Receipt - Customer {eachCustomerPayment.customerID}");
                }

            }

            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("An error occurred: " + errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private TableRow CreateTableRow(string label, string value)
        {
            TableRow row = new TableRow();

            // Label cell
            TableCell labelCell = new TableCell(new Paragraph(new Run(label)));
            labelCell.TextAlignment = TextAlignment.Right;
            labelCell.BorderThickness = new Thickness(0, 0, 20, 0); // Add space on the right side
            labelCell.BorderBrush = Brushes.Transparent; // Set the border brush to transparent to hide the line
            row.Cells.Add(labelCell);

            // Value cell
            TableCell valueCell = new TableCell(new Paragraph(new Run(value)));
            valueCell.BorderThickness = new Thickness(0); // No column lines, only space
            row.Cells.Add(valueCell);

            return row;
        }



        private TableRow CreateEmptyTableRow()
        {
            TableRow row = new TableRow();

            TableCell emptyCell = new TableCell(new Paragraph(new Run(" "))); // Add a space or empty string
            emptyCell.ColumnSpan = 2; // Set the column span to cover both columns

            row.Cells.Add(emptyCell);

            return row;
        }

        private TableRow CreateTableRowWithParagraph(Paragraph labelParagraph, Paragraph valueParagraph)
        {
            TableRow row = new TableRow();

            // Label cell
            TableCell labelCell = new TableCell(labelParagraph);
            labelCell.TextAlignment = TextAlignment.Right;
            labelCell.BorderThickness = new Thickness(0, 0, 20, 0); // Add space on the right side
            labelCell.BorderBrush = Brushes.Transparent; // Set the border brush to transparent to hide the line
            row.Cells.Add(labelCell);

            // Value cell
            TableCell valueCell = new TableCell(valueParagraph);
            valueCell.BorderThickness = new Thickness(0); // No column lines, only space
            row.Cells.Add(valueCell);

            return row;
        }





        //cash button (payment type = cash)
        private void cashBtn_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "Cash";
            cashBtn.Background = Brushes.White;
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));

            //since initial amount is 0.0
            if (tipsTextbox.Text.Length > 3)
            {
                tipsTextbox.Text = "0.0";
                CultureInfo cultureInfo = new CultureInfo("en-CA");
                cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                changeTextBox.Text = CalculateChangeAmount().ToString("C", cultureInfo);

            }

        }

        //visa button (payment type = visa)
        private void visaBtn_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "Visa";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = Brushes.White;
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            if (changeTextBox.Text.Length > 3)
            {
                changeTextBox.Text = "0.0";
                CultureInfo cultureInfo = new CultureInfo("en-CA");
                cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                tipsTextbox.Text = CalculateTipAmount().ToString("C", cultureInfo);

            }
        }

        //Master card button (payment type = MC)
        private void mcBtn_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "Mastercard";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = Brushes.White;
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));

            if (changeTextBox.Text.Length > 3)
            {
                changeTextBox.Text = "0.0";
                CultureInfo cultureInfo = new CultureInfo("en-CA");
                cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                tipsTextbox.Text = CalculateTipAmount().ToString("C", cultureInfo);

            }
        }

        //Amex button (payment type = amex)
        private void amexBtn_Click(object sender, RoutedEventArgs e)
        {
            _paymentMethod = "Amex";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = Brushes.White;
            if (changeTextBox.Text.Length > 3)
            {
                changeTextBox.Text = "0.0";
                CultureInfo cultureInfo = new CultureInfo("en-CA");
                cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                tipsTextbox.Text = CalculateTipAmount().ToString("C", cultureInfo);

            }
        }
        //***

        //**reading user input session
        //read values from customer payment textbox
        private double GetCustomerPayment()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerPayTextBox.Text))
                {
                    customerPayTextBox.Text = "0.0";
                }
                return double.Parse(customerPayTextBox.Text.Replace("$", "").Trim());
            }
            catch (System.FormatException e)
            {
                customerPayTextBox.Text = "0.0";
            }

            return 0;


        }

        //***

        //**Calculation session
        //Calculate Tip
        private double CalculateTipAmount()
        {
            double tipAmount = 0.0;
            if (_paymentMethod != null && _paymentMethod.Equals("Cash"))
            {
                return 0.0;
            }
            else
            {
                return tipAmount = GetCustomerPayment() - CalculateOrderTotalBalance();
            }


        }

        //Calculate Change
        private double CalculateChangeAmount()
        {
            double changeAmount = 0.0;
            if (_paymentMethod != null && _paymentMethod.Equals("Cash"))
            {
                return changeAmount = GetCustomerPayment() - CalculateOrderTotalBalance();
            }
            else
            {
                return 0.0;
            }
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
            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            changeTextBox.Text = CalculateChangeAmount().ToString("C", cultureInfo);
            if (string.IsNullOrWhiteSpace(changeTextBox.Text))
            {

                changeTextBox.Text = "0";


            }
        }


        //***






    }
}
