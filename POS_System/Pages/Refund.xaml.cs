using MySql.Data.MySqlClient;
using POS.Models;
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
        }

        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefundBtn_Click(object sender, RoutedEventArgs e)
        {
            string orderId = {Binding order_id};
            string paymentId = refundPaymentIdBox.Text;
            string refundAmount = refundAmountBox.Text;
            string refundMethod = refundMethodComboBox.Text;
            string refundReason = refundReasonBox.Text;
            string userId = ;

            MessageBox.Show(paymentId + ' ' + refundAmount + ' ' + refundMethod + ' ' + refundReason);
            
            if (paymentId.Length < 1 || refundAmount.Length < 1 || refundMethod.Length < 1 || refundReason.Length < 1)
            {
                MessageBox.Show("Please enter all fields");
            } else
            {
                //Create a connection object
                MySqlConnection connection = new MySqlConnection(connectionString);

                //SQL query
                MySqlCommand cmd = new MySqlCommand("insert into pos_db.refund values " + orderId + ", " + paymentId + ", " + refundAmount + ", " + refundMethod + ", " + refundReason + ", " + userId + ";", connection);

                //Open up connection with the user table
                connection.Open();
            }
        }
    }
}
