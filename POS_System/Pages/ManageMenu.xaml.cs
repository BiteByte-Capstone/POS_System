using MySql.Data.MySqlClient;
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
using POS_System; // Replace with the actual namespace


namespace POS_System.Pages
{
    public partial class ManageMenu : Window
    {
        //categories
        private ObservableCollection<Category> categories = new ObservableCollection<Category>();
        //new order
        private ObservableCollection<Item> items = new ObservableCollection<Item>();

        string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        //Thevagi
        public bool IsAddCategoryButtonVisible { get; set; }

        public ManageMenu()
        {
            InitializeComponent();

            // Set the "Add Category" button's visibility to Collapsed initially THEVAGI
            DataContext = this; // Set the DataContext to this window
            IsAddCategoryButtonVisible = false; // Initially hide the button
            LoadCategoryData();
            InitializeMenuItems(); // Load menu items (you should implement this method)
        }


        private void InitializeMenuItems()
        {

        }

        private void ShowItem_Click(object sender, RoutedEventArgs e)
        {
            DataGrid.ItemsSource = GetAllItem().DefaultView;
        }

        private void ShowCategory_Click(object sender, RoutedEventArgs e)
        {
            DataGrid.ItemsSource = GetAllCategory().DefaultView;
            //Thevagi -
            // Make the "Add Category" button visible
            IsAddCategoryButtonVisible = true;
        }

        private void ViewSalesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AdminManagement adminManagement = new AdminManagement();
            adminManagement.Show();
            this.Close();
        }

        //Method: To get all item from database
        private DataTable GetAllItem()
        {
            string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("select * from item order by 1", connection);

            DataTable dt = new DataTable();

            try
            {
                connection.Open();
                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging)
            }
            finally
            {
                // Ensure that the connection is always closed, even if an error occurs
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return dt;
        }

        private DataTable GetAllCategory()
        {
            
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("select * from category order by 1", connection);

            DataTable dt = new DataTable();

            try
            {
                connection.Open();
                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., logging)
            }
            finally
            {
                // Ensure that the connection is always closed, even if an error occurs
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return dt;
        }
        //new Thevagi
        private void LoadCategoryData()
        {
            MySqlConnection conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();
                string sql = "SELECT * FROM category;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Category category = new Category
                    {
                        Id = Convert.ToInt32(rdr["category_id"]),
                        Name = rdr["category_name"].ToString(),
                    };

                    categories.Add(category);

                    Button newCategoryButton = new Button();
                    newCategoryButton.Content = rdr["category_name"].ToString();
                    newCategoryButton.Tag = category;
                    newCategoryButton.Click += (sender, e) => LoadItemsByCategory(newCategoryButton.Content.ToString());
                    newCategoryButton.Width = 150;
                    newCategoryButton.Height = 60;
                    SetButtonStyle(newCategoryButton);

                    CategoryButtonPanel.Children.Add(newCategoryButton);
                }

                rdr.Close();
               //  Make the "Add Category" button visible
                AddCategoryButton.Visibility = Visibility.Visible;
                // Add "Add Category" button
                Button addCategoryButton = new Button();
              addCategoryButton.Content = "Add Category";
                addCategoryButton.Width = 150;
                addCategoryButton.Height = 60;
               addCategoryButton.Click += AddCategoryButton_Click; // Add a handler for this button
                SetButtonStyle(addCategoryButton);
               CategoryButtonPanel.Children.Add(addCategoryButton);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }

        // Add Category button click event handler
        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the dialog to input the new category name
            var addCategoryDialog = new AddCategoryDialog();
            if (addCategoryDialog.ShowDialog() == true)
            {
                // Retrieve the category name from the dialog
                string categoryName = addCategoryDialog.CategoryName;

                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    // Insert the new category into your database
                    if (InsertCategoryIntoDatabase(categoryName))
                    {
                       /* // Reload the category data
                        LoadCategoryData();*/
                    }
                }
                else
                {
                    MessageBox.Show("Category name cannot be empty.");
                }
            }
        }


        private bool InsertCategoryIntoDatabase(string categoryName)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert the new category into your database here
                    string insertQuery = "INSERT INTO category (category_name) VALUES (@categoryName)";
                    MySqlCommand cmd = new MySqlCommand(insertQuery, connection);
                    cmd.Parameters.AddWithValue("@categoryName", categoryName);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while adding the category: " + ex.Message);
                return false;
            }
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected category from the DataGrid
            if (DataGrid.SelectedItem is DataRowView rowView)
            {
                // Access the category name from the selected row
                string categoryName = rowView["category_name"].ToString();

                // Open the EditCategoryDialog to edit the category name
                var editCategoryDialog = new EditCategoryDialog(categoryName);
                if (editCategoryDialog.ShowDialog() == true)
                {
                    string editedCategoryName = editCategoryDialog.EditedCategoryName;
                    // Handle the edited category name here
                }
                else
                {
                    Console.WriteLine("EditCategoryDialog was canceled.");
                }
            }
            else
            {
                Console.WriteLine("No item selected in DataGrid.");
            }
        }





        //end Thevagi

        private void LoadItemsByCategory(string categoryName)
        {
            ItemButtonPanel.Children.Clear();

            MySqlConnection conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();
                string sql = "SELECT * FROM item WHERE item_category = @category;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@category", categoryName);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Item item = new Item
                    {
                        Id = Convert.ToInt32(rdr["item_id"]),
                        item_name = rdr["item_name"].ToString(),
                        ItemPrice = Convert.ToDouble(rdr["item_price"]),
                        Description = rdr["item_description"].ToString(),
                        Category = rdr["item_category"].ToString()
                    };

                    Button newItemButton = new Button();
                    newItemButton.Content = rdr["item_name"].ToString();
                    newItemButton.Tag = item;
                    newItemButton.Width = 150;
                    newItemButton.Height = 60;
                    SetButtonStyle(newItemButton);
                    
                    ItemButtonPanel.Children.Add(newItemButton);
                }

                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }

        



        //Thevagi
        // Define BoolToVisibilityConverter in code-behind
        public class BoolToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool && (bool)value)
                {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value is Visibility && (Visibility)value == Visibility.Visible;
            }
        }

        // For Styling
        private void SetButtonStyle(Button button)
        {
            button.FontFamily = new FontFamily("Verdana");
            button.FontSize = 20;
            button.Background = Brushes.Orange;
            button.Foreground = Brushes.Black;
            button.FontWeight = FontWeights.Bold;
            button.BorderBrush = Brushes.Orange;
            button.Padding = new Thickness(10);

            button.Margin = new Thickness(5);
        }

    }
}