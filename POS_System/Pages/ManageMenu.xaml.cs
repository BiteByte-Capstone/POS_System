using POS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS_System.Pages
{
    public partial class ManageMenu : Window
    {
        private List<MenuItem> menuItems; // This list represents your menu items. Replace it with your actual data structure.

        public ManageMenu()
        {
            InitializeComponent();
            InitializeMenuItems(); // Load menu items (you should implement this method)
        }

        // Initialize the list of menu items (example method, replace with your actual data)
        private void InitializeMenuItems()
        {
            menuItems = new List<MenuItem>
            {
                new MenuItem { Id = 1, Name = "Item 1", Price = 10.99 },
                new MenuItem { Id = 2, Name = "Item 2", Price = 8.99 },
                new MenuItem { Id = 3, Name = "Item 3", Price = 11.99 },

                // Add more menu items as needed
            };

            // Bind the menu items to your ListBox named ItemList
            ItemList.ItemsSource = menuItems; // Assuming you have a ListBox named ItemList
        }

        // Event handler for the "Edit Item" button
        private void EditItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if an item is selected for editing
            if (ItemList.SelectedItem != null)
            {
                // Open a dialog or a new window for editing the selected item
                MenuItem selectedItem = (MenuItem)ItemList.SelectedItem;

               

                // After editing, update the list or reload the menu items
                InitializeMenuItems(); // Replace with your actual data update method
            }
            else
            {
                MessageBox.Show("Please select an item to edit.");
            }
        }

        // Event handler for the "Delete Item" button
        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if an item is selected for deletion
            if (ItemList.SelectedItem != null)
            {
                // Confirm deletion with a dialog
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this item?", "Confirmation", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    // Perform the deletion (you should implement this method)
                    MenuItem selectedItem = (MenuItem)ItemList.SelectedItem;
                    DeleteMenuItem(selectedItem.Id); // Replace with your actual deletion method

                    // Update the list or reload the menu items
                    InitializeMenuItems(); // Replace with your actual data update method
                }
            }
            else
            {
                MessageBox.Show("Please select an item to delete.");
            }
        }

        // Event handler for the "Add Item" button
        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new item with the values from the text boxes
            string itemName = ItemNameTextBox.Text;
            double itemPrice;

            // Check if the price can be parsed
            if (!string.IsNullOrWhiteSpace(itemName) && double.TryParse(ItemPriceTextBox.Text, out itemPrice))
            {
                // Create the new item
                MenuItem newItem = new MenuItem
                {
                    Id = menuItems.Count + 1, // Assign a unique ID, you may need a better method
                    Name = itemName,
                    Price = itemPrice
                };

                // Add the new item to your list
                menuItems.Add(newItem);

                // Refresh the ListBox
                ItemList.ItemsSource = null;
                ItemList.ItemsSource = menuItems;

                // Clear the input text boxes
                ItemNameTextBox.Text = "Enter item name";
                ItemPriceTextBox.Text = "Enter item price";
            }
            else
            {
                MessageBox.Show("Please enter a valid item name and price.");
            }
        }

        // Implement your DeleteMenuItem method here
        private void DeleteMenuItem(int itemId)
        {
            // Implement the logic to delete a menu item with the specified itemId
        }

        // Event handler for the "Cancel" button in the item addition form
        private void CancelAddItemButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the item addition form
            ItemAdditionForm.Visibility = Visibility.Collapsed;
        }

        // Event handler for the "View Sales" button (you can implement this)
        private void ViewSalesButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic to view sales
        }

        // Event handler for the "Settings" button (you can implement this)
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement the logic to open settings
        }

        private void ItemNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "Enter item name")
            {
                textBox.Text = "";
            }
        }

        private void ItemNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Enter item name";
            }
        }

        private void ItemPriceTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "Enter item price")
            {
                textBox.Text = "";
            }
        }

        private void ItemPriceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Enter item price";
            }
        }
    }
    

    // Example class representing a menu item
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }
}
