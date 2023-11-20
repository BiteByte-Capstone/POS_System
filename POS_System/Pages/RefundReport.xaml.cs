using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace POS_System.Pages
{
    public partial class RefundReport : Window
    {
        private string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

        public RefundReport()
        {
            InitializeComponent();
            GetDataRefundTable();
        }

        private void GetDataRefundTable()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM refund", connection);

                connection.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                connection.Close();

                refundGrid.DataContext = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fromTimestamp = refundTimestampFilter.SelectedDate?.ToString("yyyy-MM-dd");
                string untilTimestamp = refundTimestampFilter.SelectedDate?.ToString("yyyy-MM-dd");
                string specificMethod = refundMethodFilter.Text;

                string specificUser = userIdFilter.Text;

                string sqlQuery = "SELECT * FROM refund WHERE 1=1";

                if (!string.IsNullOrEmpty(fromTimestamp) && !string.IsNullOrEmpty(untilTimestamp))
                {
                    sqlQuery += " AND refund_timestamp BETWEEN @fromTimestamp AND @untilTimestamp";
                }
                else if (!string.IsNullOrEmpty(fromTimestamp))
                {
                    sqlQuery += " AND refund_timestamp >= @fromTimestamp";
                }
                else if (!string.IsNullOrEmpty(untilTimestamp))
                {
                    sqlQuery += " AND refund_timestamp <= @untilTimestamp";
                }

                if (!string.IsNullOrEmpty(specificMethod))
                {
                    sqlQuery += " AND refund_method = @specificMethod";
                }

                if (!string.IsNullOrEmpty(specificUser))
                {
                    sqlQuery += " AND user_id = @specificUser";
                }

                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);

                // Add parameters for user inputs
                cmd.Parameters.AddWithValue("@fromTimestamp", fromTimestamp);
                cmd.Parameters.AddWithValue("@untilTimestamp", untilTimestamp);
                cmd.Parameters.AddWithValue("@specificMethod", specificMethod);
                cmd.Parameters.AddWithValue("@specificUser", specificUser);

                connection.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                connection.Close();

                refundGrid.DataContext = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(refundGrid, "Refund Report");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
