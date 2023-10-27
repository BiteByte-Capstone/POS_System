using MySql.Data.MySqlClient;
using POS.Models;
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

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for Refund.xaml
    /// </summary>
    public partial class Refund : Window
    {

        //String to make connection to database
        string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

        public Refund()
        {
            InitializeComponent();
            getDataPaymentTable();
        }

        private void getDataPaymentTable()
        {
            //Create a connection object
            MySqlConnection connection = new MySqlConnection(connectionString);

            //SQL query
            MySqlCommand cmd = new MySqlCommand("select * from pos_db.payment", connection);

            //Open up connection with the user table
            connection.Open();

            //create a datatable object to capture the database table
            DataTable dt = new DataTable();

            //Execute the command and the load the result of reader inside the datatable
            dt.Load(cmd.ExecuteReader());

            //Close connection to user table
            connection.Close();

            //Bind data table to the DataGrid on XAML
            paymentGrid.DataContext = dt;
        }        
        
        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefundBtn_Click(object sender, RoutedEventArgs e)
        {
            string orderId = "0";
            string paymentId = refundPaymentIdBox.Text;
            string refundAmount = refundAmountBox.Text;
            string refundMethod = refundMethodComboBox.Text;
            string refundReason = refundReasonBox.Text;
            string userId = "";

            MessageBox.Show(paymentId + ' ' + refundAmount + ' ' + refundMethod + ' ' + refundReason);
            
            if (paymentId.Length < 1 || refundAmount.Length < 1 || refundMethod.Length < 1 || refundReason.Length < 1)
            {
                MessageBox.Show("Please enter all fields");
            } else
            {
                //Create a connection object
                MySqlConnection connection = new MySqlConnection(connectionString);

                String sqlquery = "insert into pos_db.refund values (0, " + orderId + ", " + paymentId + ", " + refundAmount + ", '" + refundMethod + "', '" + refundReason + "', " + userId + ");";

                MessageBox.Show(sqlquery);

                //SQL query
                MySqlCommand cmd = new MySqlCommand(sqlquery, connection);

                //Open up connection with the user table
                connection.Open();

                cmd.ExecuteReader();

                connection.Close();

                MessageBox.Show("Refund Complete");
            }
        }
    }
}
