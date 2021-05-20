using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaundryManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string glob_dataPath = "E:\\c_Sharp_Solutions\\LaundryManager\\LaundryManager\\Data\\";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //entrance for the program

            //look through the CustomerDetails.xml file and load all customer numbers into the 
            LoadFindByCustomer();
        }

        private void LoadFindByCustomer()
        {
            //Pull the customer numbers from the customer file and put in the dropdown box
            CustomerDetailsFile customerDetailsFile = new CustomerDetailsFile();
            listBoxFindByCustomer.ItemsSource = customerDetailsFile.GetCustomerNumbers(glob_dataPath + "CustomerDetails.xml");            
        }

        private void ListBoxFindByCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Selection changed");
            //Put the names of customers into the select customer box
            FindMatchingCustomers(listBoxFindByCustomer.SelectedItem.ToString(),1);
        }

        private void FindMatchingCustomers(string searchData, int typeData)
        {

        }



    }
}
