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

namespace POS_System.Pages
{
    public partial class MenuPage : Window
    {
        // Define connStr at the class level
        public string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        //new order
        private ObservableCollection<Item> items = new ObservableCollection<Item>();
        private ObservableCollection<Category> categories = new ObservableCollection<Category>();

        //existing order
        private ObservableCollection<OrderedItem> orderedItems = new ObservableCollection<OrderedItem>();

       
        private double TotalAmount = 0.0;
        private string tableNumber = "";

        //Constructor 
        public MenuPage()
        {
            InitializeComponent();
            
            //it could load the page before show up
            this.DataContext = this;
            this.Loaded += Window_Loaded; // Subscribe to the Loaded event
            
            // Bind the ObservableCollection to the OrdersListBox
            


        }

        public MenuPage(string tableNumber, string orderType, string status, bool hasUnpaidOrders) : this()
        {
            TableNumberTextBox.Text = tableNumber;
            TypeTextBox.Text = orderType;
            StatusTextBlock.Text = status;

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
                    dataAdapter.Fill(unpaidOrdersTable);
                    /*items.Clear();*/
                    
                    if (unpaidOrdersTable.Rows.Count > 0)
                    {
                        MessageBox.Show("Yo");
                        OrderIdTextBlock.Text = orderId.ToString();
                    }
                    else if (unpaidOrdersTable.Rows.Count == 0)
                    {
                        MessageBox.Show("noooo");
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
            
            MySqlConnection mySqlConnection = new MySqlConnection(connStr);

            try
            {
                mySqlConnection.Open();
                string sql = "SELECT * FROM category;";
                MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection);
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
            mySqlConnection.Close();
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
                    newItemButton.Height = 60;
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
            String tableNumber = TableNumberTextBox.Text; 
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is Item)
            {
                Item item = clickedButton.Tag as Item;

                if (item != null)
                {
                    if (orderedItems.Count == 0 && StatusTextBlock.Text.Equals("New Order")) 
                    {
                        items.Add(item);
                        OrdersListBox.ItemsSource = items;
                        TotalAmount += item.ItemPrice;
                        CultureInfo cultureInfo = new CultureInfo("en-CA");
                        cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                        TotalAmountTextBlock.Text = TotalAmount.ToString("C", cultureInfo);


                    }
                    else
                    {
                        //convert it into ordered list item
                        AddItemToOrder(item);


                    }
                }
            }
        }

        //(edit item list) Add item to the list
        private void AddItemToOrder(Item item)
        {
            // Convert Item to OrderedItem
            OrderedItem orderedItem = new OrderedItem
            {
               
                item_id = item.Id,
                item_name = item.item_name,
                Quantity = 1, // Assuming quantity of 1 for new items
                ItemPrice = item.ItemPrice
            };

            orderedItems.Add(orderedItem);
            OrdersListBox.ItemsSource = orderedItems;
            TotalAmount += orderedItem.ItemPrice;
            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            TotalAmountTextBlock.Text = TotalAmount.ToString("C", cultureInfo);
        }


        //back button
        private void Back_to_TablePage(object sender, RoutedEventArgs e)
        {
            TablePage tablePage = new TablePage();
            tablePage.Show();
            this.Close();
        }




        private void VoidButton_Click(object sender, RoutedEventArgs e)
        {

            if (OrdersListBox.SelectedItem is Item selectedItem)
            {
                items.Remove(selectedItem);
                TotalAmount -= selectedItem.ItemPrice;
                TotalAmountTextBlock.Text = TotalAmount.ToString();
            }
            else if (OrdersListBox.SelectedItem is OrderedItem selectedOrderedItem)
            {
                orderedItems.Remove(selectedOrderedItem);
                TotalAmount -= selectedOrderedItem.ItemPrice;
                TotalAmountTextBlock.Text = TotalAmount.ToString();
            }
            else
            {
                MessageBox.Show("Please select an item to void.");
            }
        }


        //(Save button) 
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            
            /*DataTable unpaidOrdersTable = new DataTable();

            if (unpaidOrdersTable.Rows.Count ==0)
            {
                MessageBoxResult result = MessageBox.Show("There are no items on the list.\nAre you sure you want to quit the order input?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    TablePage tablePage = new TablePage();
                    tablePage.Show();
                    this.Close();
                    return;
                }
                else if (result == MessageBoxResult.No)
                {
                    return;
                }

            } 
            else
            {*/
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    long orderId = GetOrderId(TableNumberTextBox.Text);

                    if (StatusTextBlock.Text.Equals("New Table"))
                    {
                        string orderSql = "INSERT INTO `order` (table_num, order_timestamp, total_amount, paid) VALUES (@tableNum, @orderTimestamp, @totalAmount, 'n');";
                        MySqlCommand orderCmd = new MySqlCommand(orderSql, conn);
                        orderCmd.Parameters.AddWithValue("@tableNum", TableNumberTextBox.Text);
                        orderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                        orderCmd.Parameters.AddWithValue("@totalAmount", TotalAmount);
                        orderCmd.ExecuteNonQuery();
                        orderId = orderCmd.LastInsertedId;
                        foreach (Item newOrder in items)
                        {
                            string checkItemSql = "SELECT item_id FROM item WHERE item_name = @itemName;";
                            MySqlCommand checkItemCmd = new MySqlCommand(checkItemSql, conn);
                            checkItemCmd.Parameters.AddWithValue("@itemName", newOrder.item_name);
                            object itemId = checkItemCmd.ExecuteScalar();

                            if (itemId != null)
                            {
                                string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, quantity, item_price) VALUES (@orderId, @itemId, @quantity, @itemPrice);";
                                MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                                itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                itemCmd.Parameters.AddWithValue("@itemId", itemId);
                                itemCmd.Parameters.AddWithValue("@quantity", 1);
                                itemCmd.Parameters.AddWithValue("@itemPrice", newOrder.ItemPrice);
                                itemCmd.ExecuteNonQuery();
                            }
                        }

                    }
                    else
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
                    }
                            foreach (OrderedItem orderedItem in orderedItems)
                        {
     
                                string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, quantity, item_price) VALUES (@orderId, @itemId, @quantity, @itemPrice);";
                                MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                                itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                itemCmd.Parameters.AddWithValue("@itemId", orderedItem.item_id);
                                itemCmd.Parameters.AddWithValue("@quantity", 1);
                                itemCmd.Parameters.AddWithValue("@itemPrice", orderedItem.ItemPrice);
                                itemCmd.ExecuteNonQuery();
                           

                        }
                    

                    MessageBox.Show("Order sent successfully!");

                    items.Clear();
                    TotalAmount = 0.0;
                    TotalAmountTextBlock.Text = TotalAmount.ToString("C");

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
            /*}*/
        }



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
                    } else
                    {
                        orderId = checkUnpaidOrderCmd.LastInsertedId;
                    }

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

        //(button) go to payment page
        private void PaymentButton_Click(object sender, RoutedEventArgs e)
        {
            string tableNumber = TableNumberTextBox.Text;
            string orderType = TypeTextBox.Text;
            string status = StatusTextBlock.Text;
            long orderId = GetOrderId(tableNumber);
            PaymentPage paymentPage = new PaymentPage(orderedItems,tableNumber,orderType,orderId, status,false);
            paymentPage.Show();
            this.Close();
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




        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryData();
            
        }


    }
}