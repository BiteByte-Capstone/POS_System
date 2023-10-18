using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
using System.Xml.Schema;

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for PaymentPage.xaml
    /// </summary>
    public partial class PaymentPage : Window
    {
        String paymentMethod;
        Double totalOrderAmount;
        Double customerPaymentAmount;
        Double change;
        Double tips;//customerPaymentAmount - totalOrderAmount - change;
        
        public PaymentPage()
        {
            InitializeComponent();
            customerPaymentAmount = Double.Parse(customerPayTextBox.Text);
            change = Double.Parse(changeTextBox.Text);
            tips = customerPaymentAmount - totalOrderAmount - change;
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            MenuPage menuPage = new MenuPage();
            menuPage.Show();
            this.Close();
        }

        private void cashBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "cash";
            cashBtn.Background = Brushes.White;
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));

        }

        private void visaBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "visa";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = Brushes.White;
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
        }

        private void mcBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "mastercard";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = Brushes.White;
            amexBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
        }

        private void amexBtn_Click(object sender, RoutedEventArgs e)
        {
            paymentMethod = "amex";
            cashBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            visaBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            mcBtn.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF4C4B56"));
            amexBtn.Background = Brushes.White;
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void customerPayTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
