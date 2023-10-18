using POS.Models;
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

namespace POS_System.Pages
{
    /// <summary>
    /// Interaction logic for AdminManagement.xaml
    /// </summary>
    public partial class AdminManagement : Window
    {
        public AdminManagement()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Open the Admin window when the button is clicked
            AdminPage adminWindow = new AdminPage();
            adminWindow.Show();
        }
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            // Open the Admin window when the button is clicked
            TablePage adminWindow = new TablePage();
            adminWindow.Show();
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            // Open the Admin window when the button is clicked
            MenuPage adminWindow = new MenuPage();
            adminWindow.Show();
        }
    }
}