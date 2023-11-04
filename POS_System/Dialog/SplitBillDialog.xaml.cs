using POS_System.Models;
using POS_System.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace POS_System
{
    /// <summary>
    /// Interaction logic for SplitBillDialog.xaml
    /// </summary>
    public partial class SplitBillDialog : Window
    {
        private ObservableCollection<SplitBill> _splitBills = new ObservableCollection<SplitBill>();
        private ObservableCollection<OrderedItem> _orderedItems = new ObservableCollection<OrderedItem>();
        public bool SplitByTotalAmount { get; private set; }
        public int NumberOfPeople { get; private set; }

        public string SplitType { get; private set ; }

        double _totalAmount;
        public SplitBillDialog()
        {
            InitializeComponent();
            NumberOfPeopleTextBox.Text = string.Empty;
        }
        public SplitBillDialog(ObservableCollection<OrderedItem>orderedItems,double totalAmount) : this() 
        {
            _orderedItems = orderedItems;
            _totalAmount = totalAmount;
          
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumberOfPeopleTextBox.Text, out int numberOfPeople))
            {
                if (numberOfPeople <= 0)
                {
                    MessageBox.Show("Please enter a valid number of people.");
                }
                else
                {

                    if (_totalAmount > 0)
                    {
                        
                        double amountPerPerson = _totalAmount / numberOfPeople;

                        MessageBoxResult result = MessageBox.Show($"Do you want to split into {numberOfPeople} bills?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Menu menu = new Menu();
                            NumberOfPeople = int.Parse(NumberOfPeopleTextBox.Text);

                        }
                        else
                        {
                            return;
                        }


                        DialogResult = true;
                        Close();

                    }
                    else
                    {
                        MessageBox.Show("The total amount is not valid.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid number of people.");
            }
        }

        public ObservableCollection<SplitBill> GetSplitBills()
        {
            
            foreach(OrderedItem orderedItem in _orderedItems)
            {
                SplitBill newSplitBill = new SplitBill
                {
                    // Assuming paymentId and splitType are not critical and can be set to default values.
                    PaymentId = 0,
                    OrderId = orderedItem.order_id,
                    ItemName = orderedItem.item_name,
                    Quantity = orderedItem.Quantity,
                    Price = orderedItem.ItemPrice/ NumberOfPeople,
                    CustomerId =+ 1,
                    SplitType = "ByBill"  // Set a default or calculated value for splitType
                };
                _splitBills.Add(newSplitBill);
            }
            

            return _splitBills;
        }

    }
}
