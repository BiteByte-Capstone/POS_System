﻿using MySql.Data.MySqlClient;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Window
    {
        public MenuPage()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += Window_Loaded; // Subscribe to the Loaded event
            
        }

        public MenuPage(string tableNumber, string orderType, bool hasUnpaidOrders) : this()
        {
            TableNumberTextBox.Text = tableNumber;
            TypeTextBox.Text = orderType;

            if (hasUnpaidOrders)
            {
                // Call the method to load unpaid orders for this table.
                LoadUnpaidOrders(tableNumber);
            }
        }
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Category> Category { get; set; } = new ObservableCollection<Category>();

        public ObservableCollection<OrderedItem> OrderedItems { get; set; } = new ObservableCollection<OrderedItem>();

        private bool CheckForUnpaidOrders(string tableNumber)
        {
            // Implement the logic to check for unpaid orders for the specified table number.
            // You can use a MySQL query for this purpose.
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Use a SQL query to check for unpaid orders for the specified table number
                    string checkOrdersSql = "SELECT COUNT(*) FROM `order` WHERE table_num = @tableNum AND paid = 'n';";
                    MySqlCommand checkOrdersCmd = new MySqlCommand(checkOrdersSql, conn);
                    checkOrdersCmd.Parameters.AddWithValue("@tableNum", tableNumber);

                    // Execute the query and get the count of unpaid orders
                    int unpaidOrderCount = Convert.ToInt32(checkOrdersCmd.ExecuteScalar());

                    // If there are unpaid orders (count > 0), return true; otherwise, return false
                    return unpaidOrderCount > 0;
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that may occur during the database operation
                    MessageBox.Show("Error checking for unpaid orders: " + ex.ToString());
                    return false; // Return false in case of an error
                }
            }
        }


        private void LoadUnpaidOrders(string tableNumber)
        {
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Use a SQL query to retrieve unpaid orders for the specified table number
                    string unpaidOrdersSql = "SELECT o.order_id, i.item_name, oi.quantity, i.item_price " +
                          "FROM `order` o " +
                          "INNER JOIN ordered_itemlist oi ON o.order_id = oi.order_id " +
                          "INNER JOIN item i ON oi.item_id = i.item_id " +
                          "WHERE o.table_num = @tableNum AND o.paid = 'n';";

                    MySqlCommand unpaidOrdersCmd = new MySqlCommand(unpaidOrdersSql, conn);
                    unpaidOrdersCmd.Parameters.AddWithValue("@tableNum", tableNumber);
                   
                    MySqlDataReader rdr = unpaidOrdersCmd.ExecuteReader();


                    OrderedItem ordereditem = new OrderedItem()
                    {
                        order_id = Convert.ToInt32(rdr["order_id"]),
                        item_name = rdr["item_name"].ToString(),
                        Quantity = Convert.ToInt32(rdr["quantity"]),
                        ItemPrice= Convert.ToDouble(rdr["item_price"])
    
                    };

                    OrderedItems.Add(ordereditem);
                    OrdersListBox.ItemsSource = OrderedItems;

                    // Bind the DataTable to the OrderListBox
                    /*OrdersListBox.ItemsSource = unpaidOrdersTable.DefaultView;*/

                    // Display the orderid in the TextBlock (assuming there's only one orderid)
/*                    if (unpaidOrdersTable.Rows.Count > 0)
                    {
                        OrderIdTextBlock.Text = "Order ID: " + unpaidOrdersTable.Rows[0]["order_id"].ToString();
                    }
                    else
                    {
                        OrderIdTextBlock.Text = "No unpaid orders found.";
                    }*/
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading unpaid orders: " + ex.ToString());
                }
            }
        }






        private void Open_Table(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                string tableName = button.Name; // get the name of the button
                string orderType = button.Name; // get the name of the button

                int index = tableName.IndexOf('_'); // get the index number after "_"

                // Show table number or take-out order number
                string tableNumber = tableName.Substring(index + 1); // remove the first 5 characters ("table")
                orderType = tableName.Substring(0, index);

                String Type = "";
                if (orderType.Equals("table"))
                {
                    Type = "Dine-In";
                }
                else if (orderType.Equals("takeOut"))
                {
                    Type = "Take-Out";
                }

                // Check if there are unpaid orders for the table
                bool hasUnpaidOrders = CheckForUnpaidOrders(tableNumber);

                // Open the MenuPage with the table number and Type
                MenuPage menuPage = new MenuPage(tableNumber, Type, hasUnpaidOrders);
                menuPage.Show();
                this.Close();
            }
        }

        




        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryData(); // Call LoadCategoryData when the window is loaded           
            /*LoadItemsData(); // Call LoadFoodData when the window is loaded*/
        }



        private void LoadCategoryData()
        {
            /*ItemButtonPanel.Children.Clear();*/
            string connectString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection mySqlConnection = new MySqlConnection(connectString);

            try
            {
                mySqlConnection.Open();

                // Your query to fetch items
                string sql = "SELECT * FROM category;";
                MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    // Create an Item object for each item in database
                    Category category = new Category()
                    {
                        Id = Convert.ToInt32(rdr["category_id"]),
                        Name = rdr["category_name"].ToString(),

                    };

                    /*                    // Add item to Items collection
                                        Items.Add(item);*/

                    // Creating a new button for each item in database
                    Button newCategoryButton = new Button();
                    newCategoryButton.Content = rdr["category_name"].ToString(); // Set the text of the button to the item name
                    newCategoryButton.Tag = category;
                   /* newCategoryButton.Click += CategoryClick; // Assign a click event handler*/
                    newCategoryButton.Click += (sender, e) => LoadItemsByCategory(newCategoryButton.Content.ToString());
                    newCategoryButton.Width = 150; // Set other properties as needed
                    newCategoryButton.Height = 30;
                    newCategoryButton.Margin = new Thickness(5);
                    SetButtonStyle(newCategoryButton);

                    // Add the new button to a container on your window
                    // For example, a StackPanel with the name 'buttonPanel'
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



        private void LoadItemsData()
        {
            
            // Your connection string here
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();

                // Your query to fetch items
                string sql = "SELECT * FROM item;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    // Create an Item object for each item in database
                    Item item = new Item()
                    {
                        item_id = Convert.ToInt32(rdr["item_id"]),
                        item_name = rdr["item_name"].ToString(),
                        item_price = Convert.ToDouble(rdr["item_price"]),
                        item_description = rdr["item_description"].ToString(),
                        item_category = rdr["item_category"].ToString()
                    };

/*                    // Add item to Items collection
                    Items.Add(item);*/

                    // Creating a new button for each item in database
                    Button newItemButton = new Button();
                    newItemButton.Content = rdr["item_name"].ToString(); // Set the text of the button to the item name
                    newItemButton.Tag = item;
                    newItemButton.Click += ItemClick; // Assign a click event handler
                    newItemButton.Width = 150; // Set other properties as needed
                    newItemButton.Height = 30;
                    newItemButton.Margin = new Thickness(5);

                    // Add the new button to a container on your window
                    // For example, a StackPanel with the name 'buttonPanel'
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

        private void LoadItemsByCategory(object categoryName)
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

                    Item item = new Item()
                    {
                        item_id = Convert.ToInt32(rdr["item_id"]),
                        item_name = rdr["item_name"].ToString(),
                        item_price = Convert.ToDouble(rdr["item_price"]),
                        item_description = rdr["item_description"].ToString(),
                        item_category = rdr["item_category"].ToString()
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

/*        private void CategoryClick(object sender, RoutedEventArgs e)
        {
            ItemButtonPanel.Children.Clear();
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
                string sql = "SELECT item_name FROM item WHERE item_category = @category;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@category", categoryName);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Button newButton = new Button();
                    newButton.Content = rdr["item_name"].ToString();
                    newButton.Width = 150;
                    newButton.Height = 60;
                    SetButtonStyle(newButton);
                    newButton.Click += ItemClick;
                    ItemButtonPanel.Children.Add(newButton);
                }

                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }*/

        /*Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is Category)
            {
                Category category = (Category)clickedButton.Tag as Category;

                if (category != null)
                {
                    Category.Add(category);

                    if (category.Name == Item.)

                }
            }*/

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
                Item item = (Item)clickedButton.Tag as Item;

                if (item != null)
                {
                    Items.Add(item);

                    // Assuming your Item class has properties Name, Quantity, and Price
                    TotalAmount += item.item_price;
                    TotalAmountTextBlock.Text = TotalAmount.ToString("C");
                }
                OrdersListBox.ItemsSource = Items;
            }
        }


        private void Back_to_TablePage(object sender, RoutedEventArgs e)
        {
            // Go back to TablePage.xaml when they click on the Back button
            TablePage tablePage = new TablePage(TableNumberTextBox.Text, TypeTextBox.Text);
            tablePage.Show();
            this.Close();
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Change number to correspoding table.
            TablePage tablePage = new TablePage();

        }

        private void PaymentButton(object sender, RoutedEventArgs e)
        {
            PaymentPage paymentPage = new PaymentPage();
            paymentPage.Show();
            this.Close();

        }

        private void OrderTypeTextBox(object sender, TextChangedEventArgs e)
        {
            TablePage tablePage = new TablePage();

        }

        private void VoidButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListBox.SelectedItem is Item selectedItem)
            {
                OrdersListBox.ItemsSource = Items;

                Items.Remove(selectedItem);

                // Remove the selected ordered item from the ObservableCollection
                /*OrdersListBox.Items.Remove(selectedOrderedItem);*/

                // Subtract the item price from the total amount
                TotalAmount -= selectedItem.item_price;
                TotalAmountTextBlock.Text = TotalAmount.ToString("C");
            }
            else
            {
                MessageBox.Show("Please select an item to void.");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a connection string
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Insert the order information into the database
                    string orderSql = "INSERT INTO `order` (table_num, order_timestamp, total_amount, paid) VALUES (@tableNum, @orderTimestamp, @totalAmount, 'n');";
                    MySqlCommand orderCmd = new MySqlCommand(orderSql, conn);
                    orderCmd.Parameters.AddWithValue("@tableNum", TableNumberTextBox.Text);
                    orderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now); // You can adjust this based on your requirement
                    orderCmd.Parameters.AddWithValue("@totalAmount", TotalAmount); // Assuming TotalAmount is of type double
                    orderCmd.ExecuteNonQuery();

                    // Get the last inserted order_id
                    long orderId = orderCmd.LastInsertedId;

                    // Insert the ordered items into the database
                    foreach (Item orderedItem in Items)
                    {
                        // Ensure that the item exists in the `item` table
                        string checkItemSql = "SELECT item_id FROM item WHERE item_name = @itemName;";
                        MySqlCommand checkItemCmd = new MySqlCommand(checkItemSql, conn);
                        checkItemCmd.Parameters.AddWithValue("@itemName", orderedItem.item_name);
                        object itemId = checkItemCmd.ExecuteScalar();

                        if (itemId != null) // Item exists
                        {
                            // Insert the ordered item into the `ordered_itemlist` table
                            string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, quantity, item_price) VALUES (@orderId, @itemId, @quantity, @itemPrice);";
                            MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                            itemCmd.Parameters.AddWithValue("@orderId", orderId);
                            itemCmd.Parameters.AddWithValue("@itemId", itemId);
                            itemCmd.Parameters.AddWithValue("@quantity", 1); // You can modify this if you track item quantity
                            itemCmd.Parameters.AddWithValue("@itemPrice", orderedItem.item_price);
                            itemCmd.ExecuteNonQuery();
                        }
                        else
                        {
                            MessageBox.Show($"Item '{orderedItem.item_name}' does not exist in the database.");
                        }
                    }

                    MessageBox.Show("Order sent successfully!");

                    // Reset the items list and total amount
                    Items.Clear();
                    TotalAmount = 0.0;
                    TotalAmountTextBlock.Text = TotalAmount.ToString("C");

                    // Open the TablePage
                    TablePage tablePage = new TablePage();
                    tablePage.Show();
                    this.Close();
                }
                catch (MySqlException ex)
                {
                    // Handle MySQL-specific exceptions
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    MessageBox.Show("Error sending order: " + ex.ToString());
                }
            }
        }
    }
}

