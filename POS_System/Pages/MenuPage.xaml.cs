using MySql.Data.MySqlClient;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for MenuPage.xaml
    /// </summary>
    public partial class MenuPage : Window
    {
        public MenuPage()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Loaded += Window_Loaded; // Subscribe to the Loaded event
            
        }

        public MenuPage(string tableNumber, string Type) : this()
        {
            TableNumberTextBox.Text = tableNumber;
            TypeTextBox.Text = Type;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryData(); // Call LoadCategoryData when the window is loaded           
            /*LoadItemsData(); // Call LoadFoodData when the window is loaded*/
        }

        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<Category> Category { get; set; } = new ObservableCollection<Category> ();

        private void LoadCategoryData()
        {
            /*ItemButtonPanel.Children.Clear();*/
            string connectString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection mySqlConnection = new MySqlConnection(connectString);

            try
            {
                mySqlConnection.Open();

                // Your query to fetch items
                string sql = "SELECT * FROM category;";
                MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    // Create an Item object for each item in database
                    Category category = new Category()
                    {
                        Id = Convert.ToInt32(rdr["category_id"]),
                        Name = rdr["category_name"].ToString(),

                    };

                    /*                    // Add item to Items collection
                                        Items.Add(item);*/

                    // Creating a new button for each item in database
                    Button newCategoryButton = new Button();
                    newCategoryButton.Content = rdr["category_name"].ToString(); // Set the text of the button to the item name
                    newCategoryButton.Tag = category;
                   /* newCategoryButton.Click += CategoryClick; // Assign a click event handler*/
                    newCategoryButton.Click += (sender, e) => LoadItemsByCategory(newCategoryButton.Content.ToString());
                    newCategoryButton.Width = 150; // Set other properties as needed
                    newCategoryButton.Height = 30;
                    newCategoryButton.Margin = new Thickness(5);
                    SetButtonStyle(newCategoryButton);

                    // Add the new button to a container on your window
                    // For example, a StackPanel with the name 'buttonPanel'
                    CategoryButtonPanel.Children.Add(newCategoryButton);
                }

                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            mySqlConnection.Close();
        }



        private void LoadItemsData()
        {
            
            // Your connection string here
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();

                // Your query to fetch items
                string sql = "SELECT * FROM item;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    // Create an Item object for each item in database
                    Item item = new Item()
                    {
                        Id = Convert.ToInt32(rdr["item_id"]),
                        Name = rdr["item_name"].ToString(),
                        Price = Convert.ToDouble(rdr["item_price"]),
                        Description = rdr["item_description"].ToString(),
                        Category = rdr["item_category"].ToString()
                    };

/*                    // Add item to Items collection
                    Items.Add(item);*/

                    // Creating a new button for each item in database
                    Button newItemButton = new Button();
                    newItemButton.Content = rdr["item_name"].ToString(); // Set the text of the button to the item name
                    newItemButton.Tag = item;
                    newItemButton.Click += ItemClick; // Assign a click event handler
                    newItemButton.Width = 150; // Set other properties as needed
                    newItemButton.Height = 30;
                    newItemButton.Margin = new Thickness(5);

                    // Add the new button to a container on your window
                    // For example, a StackPanel with the name 'buttonPanel'
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

        private void LoadItemsByCategory(object categoryName)
        {
            ItemButtonPanel.Children.Clear();
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
                string sql = "SELECT * FROM item WHERE item_category = @category;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@category", categoryName);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {

                    Item item = new Item()
                    {
                        Id = Convert.ToInt32(rdr["item_id"]),
                        Name = rdr["item_name"].ToString(),
                        Price = Convert.ToDouble(rdr["item_price"]),
                        Description = rdr["item_description"].ToString(),
                        Category = rdr["item_category"].ToString()
                    };

                    Button newItemButton = new Button();
                    newItemButton.Content = rdr["item_name"].ToString();
                    newItemButton.Tag = item;
                    newItemButton.Width = 150;
                    newItemButton.Height = 60;
                    SetButtonStyle(newItemButton);
                    newItemButton.Click += ItemClick;
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

/*        private void CategoryClick(object sender, RoutedEventArgs e)
        {
            ItemButtonPanel.Children.Clear();
            string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
            MySqlConnection conn = new MySqlConnection(connStr);

            try
            {
                conn.Open();
                string sql = "SELECT item_name FROM item WHERE item_category = @category;";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@category", categoryName);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Button newButton = new Button();
                    newButton.Content = rdr["item_name"].ToString();
                    newButton.Width = 150;
                    newButton.Height = 60;
                    SetButtonStyle(newButton);
                    newButton.Click += ItemClick;
                    ItemButtonPanel.Children.Add(newButton);
                }

                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }*/

        /*Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is Category)
            {
                Category category = (Category)clickedButton.Tag as Category;

                if (category != null)
                {
                    Category.Add(category);

                    if (category.Name == Item.)

                }
            }*/

        private void SetButtonStyle(Button button)
        {
            button.FontFamily = new FontFamily("Verdana");
            button.FontSize = 20;
            button.Background = Brushes.Orange;
            button.FontWeight = FontWeights.Bold;
            button.BorderBrush = Brushes.Orange;
            button.Margin = new Thickness(5);
        }

        private double TotalAmount = 0.0;

        private void ItemClick(object sender, RoutedEventArgs e)
        {
           
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is Item)
            {
                Item item = (Item)clickedButton.Tag as Item;

                if (item != null)
                {
                    Items.Add(item);
                    
                    TotalAmount += item.Price;

                    TotalAmountTextBlock.Text = TotalAmount.ToString("C");
                }
            }
        }






        private void Back_to_TablePage(object sender, RoutedEventArgs e)
        {
            // Go to TablePage.xaml when they click on Back button
            TablePage tablePage = new TablePage();
            tablePage.Show();
            this.Close();

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Change number to correspoding table.
            TablePage tablePage = new TablePage();

        }

        private void PaymentButton(object sender, RoutedEventArgs e)
        {
            PaymentPage paymentPage = new PaymentPage();
            paymentPage.Show();
            this.Close();

        }

        private void OrderTypeTextBox(object sender, TextChangedEventArgs e)
        {
            TablePage tablePage = new TablePage();

        }

        private void VoidButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersListBox.SelectedItem is OrderedItem selectedOrderedItem)
            {
                // Remove the selected ordered item from the ObservableCollection
                OrdersListBox.Items.Remove(selectedOrderedItem);

                // Subtract the item price from the total amount
                TotalAmount -= selectedOrderedItem.ItemPrice;
                TotalAmountTextBlock.Text = TotalAmount.ToString("C");
            }
            else
            {
                MessageBox.Show("Please select an item to void.");
            }
        }
    }
}

