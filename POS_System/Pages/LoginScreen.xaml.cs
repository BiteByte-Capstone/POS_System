using POS_System.Database;
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

//import User model
using POS.Models;

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for LoginScreen.xaml
    /// </summary>
    public partial class LoginScreen : Window
    {
        private DatabaseHelper db;

        public LoginScreen()
        {
            InitializeComponent();
            db = new DatabaseHelper("localhost", "pos_db", "root", "password");
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string enteredUserId = id.Text;
            string enteredPassword = password.Password;

            if (db.AuthenticateUser(enteredUserId, enteredPassword))
            {
                string authenticatedUsername = db.GetUsername(enteredUserId);
                MessageBox.Show("Login successful! " + authenticatedUsername);

                // Instantiate a User object --> all pages can now get the static id and name from the User class
                // Would be used to track user activity in the system and activity-log report.

                User user = new User(int.Parse(enteredUserId), authenticatedUsername, "");
                
                // Pass the userId to TablePage when creating an instance
                
                TablePage window2 = new TablePage();
                window2.Show();


                // Close the current login window if needed
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid userid or password. Please try again.");
            }
        }
    }
}
