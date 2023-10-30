using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Printing;
using System.Linq;
using System.Globalization;
using Org.BouncyCastle.Utilities.Collections;

namespace POS_System.Pages
{
    public partial class MenuPage : Window
    {
        // Define connStr at the class level
        public string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        //categories
        private ObservableCollection<Category> categories = new ObservableCollection<Category>();
        //new order
        private ObservableCollection<Item> items = new ObservableCollection<Item>();
        //existing order
        private ObservableCollection<OrderedItem> orderedItems = new ObservableCollection<OrderedItem>();

        private string _tableNumber;
        private string _orderType;
        private string _status;
        private bool _hasPaidOrders;

        private double TotalAmount = 0.0;
        private int existItemCount = 0;
        bool itemClick = false;
        //Constructor 
        public MenuPage()
        {
            InitializeComponent();

            //it could load the page before show up
            this.DataContext = this;
            this.Loaded += Window_Loaded; // Subscribe to the Loaded event
        }

        public MenuPage(string tableNumber, string orderType, string status, bool hasUnpaidOrders) : this()
        {
            TableNumberTextBox.Text = tableNumber;
            TypeTextBox.Text = orderType;
            StatusTextBlock.Text = status;

            _tableNumber = tableNumber;
            _orderType = orderType;
            _status = status;
            _hasPaidOrders = hasUnpaidOrders;

            if (hasUnpaidOrders)
            {
                LoadUnpaidOrders(tableNumber);
            }
        }



        private void LoadUnpaidOrders(string tableNumber)
        {

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    long orderId = GetOrderId(tableNumber);
                    string unpaidOrdersSql = "SELECT o.order_id, o.item_id, o.quantity, o.item_price, i.item_name, i.item_description FROM ordered_itemlist o JOIN item i ON o.item_id = i.item_id WHERE o.order_id = @orderId;";
                    MySqlCommand unpaidOrdersCmd = new MySqlCommand(unpaidOrdersSql, conn);
                    unpaidOrdersCmd.Parameters.AddWithValue("@orderId", orderId);
                    MySqlDataAdapter dataAdapter = new MySqlDataAdapter(unpaidOrdersCmd);
                    DataTable unpaidOrdersTable = new DataTable();
                    dataAdapter.Fill(unpaidOrdersTable);//!!!!!!! remove messageBox later
                    /*items.Clear();*/

                    if (unpaidOrdersTable.Rows.Count > 0)
                    {

                        OrderIdTextBlock.Text = orderId.ToString();
                    }
                    else if (unpaidOrdersTable.Rows.Count == 0)
                    {

                        StatusTextBlock.Text = "Deleted all saved order before";
                        OrderIdTextBlock.Text = orderId.ToString();
                    }

                    foreach (DataRow row in unpaidOrdersTable.Rows)
                    {
                        OrderedItem orderedItem = new OrderedItem
                        {
                            order_id = Convert.ToInt32(row["order_id"]),
                            item_id = Convert.ToInt32(row["item_id"]),
                            item_name = row["item_name"].ToString(),
                            Quantity = Convert.ToInt32(row["quantity"]),
                            ItemPrice = Convert.ToDouble(row["item_price"]),
                            IsExistItem = true
                        };
                        existItemCount++;
                        orderedItems.Add(orderedItem);
                        TotalAmount += orderedItem.ItemPrice;
                    }
                    TotalAmountTextBlock.Text = TotalAmount.ToString("C", new CultureInfo("en-CA"));
                    OrdersListBox.ItemsSource = orderedItems;



                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading unpaid orders: " + ex.ToString());
                }
            }
        }




        private void LoadCategoryData()
        {

            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
                string sql = "SELECT * FROM category;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Category category = new Category
                    {
                        Id = Convert.ToInt32(rdr["category_id"]),
                        Name = rdr["category_name"].ToString(),
                    };

                    categories.Add(category);

                    Button newCategoryButton = new Button();
                    newCategoryButton.Content = rdr["category_name"].ToString();
                    newCategoryButton.Tag = category;
                    newCategoryButton.Click += (sender, e) => LoadItemsByCategory(newCategoryButton.Content.ToString());
                    newCategoryButton.Width = 150;
                    newCategoryButton.Height = 30;
                    newCategoryButton.Margin = new Thickness(5);
                    SetButtonStyle(newCategoryButton);

                    CategoryButtonPanel.Children.Add(newCategoryButton);
                }

                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }

 

        private void LoadItemsByCategory(string categoryName)
        {
            ItemButtonPanel.Children.Clear();

            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
                string sql = "SELECT * FROM item WHERE item_category = @category;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@category", categoryName);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Item item = new Item
                    {
                        Id = Convert.ToInt32(rdr["item_id"]),
                        item_name = rdr["item_name"].ToString(),
                        ItemPrice = Convert.ToDouble(rdr["item_price"]),
                        Description = rdr["item_description"].ToString(),
                        Category = rdr["item_category"].ToString()
                    };

                    Button newItemButton = new Button();
                    newItemButton.Content = rdr["item_name"].ToString();
                    newItemButton.Tag = item;
                    newItemButton.Width = 150;
                    newItemButton.Height = 80;
                    SetButtonStyle(newItemButton);
                    newItemButton.Click += ItemClick;
                    ItemButtonPanel.Children.Add(newItemButton);
                }

                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }




        //add item on list box
        private void ItemClick(object sender, RoutedEventArgs e)
        {
            itemClick = true;
            
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is Item)
            {
                Item item = clickedButton.Tag as Item;

                if (item != null)
                {
                    if (StatusTextBlock.Text.Equals("New Order")) //orderedItems.Count == 0 && 
                    {
                        AddItemToOrder(item);
                        /*OrdersListBox.ItemsSource = orderedItems;*/
                        foreach (OrderedItem ordered in orderedItems)
                        {
                            string message = $"Order ID: {ordered.order_id}\n" +
                                             $"Item ID: {ordered.item_id}\n" +
                                             $"Item Name: {ordered.item_name}\n" +
                                             $"Quantity: {ordered.Quantity}\n" +
                                             $"Item Price: {ordered.ItemPrice:C}\n" +  // Display as currency
                                             $"Is Existing Item: {ordered.IsExistItem}";

                            MessageBox.Show(message);
                        }


                    }
                    else
                    {
                        //convert it into ordered list item
                        AddItemToOrder(item);
                        foreach (OrderedItem ordered in orderedItems)
                        {
                            string message = $"Order ID: {ordered.order_id}\n" +
                                             $"Item ID: {ordered.item_id}\n" +
                                             $"Item Name: {ordered.item_name}\n" +
                                             $"Quantity: {ordered.Quantity}\n" +
                                             $"Item Price: {ordered.ItemPrice:C}\n" +  // Display as currency
                                             $"Is Existing Item: {ordered.IsExistItem}";

                            MessageBox.Show(message);
                        }


                    }
                }
            }
        }

        //(edit item list) Add new item to the existing list
        private void AddItemToOrder(Item item)
        {
            // Convert Item to OrderedItem
            OrderedItem orderedItem = new OrderedItem
            {

                item_id = item.Id,
                item_name = item.item_name,
                Quantity = 1, // Assuming quantity of 1 for new items
                ItemPrice = item.ItemPrice,
                IsExistItem = false
            };

            orderedItems.Add(orderedItem);
            OrdersListBox.ItemsSource = orderedItems;
            TotalAmount += orderedItem.ItemPrice;
            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            TotalAmountTextBlock.Text = TotalAmount.ToString("C", cultureInfo);
        }

        //(button) go to payment page
        private void PaymentButton_Click(object sender, RoutedEventArgs e)
        {
            string tableNumber = TableNumberTextBox.Text;
            string orderType = TypeTextBox.Text;
            string status = StatusTextBlock.Text;
            long orderId = GetOrderId(tableNumber);

            if (orderedItems.Count == 0)
            {
                MessageBox.Show("No item on this table. Please save before payment");
                return;
            }
            else if (ExistedItem() == false && orderedItems.Count != existItemCount)
            {
                MessageBox.Show("New Item(s) has not saved yet. Please save before payment");
                return;
            }
            else if (orderedItems.Count < existItemCount)
            {
                MessageBox.Show("Remove Item has not saved yet. Please save before payment");
                return;
            }

                else
                {
                    PaymentPage paymentPage = new PaymentPage(orderedItems, tableNumber, orderType, orderId, status, false);
                    paymentPage.Show();
                    this.Close();
                }
        }

        //back button
        private void Back_to_TablePage(object sender, RoutedEventArgs e)
        {
            if (orderedItems.Count != existItemCount)
            {
                MessageBox.Show("yes void order!");
                MessageBoxResult result = MessageBox.Show("Removed order on the list. \n Do you want to go back without save?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    BackToTablePage();
                    
                }
                else
                {
                    return;
                }
            }

            else if (ExistedItem() == true || StatusTextBlock.Text == "New Order")
            {
                MessageBox.Show("no new order!");
                BackToTablePage();
            }
            else if (ExistedItem() == false)
            {
                MessageBox.Show("yes new order!");
                MessageBoxResult result = MessageBox.Show("There is new item on the list. \n Do you want to go back without save?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    BackToTablePage();
                }
                else
                {
                    return;
                }
            }



                

        }
            
        

        //Method: for go back table page.
        private void BackToTablePage()
        {
            TablePage tablePage = new TablePage();

            if (TypeTextBox.Text.Equals("Take-Out"))

            {
                tablePage.TablePageTab.SelectedIndex = 1;
            }
            else
            {
                tablePage.TablePageTab.SelectedItem = 0;
            }

            tablePage.Show();
            this.Close();
        }

        //Method: check if any item is old item (ie. exist items)
        private bool ExistedItem()
        {
            bool ExistedItem = false;
            foreach (OrderedItem item in orderedItems)
            {
                foreach (OrderedItem itemOnViewList in orderedItems)
                {
                    if (itemOnViewList.IsExistItem == false)
                    {
                        ExistedItem = false; //added new item on list but not yet save
                    }
                    else if (itemOnViewList.IsExistItem == true)
                    {
                        ExistedItem = true; //nothing added on the existing list
                    }

                }
            }
            return ExistedItem;
        }


        private void VoidButton_Click(object sender, RoutedEventArgs e)
        {


            if (OrdersListBox.SelectedItem is Item selectedItem)
            {
                items.Remove(selectedItem);
                TotalAmount -= selectedItem.ItemPrice;
                CultureInfo cultureInfo = new CultureInfo("en-CA");
                cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                TotalAmountTextBlock.Text = TotalAmount.ToString("C", cultureInfo);
                
            }
            else if(OrdersListBox.SelectedItem is OrderedItem selectedOrderedItem)
            {
                if (ExistedItem() == true)
                {

                    
                    orderedItems.Remove(selectedOrderedItem);
                    TotalAmount -= selectedOrderedItem.ItemPrice;
                    TotalAmountTextBlock.Text = TotalAmount.ToString();
                    CultureInfo cultureInfo = new CultureInfo("en-CA");
                    cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                    TotalAmountTextBlock.Text = TotalAmount.ToString("C", cultureInfo);
                }
            }
                
            else
            {
                MessageBox.Show("Please select an item to void.");
            }
        }




        //(Save button) 
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
 
            // Save the order
            AutoSave();

            // Print the receipt
            PrintKitchenReceipt();

            MessageBox.Show("Order sent to Kitchen successfully!");

            orderedItems.Clear();






        }

        // Method to save the order
        private void AutoSave()
        {
            if (orderedItems.Count == 0 && itemClick == false)
            {
                MessageBox.Show("No Item in this table.Please add items before save!");
                return;
            }
            else if (ExistedItem() == true && orderedItems.Count > existItemCount)
            {
                MessageBox.Show("No update on the list. Please check again");
                return;
            }
            else
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    try
                    {
                        conn.Open();

                        long orderId = GetOrderId(_tableNumber);

                        if (StatusTextBlock.Text.Equals("New Order"))
                        {
                            foreach (OrderedItem ordered in orderedItems)
                            {
                                string message = $"Order ID: {ordered.order_id}\n" +
                                                 $"Item ID: {ordered.item_id}\n" +
                                                 $"Item Name: {ordered.item_name}\n" +
                                                 $"Quantity: {ordered.Quantity}\n" +
                                                 $"Item Price: {ordered.ItemPrice:C}\n" +  // Display as currency
                                                 $"Is Existing Item: {ordered.IsExistItem}";

                                MessageBox.Show(message);
                            }
                            string orderSql = "INSERT INTO `order` (table_num, order_timestamp, total_amount, order_type, paid) VALUES (@tableNum, @orderTimestamp, @totalAmount, @order_type,'n');";
                            MySqlCommand orderCmd = new MySqlCommand(orderSql, conn);
                            orderCmd.Parameters.AddWithValue("@tableNum", _tableNumber);
                            orderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                            orderCmd.Parameters.AddWithValue("@totalAmount", TotalAmount);
                            orderCmd.Parameters.AddWithValue("@order_type", TypeTextBox.Text);
                            orderCmd.ExecuteNonQuery();
                            orderId = orderCmd.LastInsertedId;
                            foreach (OrderedItem newOrder in orderedItems)
                            {


                                string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, item_name, quantity, item_price) VALUES (@orderId, @itemId, @itemName, @quantity, @itemPrice);";
                                MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                                itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                itemCmd.Parameters.AddWithValue("@itemId", newOrder.item_id);
                                itemCmd.Parameters.AddWithValue("@itemName", newOrder.item_name);
                                itemCmd.Parameters.AddWithValue("@quantity", 1);
                                itemCmd.Parameters.AddWithValue("@itemPrice", newOrder.ItemPrice);
                                itemCmd.ExecuteNonQuery();

                            }

                        }
                        else if (orderedItems.Count == 0)
                        {
                            MessageBox.Show("Please add at least one item");
                            return;
                        }
                        else if (StatusTextBlock.Text.Equals("Occupied"))
                        {




                            string removeOrderedItemlistSql = "DELETE FROM ordered_itemlist WHERE order_id = @orderId;";
                            MySqlCommand removeOrderCmd = new MySqlCommand(removeOrderedItemlistSql, conn);
                            removeOrderCmd.Parameters.AddWithValue("@orderId", orderId);
                            removeOrderCmd.ExecuteNonQuery();

                            string updateOrderSql = "UPDATE `order` SET order_timestamp = @orderTimestamp, total_amount = @totalAmount WHERE order_id = @orderId; ";
                            MySqlCommand updateOrderCmd = new MySqlCommand(updateOrderSql, conn);
                            updateOrderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                            updateOrderCmd.Parameters.AddWithValue("@totalAmount", TotalAmount);
                            updateOrderCmd.Parameters.AddWithValue("@orderId", orderId);
                            updateOrderCmd.ExecuteNonQuery();

                            foreach (OrderedItem orderedItem in orderedItems)
                            {

                                string itemSql = "INSERT INTO ordered_itemlist(order_id, item_id, item_name, quantity, item_price) VALUES(@orderId, @itemId, @itemName, @quantity, @itemPrice);";
                                MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                                itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                itemCmd.Parameters.AddWithValue("@itemId", orderedItem.item_id);
                                itemCmd.Parameters.AddWithValue("@itemName", orderedItem.item_name);
                                itemCmd.Parameters.AddWithValue("@quantity", 1);
                                itemCmd.Parameters.AddWithValue("@itemPrice", orderedItem.ItemPrice);
                                itemCmd.ExecuteNonQuery();


                            }



                        }
                        MessageBox.Show("Order save successfully!");

                        /*items.Clear();*/
                        TotalAmount = 0.0;
                        TotalAmountTextBlock.Text = TotalAmount.ToString("C");

                        TablePage tablePage = new TablePage();
                        tablePage.Show();
                        this.Close();
                        conn.Close();
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("MySQL Error: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving order: " + ex.ToString());
                    }
                }
            }
        }

        public void PrintKitchenReceipt()
        {
            // Create a FlowDocument for the kitchen receipt
            FlowDocument kitchenReceiptDocument = new FlowDocument();

            // Header for the receipt including table number, order type, order number, and date and time formatted
            Paragraph headerParagraph = new Paragraph();
            headerParagraph.FontSize = 20;
            headerParagraph.TextAlignment = TextAlignment.Justify;
            headerParagraph.Inlines.Add(new Run("Kitchen Receipt") { FontWeight = FontWeights.Bold });
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new Run("Table: " + TableNumberTextBox.Text));
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new Run("Order Type: " + TypeTextBox.Text));
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new Run("Order Number: " + OrderIdTextBlock.Text));
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new Run("Date and Time: " + DateTime.Now.ToString("MMMM/dd/yyyy hh:mm")));
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new LineBreak());
            kitchenReceiptDocument.Blocks.Add(headerParagraph);

            // Create a Table for the items
            Table itemsTable = new Table();
            TableRowGroup itemTableRowGroup = new TableRowGroup();

            // Create a header row for the items table
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Item")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Quantity")) { FontWeight = FontWeights.Bold }));
            itemTableRowGroup.Rows.Add(headerRow);

            // Create a Section for the order details
            Section orderDetailsSection = new Section();

            // Create a dictionary to store item quantities
            Dictionary<string, int> itemQuantities = new Dictionary<string, int>();


            // Add quantities for ordered items
            foreach (var orderedItem in orderedItems)
            {
                if (itemQuantities.ContainsKey(orderedItem.item_name))
                {
                    itemQuantities[orderedItem.item_name] += orderedItem.Quantity;
                }
                else
                {
                    itemQuantities.Add(orderedItem.item_name, orderedItem.Quantity);
                }

            }

            // Add rows for item details (e.g., items and quantities)
            foreach (var kvp in itemQuantities)
            {
                TableRow itemRow = new TableRow();
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run(kvp.Key))));
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run(kvp.Value.ToString()))));
                itemTableRowGroup.Rows.Add(itemRow);
            }

            itemsTable.RowGroups.Add(itemTableRowGroup);
            orderDetailsSection.Blocks.Add(itemsTable);

            // Add the order details section to the FlowDocument
            kitchenReceiptDocument.Blocks.Add(orderDetailsSection);

            // Create a DocumentPaginator for the FlowDocument
            IDocumentPaginatorSource paginatorSource = kitchenReceiptDocument;
            DocumentPaginator documentPaginator = paginatorSource.DocumentPaginator;

            // Create a PrintDialog
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Print the kitchen receipt
                printDialog.PrintDocument(documentPaginator, "Kitchen Receipt");
            }


        }

        //Method: Get Order Id
        private long GetOrderId(string tableNumber)
        {
            long orderId = 0;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string checkUnpaidOrderSql = "SELECT order_id FROM `order` WHERE table_num = @tableNum AND paid = 'n';";
                    MySqlCommand checkUnpaidOrderCmd = new MySqlCommand(checkUnpaidOrderSql, conn);
                    checkUnpaidOrderCmd.Parameters.AddWithValue("@tableNum", tableNumber);

                    object existingOrderId = checkUnpaidOrderCmd.ExecuteScalar();

                    if (existingOrderId != null)
                    {
                        orderId = Convert.ToInt64(existingOrderId);
                    }
                    else
                    {
                        orderId = checkUnpaidOrderCmd.LastInsertedId;
                    }
                    conn.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error getting order_id: " + ex.ToString());
                }
            }

            return orderId;

        }


        // Print customer receipt
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Calculate GST (5% of TotalAmount)
                double gstRate = 0.05;  // GST rate as 5%
                double gstAmount = TotalAmount * gstRate;
                // Calculate TotalAmount with GST included
                double totalAmountWithGST = TotalAmount + gstAmount;

                // Create a FlowDocument
                FlowDocument flowDocument = new FlowDocument();

                // Create a Paragraph for the header
                Paragraph headerParagraph = new Paragraph();
                headerParagraph.FontSize = 30;
                headerParagraph.TextAlignment = TextAlignment.Center;

                // Create a Run for the header text
                Run headerRun = new Run("Order Receipt");

                // Create an Underline element
                Underline underline = new Underline(headerRun);

                // Add the Underline to the Paragraph
                headerParagraph.Inlines.Add(underline);

                // Add the Paragraph to the FlowDocument
                flowDocument.Blocks.Add(headerParagraph);

                // Create a Section for the order details
                Section orderDetailsSection = new Section();

                // Table to display order details
                Table detailsTable = new Table();
                TableRowGroup tableRowGroup = new TableRowGroup();

                // Add rows for order details
                tableRowGroup.Rows.Add(CreateTableRow("Table:", TableNumberTextBox.Text));
                if (OrderIdTextBlock != null) // Check if the TextBlock exists
                {
                    // Access the text of the OrderIdTextBlock
                    tableRowGroup.Rows.Add(CreateTableRow("Order ID:", OrderIdTextBlock.Text));
                }
                // Add space (empty TableRow) for the gap
                tableRowGroup.Rows.Add(CreateEmptyTableRow());

                // Access the 'Items' collection and loop through it to add item rows.
                foreach (var OrderedItem in orderedItems)
                {
                    tableRowGroup.Rows.Add(CreateTableRow(OrderedItem.item_name, OrderedItem.ItemPrice.ToString("C")));
                }

                // Add space (empty TableRow) for the gap
                tableRowGroup.Rows.Add(CreateEmptyTableRow());

                // Create a Paragraph for "Sub Total" with underline
                // Create a Paragraph for "Sub Total" with underline
                Paragraph subTotalParagraph = new Paragraph(new Run("Sub Total:"));
                subTotalParagraph.FontSize = 20; // Increase the font size
                subTotalParagraph.TextAlignment = TextAlignment.Right;

                Paragraph subTotalValueParagraph = new Paragraph(new Run(TotalAmount.ToString("C")));
                tableRowGroup.Rows.Add(CreateTableRowWithParagraph(subTotalParagraph, subTotalValueParagraph));
                // Create a Paragraph for "GST"
                Paragraph gstLabelParagraph = new Paragraph(new Run("GST (5%):"));
                gstLabelParagraph.FontSize = 20; // Increase the font size
                gstLabelParagraph.TextAlignment = TextAlignment.Right;

                Paragraph gstValueParagraph = new Paragraph(new Run(gstAmount.ToString("C")));

                // Add the "GST" label and value to the TableRowGroup
                tableRowGroup.Rows.Add(CreateTableRowWithParagraph(gstLabelParagraph, gstValueParagraph));

                // Create a Paragraph for "Total Amount"
                Paragraph totalAmountLabelParagraph = new Paragraph(new Run("Total Amount:"));
                totalAmountLabelParagraph.FontSize = 20; // Increase the font size
                totalAmountLabelParagraph.TextAlignment = TextAlignment.Right;

                Paragraph totalAmountValueParagraph = new Paragraph(new Run(totalAmountWithGST.ToString("C")));

                // Add the "Total Amount" label and value to the TableRowGroup
                tableRowGroup.Rows.Add(CreateTableRowWithParagraph(totalAmountLabelParagraph, totalAmountValueParagraph));

                detailsTable.RowGroups.Add(tableRowGroup);
                orderDetailsSection.Blocks.Add(detailsTable);

                flowDocument.Blocks.Add(orderDetailsSection);

                // Create a DocumentPaginator for the FlowDocument
                IDocumentPaginatorSource paginatorSource = flowDocument;
                DocumentPaginator documentPaginator = paginatorSource.DocumentPaginator;

                // Send the document to the printer
                printDialog.PrintDocument(documentPaginator, "Order Receipt");
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


        // For Styling
        private void SetButtonStyle(Button button)
        {
            button.FontFamily = new FontFamily("Verdana");
            button.FontSize = 20;
            button.Background = Brushes.Orange;
            button.FontWeight = FontWeights.Bold;
            button.BorderBrush = Brushes.Orange;
            button.Margin = new Thickness(5);
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

        private TableRow CreateEmptyTableRow()
        {
            TableRow row = new TableRow();

            TableCell emptyCell = new TableCell(new Paragraph(new Run(" "))); // Add a space or empty string
            emptyCell.ColumnSpan = 2; // Set the column span to cover both columns

            row.Cells.Add(emptyCell);

            return row;
        }
        //Thevagi splitbill
        private void SplitBillButton_Click(object sender, RoutedEventArgs e)
        {
            SplitBillDialog dialog = new SplitBillDialog(TotalAmount);
            dialog.Owner = this; // Set the owner window to handle dialog behavior
            dialog.ShowDialog();
        }





        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryData();

        }


    }
}