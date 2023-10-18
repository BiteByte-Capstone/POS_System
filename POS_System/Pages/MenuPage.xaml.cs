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

namespace POS_System.Pages
{
    public partial class MenuPage : Window
    {
        // Define connStr at the class level
        private string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        private ObservableCollection<Item> items = new ObservableCollection<Item>();
        private ObservableCollection<Category> categories = new ObservableCollection<Category>();

        public MenuPage()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += Window_Loaded; // Subscribe to the Loaded event

            // Bind the ObservableCollection to the OrdersListBox
            OrdersListBox.ItemsSource = items;
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

        private bool CheckForUnpaidOrders(string tableNumber)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string checkOrdersSql = "SELECT COUNT(*) FROM `order` WHERE table_num = @tableNum AND paid = 'n';";
                    MySqlCommand checkOrdersCmd = new MySqlCommand(checkOrdersSql, conn);
                    checkOrdersCmd.Parameters.AddWithValue("@tableNum", tableNumber);
                    int unpaidOrderCount = Convert.ToInt32(checkOrdersCmd.ExecuteScalar());
                    return unpaidOrderCount > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error checking for unpaid orders: " + ex.ToString());
                    return false;
                }
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
                            item_id = Convert.ToInt32(row["item_id"]),
                            item_name = row["item_name"].ToString(),
                            item_price = Convert.ToDouble(row["item_price"]),
                            item_description = row["item_description"].ToString(),
                            item_category = row["item_category"].ToString()
                        };
                        items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading unpaid orders: " + ex.ToString());
                }
            }
        }

        private void LoadUnpaidItems(long orderId)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string loadUnpaidItemsSql = "SELECT i.item_id, i.item_name, i.item_price, i.item_description, i.item_category " +
                        "FROM item i " +
                        "INNER JOIN unpaid_itemlist u ON i.item_id = u.item_id " +
                        "WHERE u.order_id = @orderId;";
                    MySqlCommand loadUnpaidItemsCmd = new MySqlCommand(loadUnpaidItemsSql, conn);
                    loadUnpaidItemsCmd.Parameters.AddWithValue("@orderId", orderId);

                    MySqlDataAdapter unpaidDataAdapter = new MySqlDataAdapter(loadUnpaidItemsCmd);
                    DataTable unpaidItemsTable = new DataTable();
                    unpaidDataAdapter.Fill(unpaidItemsTable);

                    items.Clear();

                    if (unpaidItemsTable.Rows.Count > 0)
                    {
                        OrderIdTextBlock.Text = "Order ID: " + orderId.ToString();
                    }
                    else
                    {
                        OrderIdTextBlock.Text = "No unpaid orders found.";
                    }

                    foreach (DataRow row in unpaidItemsTable.Rows)
                    {
                        Item item = new Item
                        {
                            item_id = Convert.ToInt32(row["item_id"]),
                            item_name = row["item_name"].ToString(),
                            item_price = Convert.ToDouble(row["item_price"]),
                            item_description = row["item_description"].ToString(),
                            item_category = row["item_category"].ToString()
                        };
                        items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading unpaid items: " + ex.ToString());
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
                        item_id = Convert.ToInt32(row["item_id"]),
                        item_name = row["item_name"].ToString(),
                        item_price = Convert.ToDouble(row["item_price"]),
                        item_description = row["item_description"].ToString(),
                        item_category = row["item_category"].ToString()
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

        private void SetButtonStyle(Button button)
        {
            button.FontFamily = new FontFamily("Verdana");
            button.FontSize = 20;
            button.Background = Brushes.Orange;
            button.FontWeight = FontWeights.Bold;
            button.BorderBrush = Brushes.Orange;
            button.Margin = new Thickness(5);
        }

        private double TotalAmount = 0.0;

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
                                string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, quantity, item_price) VALUES (@orderId, @itemId, @quantity, @itemPrice);";
                                MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                                itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                itemCmd.Parameters.AddWithValue("@itemId", itemId);
                                itemCmd.Parameters.AddWithValue("@quantity", 1);
                                itemCmd.Parameters.AddWithValue("@itemPrice", item.Price);
                                itemCmd.ExecuteNonQuery();

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

        private void SendButton_Click(object sender, RoutedEventArgs e)
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            long orderId = GetOrderId(TableNumberTextBox.Text);

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Save the current unpaid items to the 'ordered_itemlist' table with the same order_id
                    string saveUnpaidItemsSql = "INSERT INTO ordered_itemlist (order_id, item_id, quantity, item_price) " +
                        "SELECT @orderId, oi.item_id, oi.quantity, oi.item_price FROM ordered_itemlist oi " +
                        "WHERE oi.order_id = @orderId;";
                    MySqlCommand saveUnpaidItemsCmd = new MySqlCommand(saveUnpaidItemsSql, conn);
                    saveUnpaidItemsCmd.Parameters.AddWithValue("@orderId", orderId);
                    saveUnpaidItemsCmd.ExecuteNonQuery();

                    items.Clear(); // Clear the list after saving

                    // Continue with other operations
                    // ...
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving unpaid items: " + ex.ToString());
                }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Create a FlowDocument
                FlowDocument flowDocument = new FlowDocument();

                // Create a Paragraph for the header
                Paragraph headerParagraph = new Paragraph(new Run("Order Receipt"));
                headerParagraph.FontSize = 20;
                headerParagraph.TextAlignment = TextAlignment.Center;
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

                // Access the 'Items' collection or replace it with the correct collection
                // and loop through it to add item rows.
                // Access the 'items' collection and loop through it to add item rows.
                foreach (var item in items)
                {
                    tableRowGroup.Rows.Add(CreateTableRow(item.Name, item.Price.ToString("C")));
                }


                // Calculate TotalAmount here if it's not already calculated.
                tableRowGroup.Rows.Add(CreateTableRow("Total Amount:", TotalAmount.ToString("C")));

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
            labelCell.BorderThickness = new Thickness(0, 0, 1, 1);
            labelCell.BorderBrush = Brushes.Black;
            row.Cells.Add(labelCell);

            // Value cell
            TableCell valueCell = new TableCell(new Paragraph(new Run(value)));
            valueCell.BorderThickness = new Thickness(0, 0, 0, 1);
            valueCell.BorderBrush = Brushes.Black;
            row.Cells.Add(valueCell);

            return row;
        }





        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryData();
        }
    }
}
