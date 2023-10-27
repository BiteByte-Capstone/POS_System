using MySql.Data.MySqlClient;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace POS_System.Pages
{
    public partial class ManageMenu : Window
    {
        //categories
        private ObservableCollection<Category> categories = new ObservableCollection<Category>();
        //new order
        private ObservableCollection<Item> items = new ObservableCollection<Item>();

        string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

        public ManageMenu()
        {
            InitializeComponent();
            
            

        }
  

        private void ShowItem_Click(object sender, RoutedEventArgs e)
        {
            itemCategoryDataGrid.ContentTemplate = (DataTemplate)this.Resources["ItemTemplate"];
            itemCategoryDataGrid.Content = GetAllItems();


        }

        private void ShowCategory_Click(object sender, RoutedEventArgs e)
        {
            itemCategoryDataGrid.ContentTemplate = (DataTemplate)this.Resources["CategoryTemplate"];
            itemCategoryDataGrid.Content = GetAllCategories();
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

        private void DeleteButton_Click(Object sender, RoutedEventArgs e)
        {

        }
        private void EditButton_Click(Object sender, RoutedEventArgs e)
        {

        }

        //Method: To get all item from database
        private ObservableCollection<Item> GetAllItems()
        {
            items = new ObservableCollection<Item>();

            using (var connection = new MySqlConnection(connectionString))
            {
                var cmd = new MySqlCommand("SELECT * FROM item ORDER BY 1", connection);
                try
                {
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new Item
                            {
                                Id = Convert.ToInt32(reader["item_id"]),
                                item_name = reader["item_name"].ToString(),
                                ItemPrice = Convert.ToDouble(reader["item_price"]),
                                Description = reader["item_description"].ToString(),
                                Category = reader["item_category"].ToString()
                                
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }
            }
            return items;
        }


        private ObservableCollection<Category> GetAllCategories()
        {
            categories = new ObservableCollection<Category>();

            using (var connection = new MySqlConnection(connectionString))
            {
                var cmd = new MySqlCommand("SELECT * FROM category ORDER BY 1", connection);
                try
                {
                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Category
                            {
                                Id = Convert.ToInt32(reader["category_id"]),
                                Name = reader["category_name"].ToString()
                                

                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }
            }
            return categories;
        }

        /*        private void LoadCategoryData()
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
                            newCategoryButton.Margin = new Thickness(5);
                            SetButtonStyle(newCategoryButton);

                            CategoryButtonPanel.Children.Add(newCategoryButton);
                        }

                        rdr.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    conn.Close();
                }*/



        /*        private void LoadItemsByCategory(string categoryName)
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
                }*/

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

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}