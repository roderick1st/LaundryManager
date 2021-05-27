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
using System.Windows.Shapes;

namespace LaundryManager
{
    /// <summary>
    /// Interaction logic for AddCustomerForm.xaml
    /// </summary>
    public partial class AddCustomerForm : Window
    {
        string glob_TopComboBoxOption = "Add New Customer";
        string glob_btnActiveContent = "Active Customer";
        string glob_btnRetiredContent = "Retired";

        public event Action<bool> CustomerInfoChange;

        public AddCustomerForm()
        {
            InitializeComponent();
        }

        private void Add_Customer_Window_Loaded(object sender, RoutedEventArgs e)
        {
            //load the know customers into the list box
            PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            CustomerDetailsFile customerDetailsFile = new CustomerDetailsFile();
            List<string> customerNumbersList = new List<string>();
            customerNumbersList = customerDetailsFile.GetCustomerNumbers(false);
            customerNumbersList.Insert(0, glob_TopComboBoxOption);
            cbCustomerNumbers.ItemsSource = customerNumbersList;
            cbCustomerNumbers.SelectedItem = glob_TopComboBoxOption;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddAmend_Click(object sender, RoutedEventArgs e)
        {
            if (cbCustomerNumbers.SelectedIndex == 0)
            {
                //Add new customer
                AddNewCustomer();
            }
            else
            {
                //edit the customer
                EditCustomer();
            }

        }

        private void EditCustomer()
        {
            //get the customer number
            int firstSpace = cbCustomerNumbers.SelectedItem.ToString().IndexOf(" ", 3);
            string stCustomerNumber = cbCustomerNumbers.SelectedItem.ToString().Substring(3, firstSpace - 3);
            List<string> customerDetailsList = new List<string>();
            customerDetailsList.Add(stCustomerNumber);

            customerDetailsList.Add(tbFirstName.Text);
            customerDetailsList.Add(tbLastName.Text);

            int lineCount = 0;
            string notes = tbNotes.Text;

            foreach (string line in tbAddress.Text.Split("\r\n", '\n'))
            {
                lineCount++;
                if (lineCount < 4)
                {
                    customerDetailsList.Add(line);
                }
                else
                {
                    if (lineCount == 4)
                    {
                        notes = notes + "\r\n ADDITIONAL ADDRESS INFORMATION";
                    }
                    notes = notes + "\r\n" + line;
                }
            }
            //fill in blank address lines
            while (customerDetailsList.Count < 6)
            {
                customerDetailsList.Add("");
            }

            customerDetailsList.Add(tbTown.Text);
            customerDetailsList.Add(tbCounty.Text);
            customerDetailsList.Add(tbPostcode.Text);
            customerDetailsList.Add(tbPhone1.Text);
            customerDetailsList.Add(tbPhone2.Text);
            customerDetailsList.Add(tbPhone3.Text);
            customerDetailsList.Add(tbEmail1.Text);
            customerDetailsList.Add(tbEmail2.Text);
            customerDetailsList.Add(tbEmail3.Text);

            if (btnActiveCustomer.Content.ToString() == glob_btnRetiredContent) //active customer?
            {
                customerDetailsList.Add("no");
            }
            else
            {
                customerDetailsList.Add("yes");
            }

            customerDetailsList.Add(notes);
            customerDetailsList.Add(""); //option 2
            customerDetailsList.Add(""); //option 3
            customerDetailsList.Add(""); //option 4
            customerDetailsList.Add(""); //option 5
            customerDetailsList.Add(""); //option 6

            //edit the customer
            CustomerDetailsFile customerDetailsFile = new CustomerDetailsFile();
            customerDetailsFile.EditCustomer(customerDetailsList);

            //reload the combobox
            PopulateComboBox();

            //reload the listbox on the main form EVENT
            if (CustomerInfoChange != null)
            {
                CustomerInfoChange(true);
            }
        }

        private void AddNewCustomer()
        {
            //get the next available customer number
            List<string> customerNumbers = new List<string>();
            CustomerDetailsFile customerDetailsFile = new CustomerDetailsFile();
            customerNumbers = customerDetailsFile.GetCustomerNumbers(false);

            int highestCustomerNumber = 0; //track the highest customer number
            foreach (string customer in customerNumbers)
            {
                int firstSpace = customer.IndexOf(" ", 3);
                string stCustomerNumber = customer.Substring(3, firstSpace - 3);
                if (highestCustomerNumber < Int32.Parse(stCustomerNumber))
                {
                    highestCustomerNumber = Int32.Parse(stCustomerNumber);
                }
            }

            highestCustomerNumber++; //increment customer number by 1

            string notes = tbNotes.Text;

            int lineCount = 0;
            //Make a list to send to the Customer Details file handler
            List<string> customerDetailsList = new List<string>();
            customerDetailsList.Add(highestCustomerNumber.ToString());
            customerDetailsList.Add(tbFirstName.Text);
            customerDetailsList.Add(tbLastName.Text);
            foreach (string line in tbAddress.Text.Split("\r\n", '\n'))
            {
                lineCount++;
                if (lineCount < 4)
                {
                    customerDetailsList.Add(line);
                }
                else
                {
                    if (lineCount == 4)
                    {
                        notes = notes + "\r\n ADDITIONAL ADDRESS INFORMATION";
                    }
                    notes = notes + "\r\n" + line;
                }
            }
            //fill in blank address lines
            while(customerDetailsList.Count < 6)
            {
                customerDetailsList.Add("");
            }

            customerDetailsList.Add(tbTown.Text);
            customerDetailsList.Add(tbCounty.Text);
            customerDetailsList.Add(tbPostcode.Text);
            customerDetailsList.Add(tbPhone1.Text);
            customerDetailsList.Add(tbPhone2.Text);
            customerDetailsList.Add(tbPhone3.Text);
            customerDetailsList.Add(tbEmail1.Text);
            customerDetailsList.Add(tbEmail2.Text);
            customerDetailsList.Add(tbEmail3.Text);

            if(btnActiveCustomer.Content.ToString() == glob_btnRetiredContent) //active customer?
            {
                customerDetailsList.Add("no");
            }
            else
            {
                customerDetailsList.Add("yes");
            }

            customerDetailsList.Add(notes);
            customerDetailsList.Add(""); //option 2
            customerDetailsList.Add(""); //option 3
            customerDetailsList.Add(""); //option 4
            customerDetailsList.Add(""); //option 5
            customerDetailsList.Add(""); //option 6

            //send the list to the handler
            customerDetailsFile.AddNewCustomer(customerDetailsList);

            //reload the combobox
            PopulateComboBox();

            //clear the form
            ClearTheForm();


            //reload the listbox on the main form EVENT
            if (CustomerInfoChange != null)
            {
                CustomerInfoChange(true);
            }
        }

        private void btnActiveCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (btnActiveCustomer.Content.ToString() == glob_btnActiveContent)
            {
                btnActiveCustomer.Content = glob_btnRetiredContent;
            }
            else
            {
                btnActiveCustomer.Content = glob_btnActiveContent;
            }
        }

        private void cbCustomerNumbers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //find the details relating to that customer
            if (cbCustomerNumbers.SelectedItem != null)
            {
                if (cbCustomerNumbers.SelectedItem.ToString() != glob_TopComboBoxOption)
                {
                    List<string> customerDetails = new List<string>();
                    CustomerDetailsFile customerDetailsFile = new CustomerDetailsFile();
                    customerDetails = customerDetailsFile.GetSelectedCustomerDetails(cbCustomerNumbers.SelectedItem.ToString());

                    //populate the form
                    tbFirstName.Text = customerDetails[1];
                    tbLastName.Text = customerDetails[2];
                    tbAddress.Text = customerDetails[3] + "\r\n" + customerDetails[4] + "\r\n" + customerDetails[5];
                    tbTown.Text = customerDetails[6];
                    tbCounty.Text = customerDetails[7];
                    tbPostcode.Text = customerDetails[8];
                    tbPhone1.Text = customerDetails[9];
                    tbPhone2.Text = customerDetails[10];
                    tbPhone3.Text = customerDetails[11];
                    tbEmail1.Text = customerDetails[12];
                    tbEmail2.Text = customerDetails[13];
                    tbEmail3.Text = customerDetails[14];

                    if (customerDetails[15] == "yes")
                    {
                        btnActiveCustomer.Content = glob_btnActiveContent;
                    }
                    else
                    {
                        btnActiveCustomer.Content = glob_btnRetiredContent;
                    }

                    tbNotes.Text = customerDetails[16];
                }
                else
                {
                    ClearTheForm();
                   
                }
            }
            
        }

        private void ClearTheForm()
        {
            tbFirstName.Clear();
            tbLastName.Clear();
            tbAddress.Clear();
            tbTown.Clear();
            tbCounty.Clear();
            tbPostcode.Clear();
            tbPhone1.Clear();
            tbPhone2.Clear();
            tbPhone3.Clear();
            tbEmail1.Clear();
            tbEmail2.Clear();
            tbEmail3.Clear();
            tbNotes.Clear();
            btnActiveCustomer.Content = glob_btnActiveContent;
            cbCustomerNumbers.SelectedIndex = 0;
        }
    }
}
