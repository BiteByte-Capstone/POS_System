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
        public SplitBillDialog(double totalAmount) : this() 
        {
           
            _totalAmount = totalAmount;
          
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

                    if (_totalAmount > 0)
                    {
                        NumberOfPeople = int.Parse(NumberOfPeopleTextBox.Text);
                        double amountPerPerson = _totalAmount / numberOfPeople;

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


    }
}
