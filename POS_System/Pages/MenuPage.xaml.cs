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

namespace POS_System.Pages
{
    public partial class MenuPage : Window
    {
        // Define connStr at the class level
        private string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        private ObservableCollection<Item> items = new ObservableCollection<Item>();
        private ObservableCollection<Category> categories = new ObservableCollection<Category>();
        
        private double TotalAmount = 0.0;

        public MenuPage()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += Window_Loaded; // Subscribe to the Loaded event

            // Bind the ObservableCollection to the OrdersListBox
           // OrdersListBox.ItemsSource = items;

            
        }

        public MenuPage(string tableNumber, string orderType, bool hasUnpaidOrders) : this()
        {
            TableNumberTextBox.Text = tableNumber;
            TypeTextBox.Text = orderType;

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
                    string checkUnpaidOrderSql = "SELECT order_id FROM `order` WHERE table_num = @tableNum AND paid = 'n';";
                    MySqlCommand checkUnpaidOrderCmd = new MySqlCommand(checkUnpaidOrderSql, conn);
                    checkUnpaidOrderCmd.Parameters.AddWithValue("@tableNum", tableNumber);
                    object existingOrderId = checkUnpaidOrderCmd.ExecuteScalar();
                    long orderId;

                    if (existingOrderId != null)
                    {
                        orderId = Convert.ToInt64(existingOrderId);
                    }
                    else
                    {
                        // If no unpaid order exists, create a new order
                        string createOrderSql = "INSERT INTO `order` (table_num, order_timestamp, total_amount, paid) VALUES (@tableNum, @orderTimestamp, 0, 'n');";
                        MySqlCommand createOrderCmd = new MySqlCommand(createOrderSql, conn);
                        createOrderCmd.Parameters.AddWithValue("@tableNum", tableNumber);
                        createOrderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                        createOrderCmd.ExecuteNonQuery();
                        orderId = createOrderCmd.LastInsertedId;
                    }

                    string unpaidOrdersSql = "SELECT item_id, item_name, item_price, item_description, item_category FROM item;";
                    MySqlCommand unpaidOrdersCmd = new MySqlCommand(unpaidOrdersSql, conn);
                    MySqlDataAdapter dataAdapter = new MySqlDataAdapter(unpaidOrdersCmd);
                    DataTable unpaidOrdersTable = new DataTable();
                    dataAdapter.Fill(unpaidOrdersTable);
                    items.Clear();

                    if (unpaidOrdersTable.Rows.Count > 0)
                    {
                        OrderIdTextBlock.Text = orderId.ToString();
                    }
                    else
                    {
                        OrderIdTextBlock.Text = "No unpaid orders found.";
                    }

                    foreach (DataRow row in unpaidOrdersTable.Rows)
                    {
                        Item item = new Item
                        {
                            Id = Convert.ToInt32(row["item_id"]),
                            Name = row["item_name"].ToString(),
                            Price = Convert.ToDouble(row["item_price"]),
                            Description = row["item_description"].ToString(),
                            Category = row["item_category"].ToString()
                        };
                        items.Add(item);
                        TotalAmount += item.Price;
                    }
                    TotalAmountTextBlock.Text = TotalAmount.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading unpaid orders: " + ex.ToString());
                }
            }
        }

       


        private void LoadCategoryData()
        {
            string connectString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection mySqlConnection = new MySqlConnection(connectString);

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
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
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
                        Name = rdr["item_name"].ToString(),
                        Price = Convert.ToDouble(rdr["item_price"]),
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
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is Item)
            {
                Item item = clickedButton.Tag as Item;

                if (item != null)
                {
                    long orderId = GetOrderId(TableNumberTextBox.Text);
                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    {
                        try
                        {
                            conn.Open();
                            string checkItemSql = "SELECT item_id FROM item WHERE item_name = @itemName;";
                            MySqlCommand checkItemCmd = new MySqlCommand(checkItemSql, conn);
                            checkItemCmd.Parameters.AddWithValue("@itemName", item.Name);
                            object itemId = checkItemCmd.ExecuteScalar();

                            if (itemId != null)
                            {
                                // Check if the item already exists in the order
                                string existingItemSql = "SELECT quantity FROM ordered_itemlist WHERE order_id = @orderId AND item_id = @itemId;";
                                MySqlCommand existingItemCmd = new MySqlCommand(existingItemSql, conn);
                                existingItemCmd.Parameters.AddWithValue("@orderId", orderId);
                                existingItemCmd.Parameters.AddWithValue("@itemId", itemId);
                                object existingQuantity = existingItemCmd.ExecuteScalar();
                                // Check if an item with the same name already exists in the order
                                //var existingOrderedItem = orderedItems.FirstOrDefault(oi => oi.Item.Name == item.Name);

                                if (existingQuantity != null)
                                {
                                    // If the item already exists, update its quantity
                                    int newQuantity = Convert.ToInt32(existingQuantity) + 1;
                                    string updateQuantitySql = "UPDATE ordered_itemlist SET quantity = @newQuantity WHERE order_id = @orderId AND item_id = @itemId;";
                                    MySqlCommand updateQuantityCmd = new MySqlCommand(updateQuantitySql, conn);
                                    updateQuantityCmd.Parameters.AddWithValue("@orderId", orderId);
                                    updateQuantityCmd.Parameters.AddWithValue("@itemId", itemId);
                                    updateQuantityCmd.Parameters.AddWithValue("@newQuantity", newQuantity);
                                    updateQuantityCmd.ExecuteNonQuery();
                                }
                                else
                                {
                                    // If the item does not exist in the order, add a new entry
                                    string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, quantity, item_price) VALUES (@orderId, @itemId, @quantity, @itemPrice);";
                                    MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                                    itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                    itemCmd.Parameters.AddWithValue("@itemId", itemId);
                                    itemCmd.Parameters.AddWithValue("@quantity", 1);
                                    itemCmd.Parameters.AddWithValue("@itemPrice", item.Price);
                                    itemCmd.ExecuteNonQuery();
                                }

                                TotalAmount += item.Price;
                                TotalAmountTextBlock.Text = TotalAmount.ToString("C");
                            }
                            else
                            {
                                MessageBox.Show($"Item '{item.Name}' does not exist in the database.");
                            }
                        }
                        catch (MySqlException ex)
                        {
                            MessageBox.Show("MySQL Error: " + ex.Message);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error adding item to the order: " + ex.ToString());
                        }
                    }
                }
            }
        }

        //back button
        private void Back_to_TablePage(object sender, RoutedEventArgs e)
        {
            TablePage tablePage = new TablePage(TableNumberTextBox.Text, TypeTextBox.Text);
            tablePage.Show();
            this.Close();
        }


        private void PaymentButton(object sender, RoutedEventArgs e)
        {
            PaymentPage paymentPage = new PaymentPage();
            paymentPage.Show();
            this.Close();
        }

        private void VoidButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListBox.SelectedItem is Item selectedItem)
            {
                long orderId = GetOrderId(TableNumberTextBox.Text);
                int itemId = selectedItem.Id;
                double itemPrice = selectedItem.Price;

                items.Remove(selectedItem);

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    try
                    {
                        conn.Open();
                        string deleteItemSql = "DELETE FROM ordered_itemlist WHERE order_id = @orderId AND item_id = @itemId;";
                        MySqlCommand deleteItemCmd = new MySqlCommand(deleteItemSql, conn);
                        deleteItemCmd.Parameters.AddWithValue("@orderId", orderId);
                        deleteItemCmd.Parameters.AddWithValue("@itemId", itemId);
                        deleteItemCmd.ExecuteNonQuery();

                        TotalAmount -= itemPrice;
                        TotalAmountTextBlock.Text = TotalAmount.ToString("C");
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("MySQL Error: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error voiding item from the order: " + ex.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an item to void.");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string orderSql = "INSERT INTO `order` (table_num, order_timestamp, total_amount, paid) VALUES (@tableNum, @orderTimestamp, @totalAmount, 'n');";
                    MySqlCommand orderCmd = new MySqlCommand(orderSql, conn);
                    orderCmd.Parameters.AddWithValue("@tableNum", TableNumberTextBox.Text);
                    orderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                    orderCmd.Parameters.AddWithValue("@totalAmount", TotalAmount);
                    orderCmd.ExecuteNonQuery();

                    long orderId = orderCmd.LastInsertedId;

                    foreach (Item orderedItem in items)
                    {
                        string checkItemSql = "SELECT item_id FROM item WHERE item_name = @itemName;";
                        MySqlCommand checkItemCmd = new MySqlCommand(checkItemSql, conn);
                        checkItemCmd.Parameters.AddWithValue("@itemName", orderedItem.Name);
                        object itemId = checkItemCmd.ExecuteScalar();

                        if (itemId != null)
                        {
                            string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, quantity, item_price) VALUES (@orderId, @itemId, @quantity, @itemPrice);";
                            MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                            itemCmd.Parameters.AddWithValue("@orderId", orderId);
                            itemCmd.Parameters.AddWithValue("@itemId", itemId);
                            itemCmd.Parameters.AddWithValue("@quantity", 1);
                            itemCmd.Parameters.AddWithValue("@itemPrice", orderedItem.Price);
                            itemCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            MessageBox.Show($"Item '{orderedItem.Name}' does not exist in the database.");
                        }
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
                    }
                    else
                    {
                        MessageBox.Show("No unpaid order found for the table.");
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
                foreach (var item in items)
                {
                    tableRowGroup.Rows.Add(CreateTableRow(item.Name, item.Price.ToString("C")));
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



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryData();
        }
    }
}