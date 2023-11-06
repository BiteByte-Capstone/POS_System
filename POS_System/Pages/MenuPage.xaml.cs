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


//Code for debug
/*foreach (OrderedItem splited in splitOrderedItems)
{
    string message = $"Order ID: {splited.order_id}\n" +
                     $"Item ID: {splited.item_id}\n" +
                     $"Item Name: {splited.item_name}\n" +
                     $"Quantity: {splited.Quantity}\n" +
                     $"Item Price: {splited.ItemPrice:C}\n" +  // Display as currency
                     $"Is Existing Item: {splited.IsExistItem}\n" +
                     $"Customer ID: {splited.customerID}";

    MessageBox.Show(message);
}
*/

/*foreach (OrderedItem splited in orderedItems)
{
    string message = $"Order ID: {splited.order_id}\n" +
                     $"Item ID: {splited.item_id}\n" +
                     $"Item Name: {splited.item_name}\n" +
                     $"Quantity: {splited.Quantity}\n" +
                     $"Item Price: {splited.ItemPrice:C}\n" +
                     $"Original price: {splited.OriginalItemPrice:C}\n" +
                     $"Is Existing Item: {splited.IsExistItem}\n" +
                     $"Customer ID: {splited.customerID}";

    MessageBox.Show(message);

}*/

namespace POS_System.Pages
{
    public partial class MenuPage : Window, INotifyPropertyChanged
    {
        // Define connStr at the class level
        public string connStr = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        //categories
        private ObservableCollection<Category> categories = new ObservableCollection<Category>();
        //new order
        private ObservableCollection<Item> items = new ObservableCollection<Item>();
        //existing order
        private ObservableCollection<OrderedItem> orderedItems = new ObservableCollection<OrderedItem>();

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

        //Splited order 
        private ObservableCollection<OrderedItem> splitOrderedItems = new ObservableCollection<OrderedItem>();

        private string _tableNumber;
        private string _orderType;
        private string _status;
        private bool _hasPaidOrders;
        private int _numberOfBill;
        private string _splitType;

        private double TotalAmount = 0.0;
        private int existItemCount = 0;
        private bool itemClick = false;
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


            OrdersListBox.Items.GroupDescriptions.Clear();
            var property = "FormattedCustomerID";
            OrdersListBox.Items.GroupDescriptions.Add(new PropertyGroupDescription(property));

        }

        //Method for refresh page: update UI after change button.
        private void Refresh()
        {
/*            splitOrderedItems.Clear();
            orderedItems.Clear();*/

            TotalAmount = 0;
            GroupItemList();
/*            OrdersListBox.Items.GroupDescriptions.Clear();*/
            /*            LoadUnpaidOrders(_tableNumber);*/

        }



        private void LoadUnpaidOrders(string tableNumber)
        {
            OrdersListBox.Items.GroupDescriptions.Clear();
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
                    dataAdapter.Fill(unpaidOrdersTable);//!!!!!!! remove messageBox later
                    /*items.Clear();*/

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
                            IsExistItem = true,
                            customerID = Convert.ToInt32(row["customer_id"])
                        };
                        existItemCount++;
                        _numberOfBill = orderedItem.customerID;


                        if (orderedItem.customerID > 0)
                        {
                            isSplited = true;
                        } else if (orderedItem.customerID == 0)
                        {
                            isSplited = false;
                        }
                        orderedItems.Add(orderedItem);
                        TotalAmount += orderedItem.ItemPrice;
                    }
                    TotalAmountTextBlock.Text = TotalAmount.ToString("C", new CultureInfo("en-CA"));
                    
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
                    newCategoryButton.Width = 150;
                    newCategoryButton.Height = 30;
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
                    newItemButton.Content = rdr["item_name"].ToString();
                    newItemButton.Tag = item;
                    newItemButton.Width = 150;
                    newItemButton.Height = 80;
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
                IsExistItem = false,
                customerID = 0
            };

            orderedItems.Add(orderedItem);
            
            TotalAmount += orderedItem.ItemPrice;
            CultureInfo cultureInfo = new CultureInfo("en-CA");
            cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
            TotalAmountTextBlock.Text = TotalAmount.ToString("C", cultureInfo);
        }

        //(Button for Split Bill)
        private void SplitBillButton_Click(object sender, RoutedEventArgs e)
        {
            SplitBillDialog splitBillDialog = new SplitBillDialog(orderedItems, TotalAmount);

            
            if (splitBillDialog.ShowDialog() == true)
            {
                _numberOfBill = splitBillDialog.NumberOfPeople;
                _splitType = "splitByBill";
                

            }
            else
            {
                return;
            }
            if (_numberOfBill > 0)
            {

                orderedItems = GetNewSplitItemList(_numberOfBill, _splitType);
                //!! remove later Since it is for connect database
                /*                RemoveOrderByOrderID(GetOrderId(_tableNumber));
                                addItemToDatabase(orderedItems);*/
                Refresh();
                MessageBox.Show($"Splited bill into {_numberOfBill}");
                
            } else
            {
                MessageBox.Show($"Splited bill into {_numberOfBill}, Please try again");
            }
        }

        //(Button) on List view
        private void CustomerNumberButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            MessageBox.Show(clickedButton.Content.ToString());
        }

        //(Method) for split item
        private ObservableCollection<OrderedItem> GetNewSplitItemList(int numberOfBill,string splitType)
        {

            foreach (OrderedItem orderedItem in orderedItems)
            {
                for (int i = 1; i <= numberOfBill; i++) { 
                OrderedItem newSplitBill = new OrderedItem
                {

                    order_id = orderedItem.order_id,
                    item_id = orderedItem.item_id,
                    item_name = orderedItem.item_name,
                    Quantity = orderedItem.Quantity,
                    origialItemPrice = orderedItem.origialItemPrice,
                    ItemPrice = orderedItem.ItemPrice / numberOfBill,
                    IsExistItem = orderedItem.IsExistItem,
                    customerID = i  
                };
                    splitOrderedItems.Add(newSplitBill);
                }
            }
            orderedItems.Clear();
            foreach (var splitItem in splitOrderedItems)
            {
                orderedItems.Add(splitItem);
            }

            return orderedItems;
        }

        private void addItemToDatabase(ObservableCollection<OrderedItem> items)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    foreach (var orderedItem in items)
                    {



                        string itemSql = "INSERT INTO ordered_itemlist (order_id, item_id, item_name, quantity, original_item_price ,item_price, customer_id) VALUES (@orderId, @itemId, @itemName, @quantity, @originalItemPrice,@itemPrice, @customerID);";
                        MySqlCommand itemCmd = new MySqlCommand(itemSql, conn);
                        itemCmd.Parameters.AddWithValue("@orderId", orderedItem.order_id);
                        itemCmd.Parameters.AddWithValue("@itemId", orderedItem.item_id);
                        itemCmd.Parameters.AddWithValue("@itemName", orderedItem.item_name);
                        itemCmd.Parameters.AddWithValue("@quantity", 1);
                        itemCmd.Parameters.AddWithValue("@originalItemPrice", orderedItem.origialItemPrice);
                        itemCmd.Parameters.AddWithValue("@itemPrice", orderedItem.ItemPrice);
                        itemCmd.Parameters.AddWithValue("@customerID", orderedItem.customerID);
                        itemCmd.ExecuteNonQuery();
                    }


                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }
        }

        //(Method) drop the item by order id
        private void RemoveOrderByOrderID(long orderId)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();


                    MessageBox.Show($"remove order id {orderId}");
                    string removeOrderedItemlistSql = "DELETE FROM ordered_itemlist WHERE order_id = @orderId;";
                    MySqlCommand removeOrderCmd = new MySqlCommand(removeOrderedItemlistSql, conn);
                    removeOrderCmd.Parameters.AddWithValue("@orderId", orderId);
                    removeOrderCmd.ExecuteNonQuery();



                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }


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
            else if (ExistedItem() == false && orderedItems.Count != existItemCount)
            {
                MessageBox.Show("New Item(s) has not saved yet. Please save before payment");
                return;
            }
/*            else if (orderedItems.Count < existItemCount)
            {
                MessageBox.Show("Remove Item has not saved yet. Please save before payment");
                return;
            }*/

                
            else
            {
                while (_numberOfBill > 0)
                {
                    while (paymentPage.Close())
                    {

                    }
                    PaymentPage paymentPage = new PaymentPage(orderedItems, _tableNumber, _orderType, orderId, _status, false);
                    paymentPage.Show();
                }


                this.Close();
            }
        }


        //(button) reset button click
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();



                    string removeOrderedItemlistSql = "DELETE FROM ordered_itemlist WHERE order_id = @orderId and customer_id > 1;";
                    MySqlCommand removeOrderCmd = new MySqlCommand(removeOrderedItemlistSql, conn);
                    removeOrderCmd.Parameters.AddWithValue("@orderId", GetOrderId(_tableNumber));
                    removeOrderCmd.ExecuteNonQuery();



                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }

            ResetCustomerID(orderedItems);
            MessageBox.Show("Reset from Split bill.");
            
            LoadUnpaidOrders(_tableNumber);
        }

        //(Method for reset button) reset method
        private void ResetCustomerID(ObservableCollection<OrderedItem> items)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();



                    foreach (var orderedItem in items)
                    {



                        string ResetItemCustomerIDSql = "UPDATE `ordered_itemlist` SET item_price = @itemPrice, customer_id = @customerID;";
                        MySqlCommand itemCmd = new MySqlCommand(ResetItemCustomerIDSql, conn);
                        itemCmd.Parameters.AddWithValue("@itemPrice", orderedItem.origialItemPrice);
                        itemCmd.Parameters.AddWithValue("@customerID", 0);
                        itemCmd.ExecuteNonQuery();
                    }




                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("MySQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }
        }

        //(button)back button
        private void Back_to_TablePage(object sender, RoutedEventArgs e)
        {
            if (orderedItems.Count != existItemCount)
            {
                MessageBox.Show("yes void order!");
                MessageBoxResult result = MessageBox.Show("Removed order on the list. \n Do you want to go back without save?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    BackToTablePage();
                    
                }
                else
                {
                    return;
                }
            }

            else if (ExistedItem() == true || StatusTextBlock.Text == "New Order")
            {
                MessageBox.Show("no new order!");
                BackToTablePage();
            }
            else if (ExistedItem() == false)
            {
                MessageBox.Show("yes new order!");
                MessageBoxResult result = MessageBox.Show("There is new item on the list. \n Do you want to go back without save?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    BackToTablePage();
                }
                else
                {
                    return;
                }
            }



                

        }
        


        //Method: for go back table page.
        private void BackToTablePage()
        {
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

        //Method: check if any item is old item (ie. exist items)
        private bool ExistedItem()
        {
            bool ExistedItem = false;
            foreach (OrderedItem item in orderedItems)
            {
                foreach (OrderedItem itemOnViewList in orderedItems)
                {
                    if (itemOnViewList.IsExistItem == false)
                    {
                        ExistedItem = false; //added new item on list but not yet save
                    }
                    else if (itemOnViewList.IsExistItem == true)
                    {
                        ExistedItem = true; //nothing added on the existing list
                    }

                }
            }
            return ExistedItem;
        }

        //button for void item: remove item from list view
        private void VoidButton_Click(object sender, RoutedEventArgs e)
        {


            if(OrdersListBox.SelectedItem is OrderedItem selectedOrderedItem)
            {
/*                if (ExistedItem() == true)
                {*/

                    
                    orderedItems.Remove(selectedOrderedItem);
                    TotalAmount -= selectedOrderedItem.ItemPrice;
                    TotalAmountTextBlock.Text = TotalAmount.ToString();
                    CultureInfo cultureInfo = new CultureInfo("en-CA");
                    cultureInfo.NumberFormat.CurrencyDecimalDigits = 2;
                    TotalAmountTextBlock.Text = TotalAmount.ToString("C", cultureInfo);
/*                }*/
            }
                
            else
            {
                MessageBox.Show("Please select an item to void.");
            }
        }




        //button for print and save to database: click save, send to database
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
 
            // Save the order
            AutoSave();

            // Print the receipt
            PrintKitchenReceipt();

            MessageBox.Show("Order sent to Kitchen successfully!");

            orderedItems.Clear();
            TablePage tablePage = new TablePage();
            tablePage.Show();
            this.Close();
            





        }

        // (Method for save button) save order to database
        private void AutoSave()
        {
            if (orderedItems.Count == 0 && itemClick == false)
            {
                MessageBox.Show("No Item in this table.Please add items before save!");
                return;
            }
            else if (ExistedItem() == true && orderedItems.Count > existItemCount)
            {
                MessageBox.Show("No update on the list. Please check again");
                return;
            }
            else
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
                        else if (orderedItems.Count == 0)
                        {
                            MessageBox.Show("Please add at least one item");
                            return;
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

                        /*items.Clear();*/
                        TotalAmount = 0.0;
                        TotalAmountTextBlock.Text = TotalAmount.ToString("C");
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


        // Print customer receipt
        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Calculate GST (5% of TotalAmount)
                double gstRate = 0.05;  // GST rate as 5%
                double gstAmount = TotalAmount * gstRate;
                // Calculate TotalAmount with GST included
                double totalAmountWithGST = TotalAmount + gstAmount;

                // Create a FlowDocument
                FlowDocument flowDocument = new FlowDocument();

                // Create a Paragraph for the header
                Paragraph headerParagraph = new Paragraph();
                headerParagraph.FontSize = 30;
                headerParagraph.TextAlignment = TextAlignment.Center;

                // Create a Run for the header text
                Run headerRun = new Run("Order Receipt");

                // Create an Underline element
                Underline underline = new Underline(headerRun);

                // Add the Underline to the Paragraph
                headerParagraph.Inlines.Add(underline);

                // Add the Paragraph to the FlowDocument
                flowDocument.Blocks.Add(headerParagraph);

                // Create a Section for the order details
                Section orderDetailsSection = new Section();

                // Table to display order details
                Table detailsTable = new Table();
                TableRowGroup tableRowGroup = new TableRowGroup();

                // Add rows for order details
                tableRowGroup.Rows.Add(CreateTableRow("Table:", TableNumberTextBox.Text));
                if (OrderIdTextBlock != null) // Check if the TextBlock exists
                {
                    // Access the text of the OrderIdTextBlock
                    tableRowGroup.Rows.Add(CreateTableRow("Order ID:", OrderIdTextBlock.Text));
                }
                // Add space (empty TableRow) for the gap
                tableRowGroup.Rows.Add(CreateEmptyTableRow());

                // Access the 'Items' collection and loop through it to add item rows.
                foreach (var OrderedItem in orderedItems)
                {
                    tableRowGroup.Rows.Add(CreateTableRow(OrderedItem.item_name, OrderedItem.ItemPrice.ToString("C")));
                }

                // Add space (empty TableRow) for the gap
                tableRowGroup.Rows.Add(CreateEmptyTableRow());

                // Create a Paragraph for "Sub Total" with underline
                // Create a Paragraph for "Sub Total" with underline
                Paragraph subTotalParagraph = new Paragraph(new Run("Sub Total:"));
                subTotalParagraph.FontSize = 20; // Increase the font size
                subTotalParagraph.TextAlignment = TextAlignment.Right;

                Paragraph subTotalValueParagraph = new Paragraph(new Run(TotalAmount.ToString("C")));
                tableRowGroup.Rows.Add(CreateTableRowWithParagraph(subTotalParagraph, subTotalValueParagraph));
                // Create a Paragraph for "GST"
                Paragraph gstLabelParagraph = new Paragraph(new Run("GST (5%):"));
                gstLabelParagraph.FontSize = 20; // Increase the font size
                gstLabelParagraph.TextAlignment = TextAlignment.Right;

                Paragraph gstValueParagraph = new Paragraph(new Run(gstAmount.ToString("C")));

                // Add the "GST" label and value to the TableRowGroup
                tableRowGroup.Rows.Add(CreateTableRowWithParagraph(gstLabelParagraph, gstValueParagraph));

                // Create a Paragraph for "Total Amount"
                Paragraph totalAmountLabelParagraph = new Paragraph(new Run("Total Amount:"));
                totalAmountLabelParagraph.FontSize = 20; // Increase the font size
                totalAmountLabelParagraph.TextAlignment = TextAlignment.Right;

                Paragraph totalAmountValueParagraph = new Paragraph(new Run(totalAmountWithGST.ToString("C")));

                // Add the "Total Amount" label and value to the TableRowGroup
                tableRowGroup.Rows.Add(CreateTableRowWithParagraph(totalAmountLabelParagraph, totalAmountValueParagraph));

                detailsTable.RowGroups.Add(tableRowGroup);
                orderDetailsSection.Blocks.Add(detailsTable);

                flowDocument.Blocks.Add(orderDetailsSection);

                // Create a DocumentPaginator for the FlowDocument
                IDocumentPaginatorSource paginatorSource = flowDocument;
                DocumentPaginator documentPaginator = paginatorSource.DocumentPaginator;

                // Send the document to the printer
                printDialog.PrintDocument(documentPaginator, "Order Receipt");
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

        private TableRow CreateEmptyTableRow()
        {
            TableRow row = new TableRow();

            TableCell emptyCell = new TableCell(new Paragraph(new Run(" "))); // Add a space or empty string
            emptyCell.ColumnSpan = 2; // Set the column span to cover both columns

            row.Cells.Add(emptyCell);

            return row;
        }







    }
}