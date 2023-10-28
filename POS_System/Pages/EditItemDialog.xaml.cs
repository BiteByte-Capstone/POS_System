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
    public partial class EditItemDialog : Window
    {
        string connectionString = "SERVER=localhost;DATABASE=pos_db;UID=root;PASSWORD=password;";
        public int editedId { get; set; } // Change 'private set' to 'set'
        public string editedName { get; set; }
        public double editedPrice { get; set; }
        public string editedDescripion { get; set; }
        public string editedCategory { get; set; }

        public EditItemDialog(int currentId,string currentName, double currentPrice, string currentDescription, string currentCategory)
        {
            InitializeComponent();
            CurrentIdTextBox.Text = currentId.ToString();
            CurrentItemNameTextBlock.Text = currentName;
            CurrentItemPriceTextBlock.Text = currentPrice.ToString();
            CurrentDescriptionTextBlock.Text = currentDescription;
            CurrentCategoryTextBlock.Text = currentCategory;


        }

        private void SaveItemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                editedId = int.Parse(EditedIdTextBox.Text);
                editedName = EditedNameTextBox.Text;
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void CancelItemButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


    }
}
