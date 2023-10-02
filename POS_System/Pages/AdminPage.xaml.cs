

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
using POS_System.Database;

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AdminPage : Window
    {

        private DatabaseHelper db;

        public AdminPage()
        {
            InitializeComponent();
            getAllUser();

            

        }

        private void getAllUser()
        {
            //Tutorial used https://www.youtube.com/watch?v=OPDPI5exPp8

            //db = new DatabaseHelper("localhost", "pos_db", "root", "password");

            //String to make connection to database
            string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            //Create a connection object
            MySqlConnection connection = new MySqlConnection(connectionString);

            //SQL query
            MySqlCommand cmd = new MySqlCommand("select * from user order by 1", connection);

            //Open up connection with the user table
            connection.Open();

            //create a datatable object to capture the database table
            DataTable dt = new DataTable();

            //Execute the command and the load the result of reader inside the datatable
            dt.Load(cmd.ExecuteReader());

            //Close connection to user table
            connection.Close();

            //Bind data table to the DataGrid on XAML
            userGrid.DataContext = dt;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void addUser_Click(object sender, RoutedEventArgs e)
        {

            //Tutorial used https://www.includehelp.com/dot-net/insert-records-into-mysql-database-in-csharp.aspx

            String username = adduser_usernameBox.Text;
            String password = adduser_passwordBox.Text;
            String id = adduser_idBox.Text;

            try
            {

                //String to make connection to database
                string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

                //Create a connection object
                MySqlConnection connection = new MySqlConnection(connectionString);

                //SQL query
                MySqlCommand cmd = new MySqlCommand("insert into user (user_id, user_name, user_password) values (" + id + ",'" + username + "','" + password + "')", connection);

                //Open up connection with the user table
                connection.Open();

                //Execute the command
                cmd.ExecuteNonQuery();

                //Close connection to user table
                connection.Close();
                getAllUser();

                //Clear textboxes after method sucessfully executes
                adduser_usernameBox.Clear();
                adduser_passwordBox.Clear();
                adduser_idBox.Clear();

            } catch (Exception ex)
            {
                MessageBox.Show("ID " + id + " is already in use!");
            }
        }

        private void deleteUser_Click(object sender, RoutedEventArgs e)
        {
            String id = deleteuser_idBox.Text;

            //String to make connection to database
            string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            //Create a connection object
            MySqlConnection connection = new MySqlConnection(connectionString);

            //SQL query
            MySqlCommand cmd = new MySqlCommand("delete from user where user_id = " + id, connection);

            //Open up connection with the user table
            connection.Open();

            //Execute the command
            cmd.ExecuteNonQuery();

            //Close connection to user table
            connection.Close();

            getAllUser();
        }

        private void editUser_Click(object sender, RoutedEventArgs e)
        {
            String username = edituser_usernameBox.Text;
            String password = edituser_passwordBox.Text;
            String id = edituser_idBox.Text;

            //String to make connection to database
            string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            //Create a connection object
            MySqlConnection connection = new MySqlConnection(connectionString);

            //SQL query
            MySqlCommand cmd = new MySqlCommand("update user set user_name = '" + username + "', user_password = '" + password + "' where user_id = " + id, connection);

            //Open up connection with the user table
            connection.Open();

            //Execute the command
            cmd.ExecuteNonQuery();

            //Close connection to user table
            connection.Close();
            getAllUser();

            //Clear textboxes after method sucessfully executes
            edituser_idBox.Clear();
            edituser_passwordBox.Clear();
            edituser_usernameBox.Clear();
        }

        private void edituser_idBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            //Tutorial used https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/how-to-detect-when-text-in-a-textbox-has-changed?view=netframeworkdesktop-4.8

            String id = edituser_idBox.Text;
            
            try 
            {
                MySqlDataReader dr;

                //String to make connection to database
                string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

                //Create a connection object
                MySqlConnection connection = new MySqlConnection(connectionString);

                //SQL query
                MySqlCommand cmd = new MySqlCommand("select * from user where user_id = " + id, connection);

                //Open up connection with the user table
                connection.Open();

                //Execute the command
                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    edituser_usernameBox.Text = dr.GetValue(1).ToString();
                    edituser_passwordBox.Text = dr.GetValue(2).ToString();
                }

                //Close connection to user table
                connection.Close();

            } catch (Exception ex) //This exception is thrown when the edituser_idBox is empty. If the edituser_idBox is empty, then the other edituser boxes should be empty too.
            {
                edituser_usernameBox.Clear();
                edituser_passwordBox.Clear();
            }
        }

        private void adduser_idBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void edituser_idBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void edituser_usernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
