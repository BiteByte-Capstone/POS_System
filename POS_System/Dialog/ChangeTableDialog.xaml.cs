using MySql.Data.MySqlClient;
using POS_System.Pages;
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

namespace POS_System.Dialog
{
    /// <summary>
    /// Interaction logic for ChangeTableDialog.xaml
    /// </summary>
    public partial class ChangeTableDialog : Window
    {
        private string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        public event EventHandler TableColorUpdated;


        public ChangeTableDialog()
        {
            InitializeComponent();
            

        }


        // OK Button
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Access the selected item from ComboBox
            string selectedToTable = cboToTable.Text;
            string selectedFromTable = cboFromTable.Text;

            // Update the table number in the database
            UpdateTableNumber(selectedFromTable, selectedToTable);

            // Close the dialog
            this.Close();
        }

        private void UpdateTableColor(Button tableButton, Button takeOutButton)
        {
            if (tableButton != null)
            {
                tableButton.Background = Brushes.Green;
            }
            else if (takeOutButton != null)
            {
                takeOutButton.Background = Brushes.Green;
            }

            // Raise the event to notify that the table color has been updated
            OnTableColorUpdated();
        }

        protected virtual void OnTableColorUpdated()
        {
            TableColorUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateTableNumber(string fromTable, string toTable)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();

                    // Update the table number in the `order` table
                    string updateTableNumberSql = "UPDATE `order` SET table_num = @toTable WHERE table_num = @fromTable;";
                    MySqlCommand updateTableNumberCmd = new MySqlCommand(updateTableNumberSql, conn);
                    updateTableNumberCmd.Parameters.AddWithValue("@toTable", toTable);
                    updateTableNumberCmd.Parameters.AddWithValue("@fromTable", fromTable);

                    int rowsAffected = updateTableNumberCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"Table updated from {fromTable} to {toTable}.");

                        // Update the table color for the 'fromTable'
                        UpdateTableColor(FindName("table_" + fromTable) as Button, FindName("takeOut_" + fromTable) as Button);
                        // Update the table color for the 'toTable'
                        UpdateTableColor(FindName("table_" + toTable) as Button, FindName("takeOut_" + toTable) as Button);
                    }
                    else
                    {
                        MessageBox.Show($"Update failed. No matching records for table {fromTable} found.");
                    }
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





        // Cancel Button
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Perform actions here
            this.Close();
        }
    }
}
