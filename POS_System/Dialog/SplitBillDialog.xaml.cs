using POS_System.Models;
using POS_System.Pages;
using System;
using System.Collections.Generic;
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
        public bool SplitByTotalAmount { get; private set; }
        public int NumberOfPeople { get; private set; }

        public string SplitType { get; private set; }

        double _totalAmount;
        public SplitBillDialog()
        {
            InitializeComponent();
        }
        public SplitBillDialog(double totalAmount)
        {
            InitializeComponent();
            _totalAmount = totalAmount;
          
        }

        private void ButtonTotalAmount_Click(object sender, RoutedEventArgs e)
        {
            ButtonTotalAmount.IsEnabled = false;
            ButtonOrderItems.IsEnabled = false;

            NumberOfPeopleTextBlock.Visibility = Visibility.Visible;
            NumberOfPeopleTextBox.Visibility = Visibility.Visible;
            OKButton.Visibility = Visibility.Visible;
            SplitType = "ByPerHead";
        }

        private void ButtonOrderItems_Click(object sender, RoutedEventArgs e)
        {
            ButtonTotalAmount.IsEnabled = false;
            ButtonOrderItems.IsEnabled = false;

            SplitByTotalAmount = false;
            SplitType = "ByItem";
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(NumberOfPeopleTextBox.Text, out int numberOfPeople))
            {
                if (numberOfPeople <= 0)
                {
                    MessageBox.Show("Please enter a valid number of people.");
                }
                else
                {
                    // Assuming you have a variable named 'totalAmount' that stores the total amount from the menu page
                    double totalAmount = GetTotalAmountFromMenuPage(); // Replace this with the actual method to get the total amount

                    if (totalAmount > 0)
                    {
                        NumberOfPeople = int.Parse(NumberOfPeopleTextBox.Text);
                        double amountPerPerson = totalAmount / numberOfPeople;

                        MessageBox.Show($"Each person owes: {amountPerPerson:C}");


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

        // Replace this with the actual method to get the total amount from your menu page
        private double GetTotalAmountFromMenuPage()
        {
            
            // Implement the logic to retrieve the total amount from your menu page
            return _totalAmount; // total amount from menupage
        }






    }
}
