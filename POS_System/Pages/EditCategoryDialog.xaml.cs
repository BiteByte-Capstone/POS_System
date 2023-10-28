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
    /// Interaction logic for EditCategoryDialog.xaml
    /// </summary>
    public partial class EditCategoryDialog : Window
    {
        public string EditedCategoryName { get; private set; }

        public EditCategoryDialog(string currentCategoryName)
        {
            InitializeComponent();
            CategoryNameTextBox.Text = currentCategoryName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("SaveButton_Click called");
                EditedCategoryName = CategoryNameTextBox.Text;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
