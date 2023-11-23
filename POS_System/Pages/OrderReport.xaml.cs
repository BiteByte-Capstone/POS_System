using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
using POS_System.Models;

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class OrderReport : Window
    {

        string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

        public OrderReport()
        {
            InitializeComponent();
            getDataOrderTable();
            getDataOrderedListTable();
            GetDataRefundTable();
        }

        private void getDataOrderTable()
        {
            //Tutorial used https://www.youtube.com/watch?v=OPDPI5exPp8

            //db = new DatabaseHelper("localhost", "pos_db", "root", "password");

            //String to make connection to database


            //Create a connection object
            MySqlConnection connection = new MySqlConnection(connectionString);

            //SQL query
            MySqlCommand cmd = new MySqlCommand("select * from pos_db.order order by 3", connection);

            //Open up connection with the user table
            connection.Open();

            //create a datatable object to capture the database table
            DataTable dt = new DataTable();

            //Execute the command and the load the result of reader inside the datatable
            dt.Load(cmd.ExecuteReader());

            //Close connection to user table
            connection.Close();

            //Bind data table to the DataGrid on XAML
            orderGrid.DataContext = dt;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Tutorial used https://www.youtube.com/watch?v=qZwT_NWQ6Mk&t=14s   

            var row = sender as DataGridRow;
            var order = row.DataContext as Order;

            MessageBox.Show(order.OrderId.ToString());
        }

        private void filterBtnOrder_Click(object sender, RoutedEventArgs e)
        {
            String fromD = fromDateOrder.ToString();
            String untilD = untilDateOrder.ToString();
            String specificTable = specificTableBoxFilter.Text;
            String orderType = OrderTypeComboBox.Text;
            String orderStatus = OrderStatusComboBox.Text;
            String fromAmount = fromAmountBoxFilter.Text;
            String toAmount = toAmountBoxFilter.Text;

            //String to be added to sql query
            String specificTableQuery = "";
            String orderTypeQuery = "";
            String orderStatusQuery = "";
            String fromToAmountQuery = "";
            String fromToDateQuery = "";

            int[] lengthcount = new int[5];

            String sqlquery = "select * from pos_db.order where ";

            if (fromD.Length + untilD.Length + specificTable.Length + orderType.Length + orderStatus.Length + fromAmount.Length + toAmount.Length < 1)
            {
                getDataOrderTable();
            }
            else
            {

                //This part checks for the filter that was entered by counting the length of the filter inputs.
                //If length of filter input > 0, then that filter was used, which then triggers the formulation of a sql query for that filter
                if (specificTable.Length > 0)
                {
                    specificTableQuery = SpecificTableFilter(specificTable);
                    lengthcount[0] = 1;

                }

                if (orderType.Length > 0)
                {
                    orderTypeQuery = OrderTypeFilter(orderType);
                    lengthcount[1] = 1;
                }

                if (orderStatus.Length > 0)
                {
                    orderStatusQuery = OrderStatusFilter(orderStatus);
                    lengthcount[2] = 1;
                }

                if (fromAmount.Length > 0 || toAmount.Length > 0)
                {
                    fromToAmountQuery = FromToAmountFilter(fromAmount, toAmount);
                    lengthcount[3] = 1;
                }

                if (fromD.Length > 0 || untilD.Length > 0)
                {
                    fromToDateQuery = FromToDateFilterOrderReport(fromD, untilD);
                    lengthcount[4] = 1;
                }

                //This part of the code will build the sqlquery to be executed, by adding segments of sqlquery from the filter that was used

                if (lengthcount[0] == 1)
                {
                    sqlquery += specificTableQuery;
                }

                if (lengthcount[1] == 1 && lengthcount[0] == 1)
                {
                    sqlquery += " and " + orderTypeQuery;
                }
                else
                {
                    sqlquery += orderTypeQuery;
                }

                if (lengthcount[2] == 1 && ((lengthcount[0] + lengthcount[1] > 0)))
                {
                    sqlquery += " and " + orderStatusQuery;
                }
                else
                {
                    sqlquery += orderStatusQuery;
                }

                if (lengthcount[3] == 1 && ((lengthcount[0] + lengthcount[1] + lengthcount[2] > 0)))
                {
                    sqlquery += " and " + fromToAmountQuery;
                }
                else
                {
                    sqlquery += fromToAmountQuery;
                }

                if (lengthcount[4] == 1 && ((lengthcount[0] + lengthcount[1] + lengthcount[2] + lengthcount[3] > 0)))
                {
                    sqlquery += " and " + fromToDateQuery;
                }
                else
                {
                    sqlquery += fromToDateQuery;
                }

                sqlquery = sqlquery + " order by 6;";

                OrderTypeComboBox.Text = "";
                OrderStatusComboBox.Text = "";

                //Show the sql query string to be executed
                MessageBox.Show(sqlquery);

                //String to make connection to database
                string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

                //Create a connection object
                MySqlConnection connection = new MySqlConnection(connectionString);

                //SQL query
                MySqlCommand cmd = new MySqlCommand(sqlquery, connection);

                //Open up connection with the user table
                connection.Open();

                //create a datatable object to capture the database table
                DataTable dt = new DataTable();

                //Execute the command and the load the result of reader inside the datatable
                dt.Load(cmd.ExecuteReader());

                //Close connection to user table
                connection.Close();

                //Bind data table to the DataGrid on XAML
                orderGrid.DataContext = dt;

            }
        }

        private String FromToDateFilterOrderReport(String fromD, String untilD)
        {
            String fromToDateFilterString = "";

            String fromFormatted = "";
            String toFormatted = "";
            String fromReplace = "";
            String toReplace = "";
            String[] fromSplit;
            String[] toSplit;

            //input validation. if input not empty, proceed to format input string to be used in sql query
            if (fromD.Length > 0)
            {
                fromReplace = fromD.Replace('/', ' ');
                fromSplit = fromReplace.Split(' ');
                fromFormatted = fromSplit[2] + '-' + fromSplit[0] + '-' + fromSplit[1].ToString();
            }

            //input validation. if input not empty, proceed to format input string to be used in sql query
            if (untilD.Length > 0)
            {
                toReplace = untilD.Replace('/', ' ');
                toSplit = toReplace.Split(' ');
                toFormatted = toSplit[2] + '-' + toSplit[0] + '-' + toSplit[1].ToString();

            }

            //sql query formulation based on selected filter

            //when only the until-date is selected
            if (fromFormatted.Length > 0 && toFormatted.Length < 1)
            {
                fromToDateFilterString = " order_timestamp >= '" + fromFormatted + "'";

                //when only the from-date is selected
            }
            else if (toFormatted.Length > 0 && fromFormatted.Length < 1)
            {
                fromToDateFilterString = " order_timestamp <= '" + toFormatted + "' + interval 1 day";

                //when a date-range is selected
            }
            else
            {
                fromToDateFilterString = " order_timestamp between '" + fromFormatted + "' and '" + toFormatted + "' + interval 1 day";
            }

            return fromToDateFilterString;
        }

        private String SpecificTableFilter(String specificTable)
        {
            String specficTableFilterString;

            if (specificTable.Contains(","))
            {
                specficTableFilterString = "table_num in ('" + specificTable.Replace(",", "','") + "')";
            }
            else if (specificTable.Contains(" "))
            {
                specficTableFilterString = "table_num in ('" + specificTable.Replace(" ", "','") + "')";
            }
            else
            {
                specficTableFilterString = "table_num = '" + specificTable + "'";
            }

            return specficTableFilterString;
        }

        private String OrderTypeFilter(String orderType)
        {
            String orderTypeFilterString = "";

            if (orderType.Contains("Dine-In"))
            {
                orderTypeFilterString = "order_type = 'Dine-In'";
            }
            else
            {
                orderTypeFilterString = "order_type = 'Take-Out'";
            }
            return orderTypeFilterString;
        }

        private String OrderStatusFilter(String orderStatus)
        {
            String orderStatusFilterString = "";

            if (orderStatus.Contains("Canceled"))
            {
                orderStatusFilterString = "paid = 'c'";
            }
            else if (orderStatus.Contains("Paid"))
            {
                orderStatusFilterString = "paid = 'y'";
            }
            else
            {
                orderStatusFilterString = "paid = 'n'";
            }
            return orderStatusFilterString;
        }

        private String FromToAmountFilter(String fromAmount, String toAmount)
        {
            String fromToAmountFilterString = "";

            if (fromAmount.Length > 0 && toAmount.Length > 0)
            {
                fromToAmountFilterString = "total_amount between " + fromAmount + " and " + toAmount;
            }
            else if (fromAmount.Length > 0 && toAmount.Length < 1)
            {
                fromToAmountFilterString = "total_amount >= " + fromAmount;
            }
            else if (fromAmount.Length < 1 && toAmount.Length > 0)
            {
                fromToAmountFilterString = "total_amount <= " + toAmount;
            }

            return fromToAmountFilterString;
        }

        private void printBtnOrder_Click(object sender, RoutedEventArgs e)
        {
            //Tutorial used https://www.youtube.com/watch?v=z7SZsmSjsfM minute 19 onwards

            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(printReport, "Order Report");
                }
            }
            catch (Exception ex)
            {

            }
        }

        /* OrderedItemReport.xaml.cs */
        private void getDataOrderedListTable()
        {
            //Tutorial used https://www.youtube.com/watch?v=OPDPI5exPp8

            //db = new DatabaseHelper("localhost", "pos_db", "root", "password");

            //String to make connection to database
            string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

            //Create a connection object
            MySqlConnection connection = new MySqlConnection(connectionString);

            //SQL query
            MySqlCommand cmd = new MySqlCommand("SELECT oi.order_id, i.item_category, oi.item_id, i.item_name, oi.item_price, oi.quantity, o.order_timestamp FROM ordered_itemlist oi JOIN item i ON oi.item_id = i.item_id JOIN pos_db.order o on oi.order_id = o.order_id order by 7;", connection);

            //Open up connection with the user table
            connection.Open();

            //create a datatable object to capture the database table
            DataTable dt = new DataTable();

            //Execute the command and the load the result of reader inside the datatable
            dt.Load(cmd.ExecuteReader());

            //Close connection to user table
            connection.Close();

            //Bind data table to the DataGrid on XAML
            orderedItemListGrid.DataContext = dt;
        }

        //private void DataGridRow_MouseDoubleClickItem(object sender, MouseButtonEventArgs e)
        //{
        //Tutorial used https://www.youtube.com/watch?v=qZwT_NWQ6Mk&t=14s   

        //  var row = sender as DataGridRow;
        //var order = row.DataContext as Order;

        //MessageBox.Show(order.OrderId.ToString());
        //}

        private void filterBtnItem_Click(object sender, RoutedEventArgs e)
        {
            String fromD = fromDateItem.ToString();
            String untilD = untilDateItem.ToString();
            String orderID = orderIdBoxFilter.Text;
            String itemID = itemIdBoxFilter.Text;
            String category = categoryBoxFilter.Text;
            String fromQuantity = fromQuantityBoxFilter.Text;
            String toQuantity = toQuantityBoxFilter.Text;
            String fromPrice = fromPriceBoxFilter.Text;
            String toPrice = toPriceBoxFilter.Text;

            //String to be added to sql query
            String orderIdQuery = "";
            String itemIdQuery = "";
            String categoryQuery = "";
            String fromToQuantityQuery = "";
            String fromToPriceQuery = "";
            String fromToDateQuery = "";

            int[] lengthcount = new int[6];

            String sqlquery = "SELECT oi.order_id, i.item_category, oi.item_id, i.item_name, oi.item_price, oi.quantity, o.order_timestamp FROM ordered_itemlist oi JOIN item i ON oi.item_id = i.item_id JOIN pos_db.order o on oi.order_id = o.order_id where ";

            if (fromD.Length + untilD.Length + orderID.Length + itemID.Length + category.Length + fromQuantity.Length + toQuantity.Length + fromPrice.Length + toPrice.Length < 1)
            {
                getDataOrderedListTable();
            }
            else
            {

                //This part checks for the filter that was entered by counting the length of the filter inputs.
                //If length of filter input > 0, then that filter was used, which then triggers the formulation of a sql query for that filter
                if (orderID.Length > 0)
                {
                    orderIdQuery = OrderIdFilter(orderID);
                    lengthcount[0] = 1;
                }

                if (itemID.Length > 0)
                {
                    itemIdQuery = ItemIdFilter(itemID);
                    lengthcount[1] = 1;
                }

                if (category.Length > 0)
                {
                    categoryQuery = CategoryFilter(category);
                    lengthcount[2] = 1;
                }

                if (fromQuantity.Length > 0 || toQuantity.Length > 0)
                {
                    fromToQuantityQuery = FromToQuantityFilter(fromQuantity, toQuantity);
                    lengthcount[3] = 1;
                }

                if (fromPrice.Length > 0 || toPrice.Length > 0)
                {
                    fromToPriceQuery = FromToPriceFilter(fromPrice, toPrice);
                    lengthcount[4] = 1;
                }

                if (fromD.Length > 0 || untilD.Length > 0)
                {
                    fromToDateQuery = FromToDateFilter(fromD, untilD);
                    lengthcount[5] = 1;
                }

                //This part of the code will build the sqlquery to be executed, by adding segments of sqlquery from the filter that was used
                if (lengthcount[0] == 1)
                {
                    sqlquery += orderIdQuery;
                }

                if (lengthcount[1] == 1 && lengthcount[0] == 1)
                {
                    sqlquery += " and " + itemIdQuery;
                }
                else
                {
                    sqlquery += itemIdQuery;
                }

                if (lengthcount[2] == 1 && ((lengthcount[0] + lengthcount[1] > 0)))
                {
                    sqlquery += " and " + categoryQuery;
                }
                else
                {
                    sqlquery += categoryQuery;
                }

                if (lengthcount[3] == 1 && ((lengthcount[0] + lengthcount[1] + lengthcount[2] > 0)))
                {
                    sqlquery += " and " + fromToQuantityQuery;
                }
                else
                {
                    sqlquery += fromToQuantityQuery;
                }

                if (lengthcount[4] == 1 && ((lengthcount[0] + lengthcount[1] + lengthcount[2] + lengthcount[1] > 0)))
                {
                    sqlquery += " and " + fromToPriceQuery;
                }
                else
                {
                    sqlquery += fromToPriceQuery;
                }

                if (lengthcount[5] == 1 && ((lengthcount[0] + lengthcount[1] + lengthcount[2] + lengthcount[3] + lengthcount[4] > 0)))
                {
                    sqlquery += " and " + fromToDateQuery;
                }
                else
                {
                    sqlquery += fromToDateQuery;
                }

                sqlquery = sqlquery + " order by 7;";

                //Show the sql query string to be executed
                MessageBox.Show(sqlquery);

                //String to make connection to database
                string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";

                //Create a connection object
                MySqlConnection connection = new MySqlConnection(connectionString);

                //SQL query
                MySqlCommand cmd = new MySqlCommand(sqlquery, connection);

                //Open up connection with the user table
                connection.Open();

                //create a datatable object to capture the database table
                DataTable dt = new DataTable();

                //Execute the command and the load the result of reader inside the datatable
                dt.Load(cmd.ExecuteReader());

                //Close connection to user table
                connection.Close();

                //Bind data table to the DataGrid on XAML
                orderedItemListGrid.DataContext = dt;

            }


        }

        private String FromToDateFilter(String fromD, String untilD)
        {
            String fromToDateFilterString = "";

            String fromFormatted = "";
            String toFormatted = "";
            String fromReplace = "";
            String toReplace = "";
            String[] fromSplit;
            String[] toSplit;

            //input validation. if input not empty, proceed to format input string to be used in sql query
            if (fromD.Length > 0)
            {
                fromReplace = fromD.Replace('/', ' ');
                fromSplit = fromReplace.Split(' ');
                fromFormatted = fromSplit[2] + '-' + fromSplit[0] + '-' + fromSplit[1].ToString();
            }

            //input validation. if input not empty, proceed to format input string to be used in sql query
            if (untilD.Length > 0)
            {
                toReplace = untilD.Replace('/', ' ');
                toSplit = toReplace.Split(' ');
                toFormatted = toSplit[2] + '-' + toSplit[0] + '-' + toSplit[1].ToString();

            }

            //sql query formulation based on selected filter

            //when only the until-date is selected
            if (fromFormatted.Length > 0 && toFormatted.Length < 1)
            {
                fromToDateFilterString = " order_timestamp >= '" + fromFormatted + "'";

                //when only the from-date is selected
            }
            else if (toFormatted.Length > 0 && fromFormatted.Length < 1)
            {
                fromToDateFilterString = " order_timestamp <= '" + toFormatted + "' + interval 1 day";

                //when a date-range is selected
            }
            else
            {
                fromToDateFilterString = " order_timestamp between '" + fromFormatted + "' and '" + toFormatted + "' + interval 1 day";
            }

            return fromToDateFilterString;
        }

        private String OrderIdFilter(String orderID)
        {
            String orderIdFilterString;

            if (orderID.Contains(","))
            {
                orderIdFilterString = "o.order_id in (" + orderID + ")";
            }
            else if (orderID.Contains(" "))
            {
                orderIdFilterString = "o.order_id in (" + orderID.Replace(" ", ",") + ")";
            }
            else
            {
                orderIdFilterString = "o.order_id = " + orderID;
            }

            return orderIdFilterString;
        }

        private String ItemIdFilter(String itemID)
        {
            String itemIdFilterString;

            if (itemID.Contains(','))
            {
                itemIdFilterString = "i.item_id in (" + itemID + ")";
            }
            else if (itemID.Contains(' '))
            {
                itemIdFilterString = "i.item_id in (" + itemID.Replace(' ', ',') + ")";
            }
            else
            {
                itemIdFilterString = "i.item_id = " + itemID;
            }

            return itemIdFilterString;
        }

        private String CategoryFilter(String category)
        {
            String categoryFilterString;

            if (category.Contains(','))
            {
                categoryFilterString = "i.item_category in ('" + category.Replace(",", "','") + "')";
            }
            else if (category.Contains(' '))
            {
                categoryFilterString = "i.item_category in ('" + category.Replace(" ", "','") + "')";
            }
            else
            {
                categoryFilterString = "i.item_category = '" + category + "'";
            }

            return categoryFilterString;
        }

        private String FromToQuantityFilter(String fromQuantity, String toQuantity)
        {
            String fromToQuantityFilterString = "";

            if (fromQuantity.Length > 0 && toQuantity.Length > 0)
            {
                fromToQuantityFilterString = "oi.quantity between " + fromQuantity + " and " + toQuantity;
            }
            else if (fromQuantity.Length > 0 && toQuantity.Length < 1)
            {
                fromToQuantityFilterString = "oi.quantity >= " + fromQuantity;
            }
            else if (fromQuantity.Length < 1 && toQuantity.Length > 0)
            {
                fromToQuantityFilterString = "oi.quantity <= " + toQuantity;
            }

            return fromToQuantityFilterString;
        }

        private String FromToPriceFilter(String fromPrice, String toPrice)
        {
            String fromToPriceFilterString = "";

            if (fromPrice.Length > 0 && toPrice.Length > 0)
            {
                fromToPriceFilterString = "oi.item_price between " + fromPrice + " and " + toPrice;
            }
            else if (fromPrice.Length > 0 && toPrice.Length < 1)
            {
                fromToPriceFilterString = "oi.item_price >= " + fromPrice;
            }
            else if (fromPrice.Length < 1 && toPrice.Length > 0)
            {
                fromToPriceFilterString = "oi.item_price <= " + toPrice;
            }

            return fromToPriceFilterString;
        }

        private void printBtnItem_Click(object sender, RoutedEventArgs e)
        {
            //Tutorial used https://www.youtube.com/watch?v=z7SZsmSjsfM minute 19 onwards

            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(printReport, "Order Report");
                }
            }
            catch (Exception ex)
            {

            }
        }

        /* RefundReport.xaml.cs */
        private void GetDataRefundTable()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM refund", connection);

                connection.Open();
                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                connection.Close();

                refundGrid.DataContext = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void FilterBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fromTimestamp = refundFromDateFilter.SelectedDate?.ToString("yyyy-MM-dd");
                string untilTimestamp = refundToDateFilter.SelectedDate?.ToString("yyyy-MM-dd");
                string refundId = refundIdFilter.Text;
                string orderId = orderIdFilter.Text;
                string paymentId = paymentIdFilter.Text;
                string refundAmount = refundAmountFilter.Text;
                string specificMethod = refundMethodFilter.Text;
                string specificUser = userIdFilter.Text;


                if (string.IsNullOrEmpty(fromTimestamp) && string.IsNullOrEmpty(untilTimestamp) && string.IsNullOrEmpty(specificMethod) && string.IsNullOrEmpty(specificUser) && string.IsNullOrEmpty(refundId) && string.IsNullOrEmpty(orderId) && string.IsNullOrEmpty(paymentId) && string.IsNullOrEmpty(refundAmount))
                {
                    GetDataRefundTable();
                }
                else
                {
                    string sqlQuery = "SELECT * FROM refund WHERE 1=1";

                    if (!string.IsNullOrEmpty(fromTimestamp) && !string.IsNullOrEmpty(untilTimestamp))
                    {
                        sqlQuery += " AND refund_timestamp BETWEEN @fromTimestamp AND @untilTimestamp + interval 1 day";
                    }
                    else if (!string.IsNullOrEmpty(fromTimestamp))
                    {
                        sqlQuery += " AND refund_timestamp >= @fromTimestamp";
                    }
                    else if (!string.IsNullOrEmpty(untilTimestamp))
                    {
                        sqlQuery += " AND refund_timestamp <= @untilTimestamp + interval 1 day";
                    }

                    if (!string.IsNullOrEmpty(specificMethod))
                    {
                        sqlQuery += " AND refund_method = @specificMethod";
                    }

                    if (!string.IsNullOrEmpty(specificUser))
                    {
                        sqlQuery += " AND user_id = @specificUser";
                    }

                    if (!string.IsNullOrEmpty(refundId))
                    {
                        sqlQuery += " AND refund_id = @refundId";
                    }

                    if (!string.IsNullOrEmpty(orderId))
                    {
                        sqlQuery += " AND order_id = @orderId";
                    }

                    if (!string.IsNullOrEmpty(paymentId))
                    {
                        sqlQuery += " AND payment_id = @paymentId";
                    }

                    if (!string.IsNullOrEmpty(refundAmount))
                    {
                        sqlQuery += " AND refund_amount = @refundAmount";
                    }

                    MySqlConnection connection = new MySqlConnection(connectionString);
                    MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);

                    MessageBox.Show(sqlQuery);

                    // Add parameters for user inputs        
                    cmd.Parameters.AddWithValue("@fromTimestamp", fromTimestamp);
                    cmd.Parameters.AddWithValue("@untilTimestamp", untilTimestamp);
                    cmd.Parameters.AddWithValue("@specificMethod", specificMethod);
                    cmd.Parameters.AddWithValue("@specificUser", specificUser);
                    cmd.Parameters.AddWithValue("@refundId", refundId);
                    cmd.Parameters.AddWithValue("@orderId", orderId);
                    cmd.Parameters.AddWithValue("@paymentId", paymentId);
                    cmd.Parameters.AddWithValue("@refundAmount", refundAmount);

                    connection.Open();
                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());
                    connection.Close();

                    refundGrid.DataContext = dt;

                    refundMethodFilter.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(refundGrid, "Refund Report");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}