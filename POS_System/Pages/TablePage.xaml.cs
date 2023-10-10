using MySql.Data.MySqlClient;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
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

    public partial class TablePage : Window
    {
        private long _currentOrderId;
        string _tableNumber;
        int unpaidTableNumber = 0;
        public TablePage()
        {
           
            InitializeComponent();
            UpdateTableColors();
            /*LoadDatabaseByUnpaid();*/
        }

        public TablePage(string tableNumber, long orderId) : this()
        {
            _currentOrderId = orderId;
            _tableNumber=tableNumber;
            UpdateButtonAfterPlaceOrder(tableNumber);
        }

        private void UpdateButtonAfterPlaceOrder(string tableNumber)
        {
            string buttonName = "table_" + tableNumber;
            Button tableButton = this.FindName(buttonName) as Button;
            if (tableButton != null)
            {
                // For instance, set the Tag to the orderId
                tableButton.Tag = _currentOrderId;

                
                // You can also make other changes, e.g., update the button's content, color, etc.
                // tableButton.Content = "Some new content";
                tableButton.Background = Brushes.Red;
            }
        }


        private void LoadDatabase()
        {

        }
        //Handle table number, order number, order type
        private void Open_Table(object sender, RoutedEventArgs e)
        {
            String type = "";
            Button button = sender as Button;
            if (button != null)
            {
                string tableName = button.Name; // get the name of the button
                string orderType = button.Name; //get the name of button


                int index = tableName.IndexOf('_'); //get the index number after "_"

                // Show table number or take-our order number
                string tableNumber = tableName.Substring(index + 1);// remove the first 5 characters ("table")
                orderType = tableName.Substring(0, index);


                
                if (orderType.Equals("table"))
                {
                    type = "Dine-In";
                }
                else if (orderType.Equals("takeOut"))
                {
                    type = "Take-Out";
                }

                bool hasUnpaidOrders = true;
                string Occrupied = "o";

                MenuPage GetIntoTable = new MenuPage(tableNumber, type);
                /*MenuPage menuPage = new MenuPage(tableNumber, Type, Occrupied, hasUnpaidOrders); // pass the table number as a string*/
                GetIntoTable.Show();
                this.Close();
            }else if (button != null && button.Tag != null)
            {
                MenuPage CheckTable = new MenuPage(_currentOrderId, _tableNumber);
                CheckTable.Show();
                this.Close();
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
                    string checkPaidStatusSql = "SELECT * FROM `order` WHERE paid = 'n';";
                    MySqlCommand cmd = new MySqlCommand(checkPaidStatusSql, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Order order = new Order()
                        {
                            Id = reader.GetInt32(0),
                            tableNumber = reader.GetInt32(1),
                            timeStamp = reader.GetDateTime(2),
                            price = reader.GetDouble(3),
                            IsPaid = reader.GetString(4),


                        };
                        // Get the table number from the query result
                        unpaidTableNumber = order.tableNumber;
                        
                        //hardcode status. since always "O"
                        String status = "Occupied";
                        String type = "Dine-in";

                        // Find the corresponding table UI element in your XAML
                        string buttonName = "table_" + unpaidTableNumber;

                        // Try to find the button by name
                        Button tableButton = FindName(buttonName) as Button;

                        if (tableButton != null)
                        {
                            // Change the background color to green
                            tableButton.Background = Brushes.Green;
                            MenuPage SendUnpaidTableInfo = new MenuPage(order.Id, unpaidTableNumber, type, order.timeStamp, order.price, order.Paid, status);
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



        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Perform logout actions here
            // For example, you can close the current window and navigate back to the login screen
            LoginScreen loginScreen = new LoginScreen();
            loginScreen.Show();
            this.Close();
        }


        /*        private void LoadDatabaseByUnpaid()
                {
                    string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    {
                        try
                        {
                            conn.Open();
                            string unpaidOrderSql = "SELECT table_num FROM `order` WHERE paid = 'n';";
                            MySqlCommand cmd = new MySqlCommand(unpaidOrderSql, conn);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                        }
                        catch { }*/



        /*        private void getTableInfo()
                {
                    string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    {
                        try
                        {
                            conn.Open();
                            string sql = "SELECT * FROM order ";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            *//*cmd.Parameters.AddWithValue("@category", categoryName);*//* // read from category button just ref.
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            while (rdr.Read())
                            {

                                Order order = new Order()
                                {
                                    Id = Convert.ToInt32(rdr["order_id"]),
                                    *//*                                    public int tableNumber { get; set; }
                                            public DateTime timeStamp { get; set; }
                                            public double price { get; set; }*//*
                                    tableNumber = Convert.ToInt16(rdr["table_num"]),
                                    *//*timeStamp = Convert.ToDouble(rdr["order_timestamp"]),*//* // work on it later for TIME!!
                                    price = Convert.ToDouble(rdr["item_description"]),
                                    IsPaid = rdr["item_category"].ToString()
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

                }*/

    }
}
