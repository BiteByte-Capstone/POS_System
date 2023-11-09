using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using POS_System.Models;

namespace POS_System.Pages
{
    public partial class RefundReport : Window
    {
        private List<RefundItem> refundItems; // Assuming RefundItem is your data model class

        public RefundReport()
        {
            InitializeComponent();
            refundItems = GetDataRefundTable();
            UpdateDataGrid(refundItems);
        }

        private List<RefundItem> GetDataRefundTable()
        {
            // Retrieve refund data from your data source, you can replace this with your data retrieval logic
            // For demonstration purposes, I'll create a sample list of RefundItem objects
            List<RefundItem> refundItems = new List<RefundItem>
            {
                new RefundItem { OrderId = 1, ItemId = 101, Name = "Item 1", Quantity = 2, Price = 10.0, Category = "Category A", Date = DateTime.Now },
                new RefundItem { OrderId = 2, ItemId = 102, Name = "Item 2", Quantity = 3, Price = 15.0, Category = "Category B", Date = DateTime.Now },
                // Add more refund items here
            };

            return refundItems;
        }

        private void UpdateDataGrid(List<RefundItem> filteredItems)
        {
            // Bind the filtered data to the DataGrid
            RefundReportGrid.ItemsSource = filteredItems;
        }

        private void FilterData()
        {
            // Apply filters using LINQ based on the filter criteria

            var filteredItems = refundItems;

            if (!string.IsNullOrWhiteSpace(orderIdBoxFilter.Text))
            {
                int orderId = int.Parse(orderIdBoxFilter.Text);
                filteredItems = filteredItems.Where(item => item.OrderId == orderId).ToList();
            }

            if (!string.IsNullOrWhiteSpace(itemIdBoxFilter.Text))
            {
                int itemId = int.Parse(itemIdBoxFilter.Text);
                filteredItems = filteredItems.Where(item => item.ItemId == itemId).ToList();
            }

            if (!string.IsNullOrWhiteSpace(categoryBoxFilter.Text))
            {
                string category = categoryBoxFilter.Text;
                filteredItems = filteredItems.Where(item => item.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Add more filter criteria as needed

            UpdateDataGrid(filteredItems);
        }

        private void filterBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterData();
        }

        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            // Add your print logic here, you can use a reporting library or framework to generate and print the refund report
            MessageBox.Show("Printing the refund report...");
        }
    }
}
