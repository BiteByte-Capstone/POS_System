﻿using System;
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
    public partial class TablePage : Window
    {
        public TablePage()
        {
            InitializeComponent();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Perform logout actions here
            // For example, you can close the current window and navigate back to the login screen
            LoginScreen loginScreen = new LoginScreen();
            loginScreen.Show();
            this.Close();
        }

        private void Open_Table(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                // Assuming the table number is the part of the button's name after "table"
                string tableName = button.Name; // get the name of the button
                string tableNumber = tableName.Substring(5); // remove the first 5 characters ("table")
                MenuPage menuPage = new MenuPage(tableNumber); // pass the table number as a string
                menuPage.Show();
                this.Close();
            }
        }


    }
}
