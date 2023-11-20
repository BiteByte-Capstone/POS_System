﻿using MySql.Data.MySqlClient;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace POS_System
{
    /// <summary>
    /// Interaction logic for AddCategoryDialog.xaml
    /// </summary>
    public partial class AddCategoryDialog : Window
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // Change 'private set' to 'set'
        

        public AddCategoryDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryId = int.Parse(CategoryIdTextBox.Text);
            // Get the category name from the TextBox
            CategoryName = CategoryNameTextBox.Text;

            // Close the dialog with a "true" result
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the dialog with a "false" result
            DialogResult = false;
            Close();
        }


    }
}
