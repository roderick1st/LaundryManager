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

        //string glob_dataPath = "E:\\c_Sharp_Solutions\\LaundryManager\\LaundryManager\\Data\\";
        List<string> glob_CurrentCustomerInfo = new List<string>();
        bool glob_addedAdditionalMessage = false;
        string glob_EmailMessage = "";
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
            listBoxFindByCustomer.ItemsSource = customerDetailsFile.GetCustomerNumbers(true);            
        }

        private void ListBoxFindByCustomer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Put the names of customers into the select customer box
            if (listBoxFindByCustomer.SelectedItem != null)
            {
                FindMatchingCustomers(listBoxFindByCustomer.SelectedItem.ToString(), 1);
            }
        }

        private void FindMatchingCustomers(string searchData, int typeData)
        {
            Debug.WriteLine("Customer Selected: " + searchData);
            if (typeData == 1) //we know there is only one customer so just display that customer then move on to populate the details box
            {
                listBoxSelectCustomer.Items.Clear(); //clear the list box
                listBoxSelectCustomer.Items.Add(searchData); //add the customer selected
                DisplayMatchingCustomerDetails(searchData); //display the customers data
            }

            //clear the other windows and info
            textBlockSendStatus.Text="";
            textBoxAdditionalMessage.Clear();
        }

        private void DisplayMatchingCustomerDetails(string selectedCustomer)
        {
            CustomerDetailsFile customerDetailsFile = new CustomerDetailsFile();
            glob_CurrentCustomerInfo = customerDetailsFile.GetSelectedCustomerDetails(selectedCustomer);

            string customerInformation = "CN";

            customerInformation = customerInformation + glob_CurrentCustomerInfo[0] + "\r\n"; //customernumber
            customerInformation = customerInformation + glob_CurrentCustomerInfo[1] + " " + glob_CurrentCustomerInfo[2] + "\r\n"; //first name and last name

            //get the rest through a loop up to line 12
            for(int i = 3; i < 12; i++)
            {
                customerInformation = customerInformation + glob_CurrentCustomerInfo[i] + "\r\n";
            }

            textBlockCustomerDetails.Text = customerInformation;
            textBoxCustNotes.Text = glob_CurrentCustomerInfo[16];

            //clear the listbox
            listBoxEmailAddress.Items.Clear();

            //load the email address
            for(int i = 12; i < 15; i++)
            {
                if (glob_CurrentCustomerInfo[i].Length > 0)
                {
                    listBoxEmailAddress.Items.Add(glob_CurrentCustomerInfo[i]);
                }

            }

            //select all the emails
            for (int ii=0;ii<listBoxEmailAddress.Items.Count; ii++)
            {
                listBoxEmailAddress.SelectAll();
            }

            //set the listbox to have focus
            listBoxEmailAddress.Focus();

            glob_EmailMessage = "Dear " + glob_CurrentCustomerInfo[1] + ", \r\n";
            glob_EmailMessage = glob_EmailMessage + "\r\n";
            glob_EmailMessage = glob_EmailMessage + "We pleased to tell you that your laundry has been completed.";
            glob_EmailMessage = glob_EmailMessage + "\r\n\r\n";
            glob_EmailMessage = glob_EmailMessage + "Regards, \r\n";
            glob_EmailMessage = glob_EmailMessage + "\r\n";
            glob_EmailMessage = glob_EmailMessage + "The Team at Wolds Laundry Services";
            textBlockMessageToSend.Text = glob_EmailMessage;
        }

        private void textBoxAdditionalMessage_TextChanged(object sender, EventArgs e)
        {
            string newEMailMessage = "";
            if ((textBoxAdditionalMessage.Text.Length > 0) | (glob_addedAdditionalMessage))//if we have text
            {
                if (!glob_addedAdditionalMessage)
                {
                    glob_EmailMessage = glob_EmailMessage + "\r\n\r\nADDITIONAL DETAILS\r\n\r\n";
                    glob_addedAdditionalMessage = true;
                }             
                newEMailMessage = textBoxAdditionalMessage.Text;
                textBlockMessageToSend.Text = glob_EmailMessage + newEMailMessage;
            }
            if ((glob_addedAdditionalMessage) & (textBoxAdditionalMessage.Text.Length < 1))//we need to remove the additional message
            {
                glob_EmailMessage = glob_EmailMessage.Substring(0, glob_EmailMessage.Length - 26);
                textBlockMessageToSend.Text = glob_EmailMessage;
                glob_addedAdditionalMessage = false;
            }
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e)
        {
            List<string> custEmails = new List<string>();
            foreach (string cEmail in listBoxEmailAddress.Items)
            {
                custEmails.Add(cEmail);
            }
            EMailServices eMailServices = new EMailServices();
            textBlockSendStatus.Text = eMailServices.SendEmail(custEmails, "Your Laundry is Done!", textBlockMessageToSend.Text);

        }

        private void buttonAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            AddCustomerForm addCustomerForm = new AddCustomerForm();
            addCustomerForm.CustomerInfoChange += AddCustomerForm_CustomerInfoChange;
            addCustomerForm.ShowDialog();

            //using (FormOptions formOptions = new FormOptions())
            //{
                // passing this in ShowDialog will set the .Owner 
                // property of the child form
                //formOptions.ShowDialog(this);
            //}
            //In the child form, use this code to pass a value back to the parent:

            //ParentForm parent = (ParentForm)this.Owner;
            //parent.NotifyMe("whatever");

        }

        private void AddCustomerForm_CustomerInfoChange(bool obj)
        {
            LoadFindByCustomer();
        }
    }
}
