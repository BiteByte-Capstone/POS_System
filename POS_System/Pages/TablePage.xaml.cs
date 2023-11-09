using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
using System.Xml.Linq;

//Import User into class
using POS.Models;



namespace POS_System.Pages
{
    public partial class TablePage : Window
    {
        private string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        // Define properties to store table number and order type
        public string TableNumber { get; private set; }
        public string OrderType { get; private set; }

        public string userName { get; private set; }
        public string userId { get; private set; }

        public TablePage()
        {
            InitializeComponent();
            UpdateTableColors();
        }

        public TablePage(string tableNumber, string orderType)
        {
            InitializeComponent();
            UpdateTableColors();
            // Store the table number and order type for future use
            this.TableNumber = tableNumber;
            this.OrderType = orderType;
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Perform logout actions here
            // For example, you can close the current window and navigate back to the login screen
            LoginScreen loginScreen = new LoginScreen();
            loginScreen.Show();
            this.Close();
        }



        // Handle table number, order number, order type
        private void Open_Table(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                string tableName = button.Name;
                string orderType = button.Name;
                int index = tableName.IndexOf('_');
                string tableNumber = tableName.Substring(index + 1);
                orderType = tableName.Substring(0, index);

                string Type = "";
                if (orderType.Equals("table"))
                {
                    Type = "Dine-In";
                }
                else if (orderType.Equals("takeOut"))
                {
                    Type = "Take-Out";
                }

                bool hasUnpaidOrders = CheckForUnpaidOrders(tableNumber);

                // If there are unpaid orders, open the existing order
                if (hasUnpaidOrders)
                {
                    MenuPage menuPage = new MenuPage(tableNumber, Type, hasUnpaidOrders);
                    menuPage.Show();
                }
                else
                {
                    // If no unpaid orders exist, create a new order
                    CreateNewOrder(tableNumber, Type);
                }

                this.Close();
            }
        }

        private void CreateNewOrder(string tableNumber, string orderType)
        {
            // Create a new order
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string createOrderSql = "INSERT INTO `order` (table_num, order_timestamp, total_amount, paid) VALUES (@tableNum, @orderTimestamp, 0, 'n');";
                    MySqlCommand createOrderCmd = new MySqlCommand(createOrderSql, conn);
                    createOrderCmd.Parameters.AddWithValue("@tableNum", tableNumber);
                    createOrderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                    createOrderCmd.ExecuteNonQuery();

                    // Pass the table number, order type, and unpaid orders status to MenuPage
                    MenuPage menuPage = new MenuPage(tableNumber, orderType, true);
                    menuPage.Show();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating a new order: " + ex.ToString());
                }
            }
        }





        // Check if there are unpaid orders for the specified table
        private bool CheckForUnpaidOrders(string tableNumber)
        {
            // Create a connection string
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Check if there are unpaid orders for the specified table
                    string checkUnpaidOrdersSql = "SELECT order_id FROM `order` WHERE table_num = @tableNum AND paid = 'n';";
                    MySqlCommand checkUnpaidOrdersCmd = new MySqlCommand(checkUnpaidOrdersSql, conn);
                    checkUnpaidOrdersCmd.Parameters.AddWithValue("@tableNum", tableNumber);
                    object unpaidOrderId = checkUnpaidOrdersCmd.ExecuteScalar();

                    return (unpaidOrderId != null);
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("MySQL Error: " + ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error checking for unpaid orders: " + ex.ToString());
                    return false;
                }
            }
        }





        // Method to update table colors based on the database
        private void UpdateTableColors()
        {
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";



            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();



                    // Query the database for tables with paid=n
                    string query = "SELECT table_num FROM `order` WHERE paid = 'n';";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();



                    while (reader.Read())
                    {
                        // Get the table number from the query result
                        int tableNumber = reader.GetInt32(0);



                        // Find the corresponding table UI element in your XAML
                        string buttonName = "table_" + tableNumber;
                            


                        // Try to find the button by name
                        Button tableButton = FindName(buttonName) as Button;



                        if (tableButton != null)
                        {
                            // Change the background color to green
                            tableButton.Background = Brushes.Green;
                        }
                    }



                    reader.Close();
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Refund refund = new Refund();
            refund.Show();
            this.Close();
        }
    }
}