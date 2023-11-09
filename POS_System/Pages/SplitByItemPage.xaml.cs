using MySql.Data.MySqlClient;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace POS_System.Pages
{
    public partial class SplitByItemPage : Window
    {
        private List<SplitBill> splitOrders = new List<SplitBill>();
        private int orderID;
        private int currentCustomerId;

        public SplitByItemPage(ObservableCollection<OrderedItem> orderedItems)
        {
            InitializeComponent();

            // Set the ItemsSource of fullListItems
            fullListItems.ItemsSource = orderedItems;

            // Set the ItemsSource of splitOrderedItems
            splitOrderedItems.ItemsSource = new ObservableCollection<OrderedItem>();

            // Initialize currentCustomerId to 1
            currentCustomerId = 1;
        }


        public SplitByItemPage(int orderId)
        {
            InitializeComponent();
            this.orderID = orderId;
        }

        public SplitByItemPage(SharedData sharedData)
        {
            InitializeComponent();

            DataContext = sharedData;

            // Your SplitByItemPage code
        }

        public void PopulateFullListItems(List<OrderedItem> items)
        {
            fullListItems.ItemsSource = items;
        }

        private void addItem_Button(object sender, RoutedEventArgs e)
        {
            if (fullListItems.SelectedItem != null)
            {
                // Check if there is a selected item in the first ListView
                OrderedItem selectedItem = (OrderedItem)fullListItems.SelectedItem;

                if (selectedItem != null)
                {
                    // Remove the selected item from the first ListView
                    (fullListItems.ItemsSource as ObservableCollection<OrderedItem>).Remove(selectedItem);

                    // Add the selected item to the second ListView
                    (splitOrderedItems.ItemsSource as ObservableCollection<OrderedItem>).Add(selectedItem);


                    // Refresh the ListViews
                    fullListItems.Items.Refresh();
                    splitOrderedItems.Items.Refresh();
                }
            }
        }


        private void removeItem_Button(object sender, RoutedEventArgs e)
        {
            if (splitOrderedItems.SelectedItem != null)
            {
                OrderedItem selectedItem = (OrderedItem)splitOrderedItems.SelectedItem;

                if (selectedItem != null)
                {
                    // Remove the selected item from the second ListView
                    (splitOrderedItems.ItemsSource as ObservableCollection<OrderedItem>).Remove(selectedItem);

                    // Add the selected item back to the first ListView
                    (fullListItems.ItemsSource as ObservableCollection<OrderedItem>).Add(selectedItem);

                    // Refresh the ListViews
                    splitOrderedItems.Items.Refresh();
                    fullListItems.Items.Refresh();
                }
            }
        }

        private void splitBill_Button(object sender, RoutedEventArgs e)
        {
            if (splitOrderedItems.Items.Count > 0)
            {
                var selectedItems = (splitOrderedItems.ItemsSource as ObservableCollection<OrderedItem>);

                // Group items by customer
                var groupedItems = selectedItems.GroupBy(item => item.CustomerId);

                foreach (var group in groupedItems)
                {
                    // Calculate the total for the customer
                    decimal total = (decimal)group.Sum(item => item.ItemPrice);

                    // Create a message to display
                    string message = $"Customer {currentCustomerId} has the following items:\n";
                    foreach (var item in group)
                    {
                        message += $"- {item.item_name}\n";
                    }
                    message += $"\nTotal: {total:C}";

                    // Show a message box with the information
                    MessageBox.Show(message, "Customer Items");

                    // Add the customer and items information to the ListBox
                    AddCustomerItemsToListBox(currentCustomerId, group.Select(item => item.item_name).ToList());

                    // Clear the items from the second ListView
                    foreach (var item in group.ToList())
                    {
                        selectedItems.Remove(item);
                    }

                    // Increment the customer ID for the next customer
                    currentCustomerId++;
                }
            }
        }

        private void AddCustomerItemsToListBox(int customerId, List<string> items)
        {
            var customerLabel = $"Customer {customerId}";
            var customerData = new CustomerData { CustomerLabel = customerLabel, Items = items };
            customerItemsListBox.Items.Add(customerData);
        }

        public class CustomerData
        {
            public string CustomerLabel { get; set; }
            public List<string> Items { get; set; }
        }


        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            // ... your other code ...

            // Group the SplitBill items by CustomerId
            var groupedSplitBills = splitOrders.GroupBy(item => item.CustomerId);

            // Create a collection to hold the grouped data
            var groupedOrders = new ObservableCollection<OrderedItem>();

            // Iterate through the groups and add OrderedItem objects
            foreach (var group in groupedSplitBills)
            {
                foreach (var item in group)
                {
                    // Create OrderedItem objects from the SplitBill items
                    var orderedItem = new OrderedItem
                    {
                        item_name = item.ItemName,
                        ItemPrice = item.Price,
                        // Set other properties as needed
                        CustomerId = item.CustomerId,
                    };

                    groupedOrders.Add(orderedItem);
                }
            }

            // Find the open MenuPage (assuming there's only one MenuPage open)
            var menuPage = Application.Current.Windows.OfType<MenuPage>().FirstOrDefault();

            // Update the OrdersListBox.ItemsSource in the existing MenuPage
            menuPage?.UpdateOrders(groupedOrders);

            // Close the SplitByItemPage
            this.Close();
        }




    }
}
