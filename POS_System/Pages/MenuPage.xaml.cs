using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using POS.Models;
using POS_System.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Printing;
using System.Linq;
using System.Globalization;
using Org.BouncyCastle.Utilities.Collections;
using System.Data.Common;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Data;
using POS.Models;

namespace POS_System.Pages
{
    public partial class MenuPage : Window, INotifyPropertyChanged
    {
        // Define connStr at the class level
        public string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        //categories
        private ObservableCollection<Category> categories = new ObservableCollection<Category>();
        //existing order & !!! it is collect to show on list view
        private ObservableCollection<OrderedItem> orderedItems = new ObservableCollection<OrderedItem>();
        //Splited order 
        private ObservableCollection<OrderedItem> splitOrderedItems = new ObservableCollection<OrderedItem>();
        //List for Back-up oreredItems before split
        private List<OrderedItem> backupOrderedItems = new List<OrderedItem>();

        // Event declaration
        public event PropertyChangedEventHandler PropertyChanged;

        // OnPropertyChanged method to raise the event
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public ObservableCollection<OrderedItem> OrderedItems
        {
            get { return orderedItems; }
            set
            {
                if (orderedItems != value)
                {
                    orderedItems = value;
                    OnPropertyChanged(nameof(OrderedItems));
                }
            }
        }


        private string _tableNumber;
        private string _orderType;
        private string _status;
        private bool _hasPaidOrders;
        private int _numberOfBill; //if more than 0, mean splitted order
        private string _splitType = "No Split";

        private double TotalAmount = 0.0;
        private int OriginalItemsCount = 0;
        private bool itemClick = false; //for condition for empty list (save button)
        private bool isSplited = false;


        //Constructor 
        public MenuPage()
        {
            InitializeComponent();

            //it could load the page before show up
            this.DataContext = this;
            this.Loaded += Window_Loaded; // Subscribe to the Loaded event

        }

        public MenuPage(string tableNumber, string orderType, string status, bool hasUnpaidOrders) : this()
        {
            TableNumberTextBox.Text = tableNumber;
            TypeTextBox.Text = orderType;
            StatusTextBlock.Text = status;


            _tableNumber = tableNumber;
            _orderType = orderType;
            _status = status;
            _hasPaidOrders = hasUnpaidOrders;
            MenuLabel.Content = $"Menu   -   {_orderType}";

            if (_orderType == "Take-Out")
            {
                TableNumberTextBlock.Text = "     Take-Out# : ";
                SplitBillButton.IsEnabled = false;
                SplitItemButton.IsEnabled = false;
            }

            if (hasUnpaidOrders)
            {
                LoadUnpaidOrders(tableNumber);
            }
        }

        //Method for loading when Menu Page open
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategoryData();

        }

        //Method: Group List by customer id
        private void GroupItemList()
        {

            OrdersListView.Items.GroupDescriptions.Clear();
            var property = "FormattedCustomerID";
            OrdersListView.Items.GroupDescriptions.Add(new PropertyGroupDescription(property));

        }

        //Method for refresh page: update UI after change button.
        private void Refresh()
        {

            TotalAmount = 0;
            GroupItemList();


        }



        private void LoadUnpaidOrders(string tableNumber)
        {
            OrdersListView.Items.GroupDescriptions.Clear();
            orderedItems.Clear();
            TotalAmount = 0;
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    long orderId = GetOrderId(tableNumber);
                    string unpaidOrdersSql = "SELECT o.order_id, o.item_id, o.quantity, o.item_price, o.original_item_price,o.customer_id, i.item_name, i.item_description FROM ordered_itemlist o JOIN item i ON o.item_id = i.item_id WHERE o.order_id = @orderId;";
                    MySqlCommand unpaidOrdersCmd = new MySqlCommand(unpaidOrdersSql, conn);
                    unpaidOrdersCmd.Parameters.AddWithValue("@orderId", orderId);
                    MySqlDataAdapter dataAdapter = new MySqlDataAdapter(unpaidOrdersCmd);
                    DataTable unpaidOrdersTable = new DataTable();
                    dataAdapter.Fill(unpaidOrdersTable);

                        if (unpaidOrdersTable.Rows.Count > 0)
                        {

                            OrderIdTextBlock.Text = orderId.ToString();
                        }
                        else if (unpaidOrdersTable.Rows.Count == 0)
                        {

                            StatusTextBlock.Text = "Deleted all saved order before";
                            OrderIdTextBlock.Text = orderId.ToString();
                        }

                        foreach (DataRow row in unpaidOrdersTable.Rows)
                        {
                            OrderedItem orderedItem = new OrderedItem
                            {
                                order_id = Convert.ToInt32(row["order_id"]),
                                item_id = Convert.ToInt32(row["item_id"]),
                                item_name = row["item_name"].ToString(),
                                Quantity = Convert.ToInt32(row["quantity"]),
                                ItemPrice = Convert.ToDouble(row["item_price"]),
                                origialItemPrice = Convert.ToDouble(row["original_item_price"]),
                                IsSavedItem = true,
                                customerID = Convert.ToInt32(row["customer_id"])
                            };
                            OriginalItemsCount++;
                            _numberOfBill = orderedItem.customerID;


                            if (orderedItem.customerID > 0)
                            {
                                isSplited = true;
                            }
                            else if (orderedItem.customerID == 0)
                            {
                                isSplited = false;
                            }
                            orderedItems.Add(orderedItem);
                            BackupOrderedItemCollection();

                            TotalAmount += orderedItem.ItemPrice;
                        }
                        TotalAmountTextBlock.Text = "Total Amount :             " + TotalAmount.ToString("C", new CultureInfo("en-CA"));

                        if (isSplited == true)
                        {
                            GroupItemList();
                        }


                    


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading unpaid orders: " + ex.ToString());
                }
            
                    
                
            }
        }




        private void LoadCategoryData()
        {

            MySqlConnection conn = new MySqlConnection(connStr);

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
                    newCategoryButton.Width = 120;
                    newCategoryButton.Height = 50;
                    newCategoryButton.FontSize = 15;
                    newCategoryButton.Background = Brushes.DarkOrange;

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
        }



        private void LoadItemsByCategory(string categoryName)
        {
            ItemButtonPanel.Children.Clear();

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
                    Item item = new Item
                    {
                        Id = Convert.ToInt32(rdr["item_id"]),
                        item_name = rdr["item_name"].ToString(),
                        ItemPrice = Convert.ToDouble(rdr["item_price"]),
                        Description = rdr["item_description"].ToString(),
                        Category = rdr["item_category"].ToString()
                    };

                    Button newItemButton = new Button();
                    // Create a TextBlock for the button content
                    TextBlock textBlock = new TextBlock
                    {
                        Text = rdr["item_name"].ToString(),
                        TextWrapping = TextWrapping.Wrap, // Enable text wrapping
                        TextAlignment = TextAlignment.Center
                    };

                    newItemButton.Content = textBlock;
                    newItemButton.Tag = item;
                    newItemButton.Width = 120;
                    newItemButton.Height = 80;
                    newItemButton.FontSize = 15;
                    newItemButton.Background = Brushes.LightGoldenrodYellow;


                    SetButtonStyle(newItemButton);
                    newItemButton.Click += ItemButton_Click;
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





        //add item on list box
        private void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            itemClick = true;
            
            Button clickedButton = sender as Button;
            if (clickedButton != null && clickedButton.Tag is Item)
            {
                Item item = clickedButton.Tag as Item;

                if (item != null)
                { 
                    AddItemToOrder(item);
                }
            }
        }

        //(edit item list) Add new item to the existing list
        private void AddItemToOrder(Item item)
        {
            // Convert Item to OrderedItem
            OrderedItem orderedItem = new OrderedItem
            {
                item_id = item.Id,
                item_name = item.item_name,
                Quantity = 1, 
                origialItemPrice = item.ItemPrice,
                ItemPrice = item.ItemPrice,
                IsSavedItem = false,
                customerID = 0
            };

            orderedItems.Add(orderedItem);
            
            TotalAmount += orderedItem.ItemPrice;
            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            TotalAmountTextBlock.Text = "Total Amount :             " + TotalAmount.ToString("C", cultureInfo);
        }

        //(Button for Split Bill)
        private void SplitBillButton_Click(object sender, RoutedEventArgs e)
        {
            if (_numberOfBill > 0)
            {
                MessageBox.Show("The order is splitted. \nPlease reset before split again!");
                return;
            }
            else if (IsSavedItem() == false)
            {
                MessageBox.Show("Please save first. \n\nSo, The order can be splitted.");
                return;
            }
            else if (_numberOfBill == 0)
            {
                SplitBillDialog splitBillDialog = new SplitBillDialog(orderedItems, TotalAmount);

                if (splitBillDialog.ShowDialog() == true)
                {
                    _numberOfBill = splitBillDialog.NumberOfPeople;
                    _splitType = "ByBill";
                    BackupOrderedItemCollection();
                    GetNewSplitItemList(orderedItems, _numberOfBill, _splitType);
                    Refresh();
                    MessageBox.Show($"Splited bill into {_numberOfBill}");

                }
                else
                {
                    return;
                }
            }


        }

        //(Button) split by item 
        private void SplitbyItem_Click(object sender, RoutedEventArgs e)
        {
            if (_numberOfBill > 0)
            {
                MessageBox.Show("The order is splitted. \nPlease reset before split again!");
                return;
            }
            else if (IsSavedItem() == false)
            {
                MessageBox.Show("Please save first. \n\nSo, The order can be splitted.");
                return;
            }
            else if (_numberOfBill == 0)
            {
                SplitByItemPage splitByItemPage = new SplitByItemPage(orderedItems);
                BackupOrderedItemCollection();

                if (splitByItemPage.ShowDialog() == true)
                {
                    var splitOrderedItems = splitByItemPage._assignCustomerIDItems;
                    _splitType = "ByItem";
                    _numberOfBill = splitByItemPage.currentCustomerId;

                    GetNewSplitItemList(splitOrderedItems, _numberOfBill, _splitType);
                    Refresh();

                }
                else
                {
                    return;
                }
            }


        }


        //(Button) on List view
        private void CustomerNumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            MessageBox.Show(clickedButton.Content.ToString());
        }

        //(Method) for split item
        private ObservableCollection<OrderedItem> GetNewSplitItemList(ObservableCollection<OrderedItem>splitedList,int numberOfBill,string splitType)
        {
            ObservableCollection<OrderedItem> items = splitedList;
            foreach (OrderedItem splitOrderedItem in items)
            {
                if (splitType == "ByItem")
                {
                    for (int i = 1; numberOfBill > 0; i++)
                    {
                        OrderedItem newSplitBill = new OrderedItem
                    {

                        order_id = splitOrderedItem.order_id,
                        item_id = splitOrderedItem.item_id,
                        item_name = splitOrderedItem.item_name,
                        Quantity = splitOrderedItem.Quantity,
                        origialItemPrice = splitOrderedItem.origialItemPrice,
                        ItemPrice = splitOrderedItem.ItemPrice,
                        IsSavedItem = true,
                        customerID = i

                    };
                        splitOrderedItems.Add(newSplitBill);
                        
                        numberOfBill--;
                    }

                }
                else if (splitType == "ByBill")
                {

                for (int i = 1; i <= numberOfBill; i++) { 
                OrderedItem newSplitBill = new OrderedItem
                {

                    order_id = splitOrderedItem.order_id,
                    item_id = splitOrderedItem.item_id,
                    item_name = splitOrderedItem.item_name,
                    Quantity = splitOrderedItem.Quantity,
                    origialItemPrice = splitOrderedItem.origialItemPrice,
                    ItemPrice = splitOrderedItem.ItemPrice / numberOfBill,
                    IsSavedItem = true,
                    customerID = i  
                };
                    splitOrderedItems.Add(newSplitBill);
                }

                }
            }
            orderedItems.Clear();
            foreach (var splitItem in splitOrderedItems)
            {
                orderedItems.Add(splitItem);
            }

            return orderedItems;
        }


        //(button) go to payment page
        private void PaymentButton_Click(object sender, RoutedEventArgs e)
        {
            PaymentPage paymentPage = new PaymentPage();
            long orderId = GetOrderId(_tableNumber);

            if (orderedItems.Count == 0)
            {
                MessageBox.Show("No item on this table. Please save before payment");
                return;
            }
            else if (IsSavedItem() == false && orderedItems.Count != OriginalItemsCount)
            {
                MessageBox.Show("New Item(s) has not saved yet. Please save before payment");
                return;
            }
            else if (orderedItems.Count < OriginalItemsCount)
            {
                MessageBox.Show("Remove Item has not saved yet. Please save before payment");
                return;
            }


            else
            {
                PaymentWindow paymentWindow = new PaymentWindow(this,orderedItems, _tableNumber, _orderType, orderId, _status, false, _numberOfBill);
                paymentWindow.ShowDialog();
                


            }
        }

        // new cancel button Thevagi from PK
        //Method for CancelButtonClick - By PK
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_hasPaidOrders.Equals(false))
            {
                MessageBox.Show("This page has no unpaid order");
            }
            else
            {
                CancelOrder(_tableNumber);
                TablePage tablePage = new TablePage();
                tablePage.ShowDialog();
                this.Close();

            }
        }

        //Method for CancelOrder - By PK
        private void CancelOrder(string tableNumber)
        {
            //MessageBox.Show("Order Canceled");
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    long orderId = GetOrderId(tableNumber);
                    string cancelOrderSql = "UPDATE pos_db.order SET paid = 'c' WHERE order_id = @orderId;";
                    MySqlCommand cancelOrderCmd = new MySqlCommand(cancelOrderSql, conn);
                    cancelOrderCmd.Parameters.AddWithValue("@orderId", orderId);
                    MessageBox.Show(cancelOrderSql);
                    cancelOrderCmd.ExecuteReader();
                    conn.Close();

                    //OrdersListBox.Items.GroupDescriptions.Clear(); <--- Is this needed?
                    orderedItems.Clear();
                    //TotalAmount = 0; <--- Is this needed?

                    MessageBox.Show("Order ID: " + orderId + " has been canceled!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error canceling orders: " + ex.ToString());
                }
            }
        }



        //Button for reset Split
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (orderedItems.Count == 0) //Condition: it is for empty list
            {
                MessageBox.Show("Sorry! there is nothing to reset!");
                return;
            }
            
            else if (OriginalItemsCount == orderedItems.Count)
            {
                MessageBox.Show("Sorry! there is no change on the list");
            }

            else if (_numberOfBill > 0 && (_splitType == "ByItem" || _splitType == "ByBill")) //Condition: it is for split order
            {
                MessageBoxResult result = MessageBox.Show("There is splitted order. \nDo you confirm reset split items?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Reset();

                }
                else
                {
                    return;
                }
            }

            else if (OriginalItemsCount > 0 && IsSavedItem() == false) //Condition: it is for existing order and added items
            {
                MessageBoxResult result = MessageBox.Show("There are unsave item(s) on the list. \n Do you want to remove all add items?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Reset();

                }
                else
                {
                    return;
                }
            } 
            
            else if (OriginalItemsCount > orderedItems.Count)
            {
                MessageBoxResult result = MessageBox.Show("Saved Item was removed before. \n Do you want to put back the saved item?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Reset();

                }
                else
                {
                    return;
                }
            }

            else if (OriginalItemsCount == 0 && itemClick == true && orderedItems.Count>0)
            {
                MessageBoxResult result = MessageBox.Show("New item added on the list \n Do you want to clear all items?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Reset();

                }
                else
                {
                    return;
                }
            }

        }

        //(Method) Reset order to backup
        private void Reset()
        {
            splitOrderedItems.Clear();
            orderedItems.Clear();
            _numberOfBill = 0;
            TotalAmount = 0;
            _splitType = "No Split";

            foreach (OrderedItem backupOrderedItem in backupOrderedItems)
            {

                orderedItems.Add(backupOrderedItem);
                TotalAmount +=backupOrderedItem.ItemPrice;
            }
            OrdersListView.Items.GroupDescriptions.Clear();
            TotalAmountTextBlock.Text = "Total Amount :             " + TotalAmount.ToString("C", new CultureInfo("en-CA"));
        }

        //(Method) Back up OrderedItem collection
        private List<OrderedItem> BackupOrderedItemCollection()
        {
            backupOrderedItems.Clear();
            foreach (OrderedItem items in orderedItems) 
            {
                backupOrderedItems.Add(items);
            }


            return backupOrderedItems;
        }



        //(button)back button
        private void Back_to_TablePage(object sender, RoutedEventArgs e)
        {
            if (orderedItems.Count == 0 && OriginalItemsCount > 0)
            {
                MessageBox.Show("Since it is a saved order and nothing on the list now. \n\n Please canel order before closing window!");
                return;
            }

            else if (orderedItems.Count < OriginalItemsCount) // Condition: removed item
            {

                MessageBoxResult result = MessageBox.Show("Removed Item on the list. \n\n\n Do you want to save? \nYes --- Save the order \nNo --- Back to Table Page", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    AutoSave();

                }
                else
                {
                    BackToTablePage();
                }
            }

            else if (IsSavedItem() == false && orderedItems.Count > OriginalItemsCount ) // Condition: added item 
            {

                MessageBoxResult result = MessageBox.Show("There is new item on the list. \n\n\n Do you want to save? \nYes --- Save the order \nNo ---  Back to Table Page", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    AutoSave();
                }
                else
                {
                    BackToTablePage();
                }
            }

            

            else if (IsSavedItem() == true || StatusTextBlock.Text == "New Order")
            {
                BackToTablePage();
            }

        }
        


        //Method : for go back table page.
        private void BackToTablePage()
        {
            orderedItems.Clear();
            TablePage tablePage = new TablePage();

            if (TypeTextBox.Text.Equals("Take-Out"))

            {
                tablePage.TablePageTab.SelectedIndex = 1;
            }
            else
            {
                tablePage.TablePageTab.SelectedItem = 0;
            }

            tablePage.Show();
            this.Close();
        }

        //Method : check if any item is old item (ie. exist items)
        private bool IsSavedItem()
        {
            bool IsSavedItem = false;

                foreach (OrderedItem itemOnListView in OrderedItems)
                {
                    if (itemOnListView.IsSavedItem == false)
                    {
                    IsSavedItem = false; //added new item on list but not yet save
                    }
                    else if (itemOnListView.IsSavedItem == true)
                    {
                    IsSavedItem = true; //nothing added on the existing list
                    }

                }
            
            return IsSavedItem;
        }

        //button for void item: remove item from list view
        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {


            if(OrdersListView.SelectedItem is OrderedItem selectedOrderedItem)
            {

                if (IsSavedItem() == false) //Condition: new item delete
                {
                    orderedItems.Remove(selectedOrderedItem);
                    TotalAmount -= selectedOrderedItem.ItemPrice;
                    TotalAmountTextBlock.Text = "Total Amount :             "  + TotalAmount.ToString();
                    CultureInfo cultureInfo = new CultureInfo("en-CA");
                    cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                    TotalAmountTextBlock.Text = "Total Amount :             " + TotalAmount.ToString("C", cultureInfo);
                }

                else
                {
                    MessageBoxResult result = MessageBox.Show("Selected the saved item \n \n Are you sure to delete the saved item", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);// condition: saved item delete confirmation
                    if (result == MessageBoxResult.Yes)
                    {
 
                        orderedItems.Remove(selectedOrderedItem);
                        TotalAmount -= selectedOrderedItem.ItemPrice;
                        TotalAmountTextBlock.Text = "Total Amount :             " + TotalAmount.ToString();
                        CultureInfo cultureInfo = new CultureInfo("en-CA");
                        cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                        TotalAmountTextBlock.Text = "Total Amount :             " + TotalAmount.ToString("C", cultureInfo);
                        
                    }
                    else
                    {
                        return;
                    }
                }
            }
                
            else
            {
                MessageBox.Show("Please select an item to void.");
            }
        }




        //button for print and save to database: click save, send to database
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (orderedItems.Count == 0 && itemClick == false)//Condition: Empty list
            {
                MessageBox.Show("No Item in this table.Please add items before save!");
                return;
            }
            else if (IsSavedItem() == true && OriginalItemsCount == orderedItems.Count)//Condition: if no items added to existing order  
            {
                MessageBox.Show("No update on the list. Please check again");
                return;
            }


            else
            {
                
                if (OriginalItemsCount > orderedItems.Count) // Condition: if items deleted, double check is it correct
                {
                    MessageBoxResult result = MessageBox.Show("Saved order removed. \n Do you want to save?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        AutoSave();
                        BackToTablePage();
                    }
                    else
                    {
                        return;
                    }
                } else
                {
                    // Save the order
                    AutoSave();
                    BackToTablePage();
                }

            }
            //Howard: Moved the print bill kichen to autosave

        }

        // (Method for save button) save order to database
        private void AutoSave()
        {

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    try
                    {
                        conn.Open();

                        long orderId = GetOrderId(_tableNumber);

                        if (StatusTextBlock.Text.Equals("New Order"))
                        {

                            string orderSql = "INSERT INTO `order` (table_num, order_timestamp, total_amount, order_type, paid) VALUES (@tableNum, @orderTimestamp, @totalAmount, @order_type,'n');";
                            MySqlCommand orderCmd = new MySqlCommand(orderSql, conn);
                            orderCmd.Parameters.AddWithValue("@tableNum", _tableNumber);
                            orderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                            orderCmd.Parameters.AddWithValue("@totalAmount", TotalAmount);
                            orderCmd.Parameters.AddWithValue("@order_type", TypeTextBox.Text);
                            orderCmd.ExecuteNonQuery();
                            orderId = orderCmd.LastInsertedId;
                            foreach (OrderedItem newOrder in orderedItems)
                            {


                                string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, item_name, quantity, item_price, original_item_price ,customer_id) VALUES (@orderId, @itemId, @itemName, @quantity, @itemPrice, @originalItemPrice,0);";
                                MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                                itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                itemCmd.Parameters.AddWithValue("@itemId", newOrder.item_id);
                                itemCmd.Parameters.AddWithValue("@itemName", newOrder.item_name);
                                itemCmd.Parameters.AddWithValue("@quantity", 1);
                                itemCmd.Parameters.AddWithValue("@originalItemPrice", newOrder.origialItemPrice);
                                itemCmd.Parameters.AddWithValue("@itemPrice", newOrder.ItemPrice);
                                itemCmd.ExecuteNonQuery();

                            }

                        }
                        else if (StatusTextBlock.Text.Equals("Occupied"))
                        {




                            string removeOrderedItemlistSql = "DELETE FROM ordered_itemlist WHERE order_id = @orderId;";
                            MySqlCommand removeOrderCmd = new MySqlCommand(removeOrderedItemlistSql, conn);
                            removeOrderCmd.Parameters.AddWithValue("@orderId", orderId);
                            removeOrderCmd.ExecuteNonQuery();

                            string updateOrderSql = "UPDATE `order` SET order_timestamp = @orderTimestamp, total_amount = @totalAmount WHERE order_id = @orderId; ";
                            MySqlCommand updateOrderCmd = new MySqlCommand(updateOrderSql, conn);
                            updateOrderCmd.Parameters.AddWithValue("@orderTimestamp", DateTime.Now);
                            updateOrderCmd.Parameters.AddWithValue("@totalAmount", TotalAmount);
                            updateOrderCmd.Parameters.AddWithValue("@orderId", orderId);
                            updateOrderCmd.ExecuteNonQuery();

                            foreach (OrderedItem orderedItem in orderedItems)
                            {

                                string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, item_name, quantity, item_price, original_item_price ,customer_id) VALUES (@orderId, @itemId, @itemName, @quantity, @itemPrice, @originalItemPrice,0);";
                                MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                                itemCmd.Parameters.AddWithValue("@orderId", orderId);
                                itemCmd.Parameters.AddWithValue("@itemId", orderedItem.item_id);
                                itemCmd.Parameters.AddWithValue("@itemName", orderedItem.item_name);
                                itemCmd.Parameters.AddWithValue("@quantity", 1);
                                itemCmd.Parameters.AddWithValue("@originalItemPrice", orderedItem.origialItemPrice);
                                itemCmd.Parameters.AddWithValue("@itemPrice", orderedItem.ItemPrice);
                                itemCmd.ExecuteNonQuery();


                            }



                        }
                        MessageBox.Show("Order save successfully!");
                     // Print the receipt
                    if (OriginalItemsCount > orderedItems.Count || IsSavedItem() == false)
                    {
                        PrintKitchenReceipt();
                    }
                    conn.Close();
                    
                }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("MySQL Error: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving order: " + ex.ToString());
                    }
                }
            
        }

        //(Method for print bill button) print kitchen receipt
        public void PrintKitchenReceipt()
        {
            // Create a FlowDocument for the kitchen receipt
            FlowDocument kitchenReceiptDocument = new FlowDocument();

            // Header for the receipt including table number, order type, order number, and date and time formatted
            Paragraph headerParagraph = new Paragraph();
            headerParagraph.FontSize = 20;
            headerParagraph.TextAlignment = TextAlignment.Justify;
            headerParagraph.Inlines.Add(new Run("Kitchen Receipt") { FontWeight = FontWeights.Bold });
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new Run("Table: " + TableNumberTextBox.Text));
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new Run("Order Type: " + TypeTextBox.Text));
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new Run("Order Number: " + OrderIdTextBlock.Text));
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new Run("Date and Time: " + DateTime.Now.ToString("MMMM/dd/yyyy hh:mm")));
            headerParagraph.Inlines.Add(new LineBreak());
            headerParagraph.Inlines.Add(new LineBreak());
            kitchenReceiptDocument.Blocks.Add(headerParagraph);

            // Create a Table for the items
            Table itemsTable = new Table();
            TableRowGroup itemTableRowGroup = new TableRowGroup();

            // Create a header row for the items table
            TableRow headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Item")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("Quantity")) { FontWeight = FontWeights.Bold }));
            itemTableRowGroup.Rows.Add(headerRow);

            // Create a Section for the order details
            Section orderDetailsSection = new Section();

            // Create a dictionary to store item quantities
            Dictionary<string, int> itemQuantities = new Dictionary<string, int>();


            // Add quantities for ordered items
            foreach (var orderedItem in orderedItems)
            {
                if (itemQuantities.ContainsKey(orderedItem.item_name))
                {
                    itemQuantities[orderedItem.item_name] += orderedItem.Quantity;
                }
                else
                {
                    itemQuantities.Add(orderedItem.item_name, orderedItem.Quantity);
                }

            }

            // Add rows for item details (e.g., items and quantities)
            foreach (var kvp in itemQuantities)
            {
                TableRow itemRow = new TableRow();
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run(kvp.Key))));
                itemRow.Cells.Add(new TableCell(new Paragraph(new Run(kvp.Value.ToString()))));
                itemTableRowGroup.Rows.Add(itemRow);
            }

            itemsTable.RowGroups.Add(itemTableRowGroup);
            orderDetailsSection.Blocks.Add(itemsTable);

            // Add the order details section to the FlowDocument
            kitchenReceiptDocument.Blocks.Add(orderDetailsSection);

            // Create a DocumentPaginator for the FlowDocument
            IDocumentPaginatorSource paginatorSource = kitchenReceiptDocument;
            DocumentPaginator documentPaginator = paginatorSource.DocumentPaginator;

            // Create a PrintDialog
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Print the kitchen receipt
                printDialog.PrintDocument(documentPaginator, "Kitchen Receipt");
                MessageBox.Show("Order sent to Kitchen successfully!");
            }

            


        }

        //Method: Get Order Id
        private long GetOrderId(string tableNumber)
        {
            long orderId = 0;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string checkUnpaidOrderSql = "SELECT order_id FROM `order` WHERE table_num = @tableNum AND paid = 'n';";
                    MySqlCommand checkUnpaidOrderCmd = new MySqlCommand(checkUnpaidOrderSql, conn);
                    checkUnpaidOrderCmd.Parameters.AddWithValue("@tableNum", tableNumber);

                    object existingOrderId = checkUnpaidOrderCmd.ExecuteScalar();

                    if (existingOrderId != null)
                    {
                        orderId = Convert.ToInt64(existingOrderId);
                    }
                    else
                    {
                        orderId = checkUnpaidOrderCmd.LastInsertedId;
                    }
                    conn.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error getting order_id: " + ex.ToString());
                }
            }

            return orderId;

        }


        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (orderedItems.Count == 0 && itemClick==false) 
            {
                MessageBox.Show("Sorry! Nothing to print for the order!");
                return;
            } 

            else if (orderedItems.Count == 0)
            {
                MessageBox.Show("Sorry! Nothing to print for the order!");
                return;
            }
            
            else if (IsSavedItem()==false && orderedItems.Count > 0) 
            {
                MessageBox.Show("New Item on the list. \n\nPlease save the order first!");
                return;
            }
            else
            {
                PrintDialog printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    // Calculate GST (5% of TotalAmount)
                    double gstRate = 0.05;  // GST rate as 5%
                    double gstAmount = TotalAmount * gstRate;
                    // Calculate TotalAmount with GST included
                    double totalAmountWithGST = TotalAmount + gstAmount;

                    // Iterate through split bills
                    for (int customerID = 1; customerID <= _numberOfBill; customerID++)
                    {
                        // Create a FlowDocument for each split bill
                        FlowDocument flowDocument = new FlowDocument();

                        // Add the "-------------------------------------------------" separator at the top
                        flowDocument.Blocks.Add(new Paragraph(new Run("-------------------------------------------------")));
                        // Create a paragraph for restaurant information
                        Paragraph restaurantInfoParagraph = new Paragraph();
                        restaurantInfoParagraph.TextAlignment = TextAlignment.Center;

                        // Restaurant Name
                        Run restaurantNameRun = new Run("Thai Bistro\n");
                        restaurantNameRun.FontSize = 20;
                        restaurantInfoParagraph.Inlines.Add(restaurantNameRun);

                        // Address
                        Run addressRun = new Run("233 Centre St S #102,\n Calgary, AB T2G 2B7\n");
                        addressRun.FontSize = 12;
                        restaurantInfoParagraph.Inlines.Add(addressRun);

                        // Phone
                        Run phoneRun = new Run("Phone: (403) 313-9922\n");
                        phoneRun.FontSize = 12;
                        restaurantInfoParagraph.Inlines.Add(phoneRun);

                        // Add the restaurant info paragraph
                        flowDocument.Blocks.Add(restaurantInfoParagraph);

                        // Add the "-------------------------------------------------" separator at the top
                        flowDocument.Blocks.Add(new Paragraph(new Run("-------------------------------------------------")));

                        ///^^^^^^good^^^^^^^^^^^

                        ////////order detail session!!!!

                        // Create a Section for the order details
                        Section orderDetailsSection = new Section();

                        // Table to display order details
                        Table detailsTable = new Table();
                        TableRowGroup detailTableRowGroup = new TableRowGroup();
                        // Add rows for order details
                        detailTableRowGroup.Rows.Add(CreateTableRow("Date:", DateTime.Now.ToString("MMMM/dd/yyyy hh:mm")));
                        detailTableRowGroup.Rows.Add(CreateTableRow("Table:", TableNumberTextBox.Text));
                        if (OrderIdTextBlock != null) // Check if the TextBlock exists
                        {
                            // Access the text of the OrderIdTextBlock
                            detailTableRowGroup.Rows.Add(CreateTableRow("Order ID:", OrderIdTextBlock.Text));
                        }
                        detailTableRowGroup.Rows.Add(CreateTableRow("Server:", "John"));

                        // Add a line with dashes after "Server: John"
                        TableRow dashedLineRow = new TableRow();
                        TableCell dashedLineCell = new TableCell();

                        Paragraph dashedLineParagraph = new Paragraph(new Run("-------------------------------------------------"));
                        //dashedLineParagraph.TextAlignment = TextAlignment.Center;
                        dashedLineCell.ColumnSpan = 2;
                        dashedLineCell.Blocks.Add(dashedLineParagraph);
                        dashedLineRow.Cells.Add(dashedLineCell);
                        detailTableRowGroup.Rows.Add(dashedLineRow);

                        ///item session

                        // Create a TableRow for displaying items and their prices
                        TableRowGroup itemTableRowGroup = new TableRowGroup();
                        Section itemSection = new Section();
                        // Add space (empty TableRow) for the gap
                        itemTableRowGroup.Rows.Add(CreateEmptyTableRow());

                        // Create a nested Table within the items cell
                        Table itemsTable = new Table();

                        // Access the 'Items' collection and loop through it to add item rows.
                        foreach (var OrderedItem in orderedItems.Where(item => item.customerID == customerID))
                        {
                            itemTableRowGroup.Rows.Add(CreateTableRow(OrderedItem.item_name, OrderedItem.ItemPrice.ToString("C")));
                        }

                        // Initialize the TableRowGroup
                        itemSection.Blocks.Add(itemsTable);

                        // Create a new TableRow for the itemsCell and add it to the tableRowGroup
                        // Add space (empty TableRow) for the gap
                        itemTableRowGroup.Rows.Add(CreateEmptyTableRow());
                        /////!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        ///////sub total session
                        Table paymentTable = new Table();
                        Section paymentSection = new Section();
                        TableRowGroup paymentTableRowGroup = new TableRowGroup();
                        // Create a Paragraph for "Sub Total" with underline
                        Paragraph subTotalParagraph = new Paragraph(new Run("Sub Total:"));
                        subTotalParagraph.FontSize = 20; // Increase the font size
                        subTotalParagraph.TextAlignment = TextAlignment.Right;

                        double customerTotalAmount = orderedItems.Where(item => item.customerID == customerID).Sum(item => item.ItemPrice);
                        Paragraph subTotalValueParagraph = new Paragraph(new Run(customerTotalAmount.ToString("C")));
                        paymentTableRowGroup.Rows.Add(CreateTableRowWithParagraph(subTotalParagraph, subTotalValueParagraph));

                        // Create a Paragraph for "GST"
                        Paragraph gstLabelParagraph = new Paragraph(new Run("GST (5%):"));
                        gstLabelParagraph.FontSize = 20; // Increase the font size
                        gstLabelParagraph.TextAlignment = TextAlignment.Right;

                        double customerGSTAmount = customerTotalAmount * gstRate;
                        Paragraph gstValueParagraph = new Paragraph(new Run(customerGSTAmount.ToString("C")));
                        paymentTableRowGroup.Rows.Add(CreateTableRowWithParagraph(gstLabelParagraph, gstValueParagraph));

                        // Create a Paragraph for "Total Amount"
                        Paragraph totalAmountLabelParagraph = new Paragraph(new Run("Total Amount:"));
                        totalAmountLabelParagraph.FontSize = 20; // Increase the font size
                        totalAmountLabelParagraph.TextAlignment = TextAlignment.Right;

                        double customerTotalAmountWithGST = customerTotalAmount + customerGSTAmount;
                        Paragraph totalAmountValueParagraph = new Paragraph(new Run(customerTotalAmountWithGST.ToString("C")));
                        paymentTableRowGroup.Rows.Add(CreateTableRowWithParagraph(totalAmountLabelParagraph, totalAmountValueParagraph));
                        //////////////////////////////////////////////////

                        detailsTable.RowGroups.Add(detailTableRowGroup);
                        itemsTable.RowGroups.Add(itemTableRowGroup);
                        paymentTable.RowGroups.Add(paymentTableRowGroup);

                        orderDetailsSection.Blocks.Add(detailsTable);
                        itemSection.Blocks.Add(itemsTable);
                        paymentSection.Blocks.Add(paymentTable);

                        flowDocument.Blocks.Add(orderDetailsSection);
                        flowDocument.Blocks.Add(itemSection);
                        flowDocument.Blocks.Add(paymentSection);
                        // /*****************************
                        // Create a new paragraph for the "Thank You" message
                        Paragraph thankYouParagraph = new Paragraph();
                        thankYouParagraph.TextAlignment = TextAlignment.Center;
                        thankYouParagraph.FontSize = 16; // You can set the font size as you wish
                        thankYouParagraph.Inlines.Add(new Run("Thank You for dining with us!"));
                        thankYouParagraph.Margin = new Thickness(0, 10, 0, 0); // Add some space before the message if needed

                        // Add a "-------------------------------------------------" separator before the "Thank You" message
                        flowDocument.Blocks.Add(new Paragraph(new Run("-------------------------------------------------")));

                        // Add the "Thank You" paragraph to the FlowDocument
                        flowDocument.Blocks.Add(thankYouParagraph);
                        flowDocument.Blocks.Add(new Paragraph(new Run("-------------------------------------------------")));

                        //**********************************

                        // Create a DocumentPaginator for the FlowDocument
                        IDocumentPaginatorSource paginatorSource = flowDocument;
                        DocumentPaginator documentPaginator = paginatorSource.DocumentPaginator;

                        // Send the document to the printer
                        printDialog.PrintDocument(documentPaginator, $"Order Receipt - Customer {customerID}");
                    }
                }
            }
        }

        private TableRow CreateTableRow(string label, string value)
        {
            TableRow row = new TableRow();

            // Label cell
            TableCell labelCell = new TableCell(new Paragraph(new Run(label)));
            labelCell.TextAlignment = TextAlignment.Right;
            labelCell.BorderThickness = new Thickness(0, 0, 20, 0); // Add space on the right side
            labelCell.BorderBrush = Brushes.Transparent; // Set the border brush to transparent to hide the line
            row.Cells.Add(labelCell);

            // Value cell
            TableCell valueCell = new TableCell(new Paragraph(new Run(value)));
            valueCell.BorderThickness = new Thickness(0); // No column lines, only space
            row.Cells.Add(valueCell);

            return row;
        }



        private TableRow CreateEmptyTableRow()
        {
            TableRow row = new TableRow();

            TableCell emptyCell = new TableCell(new Paragraph(new Run(" "))); // Add a space or empty string
            emptyCell.ColumnSpan = 2; // Set the column span to cover both columns

            row.Cells.Add(emptyCell);

            return row;
        }


        // For Styling
        private void SetButtonStyle(Button button)
        {
            button.FontFamily = new FontFamily("Verdana");
            button.FontSize = 20;
            button.Background = Brushes.Orange;
            button.FontWeight = FontWeights.Bold;
            button.BorderBrush = Brushes.Orange;
            button.Margin = new Thickness(5);
        }

        private TableRow CreateTableRowWithParagraph(Paragraph labelParagraph, Paragraph valueParagraph)
        {
            TableRow row = new TableRow();

            // Label cell
            TableCell labelCell = new TableCell(labelParagraph);
            labelCell.TextAlignment = TextAlignment.Right;
            labelCell.BorderThickness = new Thickness(0, 0, 20, 0); // Add space on the right side
            labelCell.BorderBrush = Brushes.Transparent; // Set the border brush to transparent to hide the line
            row.Cells.Add(labelCell);

            // Value cell
            TableCell valueCell = new TableCell(valueParagraph);
            valueCell.BorderThickness = new Thickness(0); // No column lines, only space
            row.Cells.Add(valueCell);

            return row;
        }


        

    }
}