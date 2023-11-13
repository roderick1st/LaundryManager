using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Xml;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Reflection;

namespace LaundryManager
{
    /// <summary>
    /// Interaction logic for Billing.xaml
    /// </summary>
    public partial class Billing : Window
    {

        string glob_ticketsFilePath = "";
        string glob_PriceListsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\PriceLists";
        string glob_CustomerDetailsXML = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\CustomerDetails.xml";
        DataTable gridTableOfBillibletickets = new();
        DataTable glob_PriceListsTable;
        DataTable glob_TicketIssues = new();
        DataSet unbilledTicketsDataSet = new(); //tickets that are unbilled
        DataSet billableTickets = new(); //tickets within the date range that are unbilled
        //DataSet finnishedTickets = new(); //because we need a new table after adding items in billabletickets
        DataSet customerDetails = new();
        DataTable priceListUsed = new();
        bool firstRun = true;

        //glob constants
        const string CONST_noDelPrice = "Delivery included but there is no price";
        const string CONST_noXeroCode = "Item has no Xero code set";
        const string CONST_emptyTicket = "Ticket is Empty";
        const string CONST_ok = "OK";

        const string CONST_discountCode = "200-999";
        const string CONST_vatLable = "No VAT";



        public Billing()
        {
            InitializeComponent();
            glob_ticketsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\Tickets";
        }

        private void DataGridRowHeader_Click(object sender, RoutedEventArgs e)
        {
            

        }

        private bool AssignTicketIssueToTable(string ticketNum, string issue , int callingID)
        {
            //calling ID is for debug
            DataRow newIssueRow = glob_TicketIssues.NewRow();
            newIssueRow["TicketNum"] = ticketNum.ToString();
            newIssueRow["Issue"] = issue;
            glob_TicketIssues.Rows.Add(newIssueRow);
            return true;
        }

        private void Billing_Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            BuildTicketIssuesTable();
            SortDateOut();
            LoadPriceLists();
            LoadCustomerInformation();
            LoadTicketsIntoDataSet();
            PopulateBilledTicketsDataGrid();
            firstRun = false;
        }

        private void BuildTicketIssuesTable()
        {
            DataColumn ticketNumCol = new DataColumn("TicketNum", typeof(string));
            DataColumn ticketItemIssue = new DataColumn("Issue", typeof(string));

            glob_TicketIssues.Columns.Add(ticketNumCol);
            glob_TicketIssues.Columns.Add(ticketItemIssue);
        }

        private void LoadCustomerInformation()
        {
            //we need to pull the customer database xml apart and put it into a dataset
            XmlDocument customerInformation = new XmlDocument();
            customerInformation.Load(glob_CustomerDetailsXML);
            XmlNodeList customerNodes = customerInformation.GetElementsByTagName("Customer");
            foreach (XmlNode customerNode in customerNodes)
            {
                //create a new table for the dataset
                DataTable customerTable = new(); //new datatable
                customerTable.Columns.AddRange(new DataColumn[2] { new DataColumn("Property"), new DataColumn("Value")}); //add the columns

                foreach(XmlNode childNode in customerNode.ChildNodes) //loop through each child node of the customer
                {
                    DataRow row = customerTable.NewRow();
                    row["Property"] = childNode.Name;
                    row["Value"] = childNode.InnerText;

                    if(childNode.Name == "CN")//change the name of the table
                    {
                        customerTable.TableName = childNode.Name + childNode.InnerText;
                    }

                    customerTable.Rows.Add(row);
                }

                //add the table to the dataset
                customerDetails.Tables.Add(customerTable);

            }
        }

        private void ClearDataSets()
        {
            int tableCount = billableTickets.Tables.Count;
            //billableTickets.Clear();

            for (int i = 0; i < tableCount; i++)
            {
                DataTable table = billableTickets.Tables[0];
                billableTickets.Tables.Remove(table);
            }
            

        }

        private DateTime CheckDates(bool startDateRequired)
        {
            DateTime newStartDate;
            DateTime newEndDate;

            //DateTime formStartDate = (DateTime)startDatePicker.SelectedDate;
            int dateResultSillyDate = DateTime.Compare((DateTime)startDatePicker.SelectedDate, DateTime.Parse("01/01/1900 00:00:00"));
            int dateResultStartEnd = DateTime.Compare((DateTime)startDatePicker.SelectedDate, (DateTime)endDatePicker.SelectedDate);

            if(dateResultSillyDate < 0 || dateResultStartEnd > 0)
            {
                DateTime[] dateArray = SortDateOut().ToArray();
                //newStartDate = dateArray[0];
                //newEndDate = dateArray[1];
                startDatePicker.SelectedDate = dateArray[0];
                endDatePicker.SelectedDate = dateArray[1];
            } //else
            //{
            
            newStartDate = (DateTime)startDatePicker.SelectedDate;
            newEndDate = (DateTime)endDatePicker.SelectedDate;
            //}

            if (startDateRequired)
            {
                return newStartDate;
            } else
            {
                return newEndDate;
            }

        }

        private void AddMessagetoBar(string message)
        {
            lblMessageBar.Content = message;
        }

        private void CollectValidTickets()
        {
            DateTime startDate = CheckDates(true);
            DateTime endDate = CheckDates(false);

            //string rowHeading;
            //loopthrough the ticket dataset and get all tickets that fall in the date range
            foreach (DataTable unbilledTicketTable in unbilledTicketsDataSet.Tables)
            {
                bool ticketInDateRange = false;
                bool ticketNotBilled = false;

                //lets find the date of the ticket
                foreach (DataRow unbilledTicketTableRow in unbilledTicketTable.Rows)                             //loop each row in the unbilledTicketTable
                {

                    if (unbilledTicketTableRow.Field<string>("Desc") == "TicketDate")                            //Date of the ticket found
                    {
                        DateTime TicketDate = DateTime.Parse(unbilledTicketTableRow.Field<string>("Val"));      //get the date
                        int dateCompareStartResult = DateTime.Compare(TicketDate, startDate);                   //compare the date against our start date
                        int dateCompareEndResult = DateTime.Compare(TicketDate, endDate);                       //compare the date against our end date

                        if (dateCompareStartResult >= 0 && dateCompareEndResult <= 0)                            //if the date falls between our start and end date
                        {
                            ticketInDateRange = true;                                                           //it can be added
                        }
                    }
                    if (unbilledTicketTableRow.Field<string>("Desc") == "TicketBilled")                        //has the ticket already been billed
                    {
                        if (unbilledTicketTableRow.Field<string>("Val") == "NO")
                        {
                            ticketNotBilled = true;                                                             //ticket has not been billed
                        }

                    }

                }

                if (ticketInDateRange && ticketNotBilled)                                                        //if the ticket is billable
                {
                    billableTickets.Tables.Add(unbilledTicketTable.Copy());                                     //add it to the billable tickets table
                }
            }
        }

        private void AddNewColumnsToBillableTickets()
        {
            foreach (DataTable billableTicketTable in billableTickets.Tables)                           //loop through each table in the billableTickets Dataset
            {
                DataColumnCollection columnNames = billableTicketTable.Columns;                         //check to see if the columns already exist

                if (!columnNames.Contains("ItemPrice"))
                {
                    billableTicketTable.Columns.Add("ItemPrice", typeof(double));
                }
                if (!columnNames.Contains("ItemTotalPrice"))
                {
                    billableTicketTable.Columns.Add("ItemTotalPrice", typeof(double));
                }
            }
        }

        private void PopulateBilledTicketsDataGrid()
        {

            bool totalTicketPriceExists = false;
            int totalTicketPriceRowIndex = 0;

            //variables to controll columns
            //int singleDeliveryPriceColumn = 5;
            int indexColumn = 9;
            int ticketFileColumn = 10;
            int totalColumns = ticketFileColumn;

            //float totalTicketPrice = 0.00F;

            //clear out the global datasets
            ClearDataSets();                        //clear out all old data
            CollectValidTickets();                  //load all tickets that fit correct range into billableTickets dataset
            AddNewColumnsToBillableTickets();       //adds the ItemPrice and ItemTotalPrice Columns

            

            //all the tickets we are interested in are in billableTickets
            //loop each ticket, get the CN number and pull the relavant pricing data from the customerDetails table
           

            //create the finnished tickets dataset

            
            //current price list is stored in PriceListUsed - loaded when we clicked on the price list listbox
            //foreach(DataTable ticketTable in unbilledTicketsDataSet.Tables)
            foreach (DataTable billableTicketTable in billableTickets.Tables)                           //loop through each table in the billableTickets Dataset
                {

                DataRow discountPercRow = billableTicketTable.NewRow();
                DataRow discountStartAmountRow = billableTicketTable.NewRow();
                //DataRow deliveryAmountRow = billableTicketTable.NewRow();
                DataRow customerName = billableTicketTable.NewRow();

                DataRow newBillableTicketTableRow = billableTicketTable.NewRow();                          
                double custDeliveryChargePerRun = 0.00f;
                double custDiscountStart = 0.00f;
                double custDiscountPercent = 0.00f;
                //double custDiscountTotal = 0.00f;
                //double calcFinalTicketPrice = 0.00f;
                double totalItemsPrice = 0.00f;
                //double totalTicketPrice = 0.00f;
                int delRowIndex = -1;
                int delRowCounter = 0;
                string customerFirstName = "";
                string customerLastName = "";
                

                foreach (DataRow billableTicketTableRow in billableTicketTable.Rows)                                       //loop each row in the billableTickets data table
                {                   
                    //find the CN number 
                    if(billableTicketTableRow.Field<string>("Desc") == "CN")                                               //find the customer number
                    {
                        string customerNumber = "CN" + billableTicketTableRow.Field<string>("Val").ToString();             //make the customer number
                        //look up the customer in the customer table
                        //Add required data to the processing ticket
                        foreach(DataRow custDetailsRow in customerDetails.Tables[customerNumber].Rows)  //loop through the customer data for the current customer
                        { 
                            //we want to find the customers:
                            //Discount %
                            //Discount Start Amount
                            //Delivery Charge
                            
                            switch (custDetailsRow.Field<string>("Property")){                          //scan the customer 
     
                                case "DiscountPercent":
                                    
                                    discountPercRow["Desc"] = "Discount %";
                                    discountPercRow["Val"] = custDetailsRow.Field<string>("Value");

                                    if(!double.TryParse(custDetailsRow.Field<string>("Value"), out custDiscountPercent))
                                    {
                                        custDiscountPercent = 0.00f;
                                        AddMessagetoBar("PopulateBilledTicketsDataGrid - Row 312");
                                    }
                                    
                                    break;

                                case "DiscountStartAmount":
                                    
                                    discountStartAmountRow["Desc"] = custDetailsRow.Field<string>("Property");
                                    discountStartAmountRow["Val"] = custDetailsRow.Field<string>("Value");

                                    if(!double.TryParse(custDetailsRow.Field<string>("Value"), out custDiscountStart))
                                    {
                                        custDiscountStart = 0.00f;
                                        AddMessagetoBar("PopulateBilledTicketsDataGrid - Row 324");
                                    }
                                    
                                    break;

                                case "DeliveryCharge":                                  
                                    
                                    //deliveryAmountRow["Desc"] = "Delivery";
                                    //deliveryAmountRow["Val"] = custDetailsRow.Field<string>("Value");
                                    if (!double.TryParse(custDetailsRow.Field<string>("Value"), out custDeliveryChargePerRun))
                                    {
                                        if(custDetailsRow.Field<string>("Value") != "")
                                        {
                                            AddMessagetoBar("PopulateBilledTicketsDataGrid - Row 334 - Delivery charge issue");
                                        }

                                    //    custDeliveryCharge = 0.00f;
                                        
                                    }
                                    break;

                                case "FirstName":
                                    customerFirstName = custDetailsRow.Field<string>("Value").ToString();

                                    break;

                                case "LastName":
                                    customerLastName = custDetailsRow.Field<string>("Value").ToString();

                                    break;
                            }

                            
                        }

                    }

                    if(billableTicketTableRow.Field<string>("Desc") == "DeliveryCount") //get the number of deliveries
                    {
                        delRowIndex = delRowCounter; //store the location of the delivery information
                    }
                    
                    //find the item
                    if(billableTicketTableRow.Field<string>("Desc") == "Item")
                    {
                        foreach(DataRow priceListRow in priceListUsed.Rows) //loop through the price list table
                        {
                            if(priceListRow.Field<string>("Desc") == billableTicketTableRow.Field<string>("Val")) //if the item in the ticket table = the price list table item
                            {
                                //float fItemPrice = float.Parse(priceListRow.Field<string>("Price"));
                                if(!double.TryParse(priceListRow.Field<string>("Price"), out double pricelistrowval)){
                                    billableTicketTableRow["ItemPrice"] = 0.00f;
                                    AddMessagetoBar(priceListRow[1].ToString() + " has price of : " + priceListRow[2].ToString());
                                } else
                                {
                                    billableTicketTableRow["ItemPrice"] = pricelistrowval;
                                }
                                //row["ItemPrice"] = double.Parse(priceListRow.Field<string>("Price"));
                                double itemPrice = billableTicketTableRow.Field<double>("ItemPrice");
                                int itemQty = int.Parse(billableTicketTableRow.Field<string>("Count"));

                                //double totalPriceone = Math.Round(6.342, 2, MidpointRounding.ToPositiveInfinity);

                                double totalPrice = Math.Round((itemPrice * itemQty),2,MidpointRounding.AwayFromZero);
                                billableTicketTableRow["ItemTotalPrice"] = totalPrice;
                                totalItemsPrice = (double)(totalItemsPrice + totalPrice);
                            }

                        }
                    }

                    //check to see if we have a totalticketprice row
                    if (billableTicketTableRow.Field<string>("Desc") == "TotalItemsPrice")
                    {
                        totalTicketPriceExists = true;
                        totalTicketPriceRowIndex = billableTicketTable.Rows.IndexOf(billableTicketTableRow);
                    }

                    delRowCounter++;

                }
                //resolve the customer name
                customerName["desc"] = "CustomerName";
                customerName["Val"] = customerFirstName + " " + customerLastName;
                customerFirstName = "";
                customerLastName = "";
                billableTicketTable.Rows.Add(customerName);

                //add the new data rows
                billableTicketTable.Rows.Add(discountPercRow);
                billableTicketTable.Rows.Add(discountStartAmountRow);
                //billableTicketTable.Rows.Add(deliveryAmountRow);

                //finished looping each row of the ticket table

                //add the new data to the billable tickets tables             

                //check if the total ticket price row exists
                if (!totalTicketPriceExists)
                {
                    //add the total price to the ticket
                    DataRow totalItemsPriceRow = billableTicketTable.NewRow();
                    totalItemsPriceRow["Desc"] = "TotalItemsPrice";
                    totalItemsPriceRow["ItemTotalPrice"] = Math.Round(totalItemsPrice, 2, MidpointRounding.AwayFromZero);
                    billableTicketTable.Rows.Add(totalItemsPriceRow);
                } else
                {
                    billableTicketTable.Rows[totalTicketPriceRowIndex]["ItemTotalPrice"] = totalItemsPrice;
                }

                //sort out delivery charge
                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                double calcDeliveryCharge = 0.00f;

                //add the price per delivery to the table
                //billableTicketTable.Rows[delRowIndex][3] = custDeliveryChargePerRun;

                if (delRowIndex > -1) //was there any delivery information
                {
                    billableTicketTable.Rows[delRowIndex][3] = custDeliveryChargePerRun;
                    if(!double.TryParse(billableTicketTable.Rows[delRowIndex][1].ToString(), out double deliveryRunCharge) || !double.TryParse(billableTicketTable.Rows[delRowIndex][3].ToString(), out double deliveryRuns))
                    {
                        AddMessagetoBar("PopulateBilledTicketsDataGrid - Deliver Calc problem");
                    } else
                    {
                        calcDeliveryCharge = Math.Round((double.Parse(billableTicketTable.Rows[delRowIndex][1].ToString()) * double.Parse(billableTicketTable.Rows[delRowIndex][3].ToString())), 2, MidpointRounding.AwayFromZero);
                        billableTicketTable.Rows[delRowIndex][4] = calcDeliveryCharge;
                    }
                    
                }


                //add the final price to the main table
                double totalTicketPrice = Math.Round(totalItemsPrice + calcDeliveryCharge, 2, MidpointRounding.AwayFromZero);
                DataRow totalTicketPriceRow = billableTicketTable.NewRow();
                totalTicketPriceRow["Desc"] = "TotalTicketPrice";
                totalTicketPriceRow["ItemTotalPrice"] = totalTicketPrice;
                billableTicketTable.Rows.Add(totalTicketPriceRow);


            }


            //lets create a list for the actual grid on the display
            //loop the newly created billable tickets
            gridTableOfBillibletickets.Clear();
            gridTableOfBillibletickets.Columns.Clear();
            DataColumn TicketNumCol = new DataColumn("Ticket", typeof(int));
            DataColumn TicketDateCol = new DataColumn("Date", typeof(string));
            DataColumn CustomerNumberCol = new DataColumn("CN", typeof(int));
            DataColumn CustomerNameCol = new DataColumn("Name", typeof(string));
            DataColumn ItemTotalCol = new DataColumn("Items £", typeof(string));
            //DataColumn DiscountCol = new DataColumn("Discount Start", typeof(string));
            DataColumn DiscountCol = new DataColumn("Discount", typeof(string));
            //DataColumn DeliveryCharge = new DataColumn("Deliverys", typeof(string));
            //DataColumn SingleDeliveryCharge = new DataColumn("Del £", typeof(string));
            //DataColumn Deliveries = new DataColumn("Del Count", typeof(string));
            DataColumn DeliveryTotal = new DataColumn("Delivery", typeof(string));
            DataColumn TicketTotalPrice = new DataColumn("Ticket Total", typeof(string));
            DataColumn UnbilledIndex = new DataColumn("Unbilled Index", typeof(int));
            DataColumn TicketFile = new DataColumn("Ticket File", typeof(string));
            DataColumn BillTicket = new DataColumn("Bill ticket", typeof(bool));
            //DataColumn TicketLocationCol = new DataColumn("TicketID", typeof(string));
            int unbilledIndex = 0;

            gridTableOfBillibletickets.Columns.Add(BillTicket);
            gridTableOfBillibletickets.Columns.Add(TicketNumCol);
            gridTableOfBillibletickets.Columns.Add(TicketDateCol);
            gridTableOfBillibletickets.Columns.Add(CustomerNumberCol);
            gridTableOfBillibletickets.Columns.Add(CustomerNameCol);
            gridTableOfBillibletickets.Columns.Add(DiscountCol);
            gridTableOfBillibletickets.Columns.Add(ItemTotalCol);
            //gridTableOfBillibletickets.Columns.Add(DiscountCol);
            //gridTableOfBillibletickets.Columns.Add(SingleDeliveryCharge);
            //gridTableOfBillibletickets.Columns.Add(Deliveries);
            gridTableOfBillibletickets.Columns.Add(DeliveryTotal);
            gridTableOfBillibletickets.Columns.Add(TicketTotalPrice);
            gridTableOfBillibletickets.Columns.Add(UnbilledIndex);
            gridTableOfBillibletickets.Columns.Add(TicketFile);
            

            foreach (DataTable billableTicket in billableTickets.Tables)
            {
                DataRow newGridRow = gridTableOfBillibletickets.NewRow();

                string discountRateForDisplay = "";
                string discountPercentForDisplay = "";
                double deliveryCharge = 0.00f;

                newGridRow["Unbilled Index"] = unbilledIndex++;
                newGridRow["Ticket File"] = billableTicket.TableName;
                newGridRow["Bill Ticket"] = true;

                foreach (DataRow row in billableTicket.Rows)
                {
                    
                    switch (row[0].ToString())
                    {
                     
                        case "TicketNumber":
                            newGridRow["Ticket"] = int.Parse(row[1].ToString());
                            break;

                        case "TicketDate":
                            DateTime ticketDate = DateTime.Parse((string)row[1]);
                            string formattedDate = ticketDate.ToString("dd/M/yyyy");
                            newGridRow["Date"] = formattedDate;
                            break;

                        case "CN":
                            bool cnParseSuccess = int.TryParse(row[1].ToString(), out int cnASint);
                            newGridRow["CN"] = cnASint;
                            break;

                        case "CustomerName":
                            newGridRow["Name"] = row[1].ToString();
                            break;

                        case "TotalItemsPrice":
                            newGridRow["Items £"] = String.Format("{0:C}", row[4]);
                            break;

                        case "DeliveryCount":
                            if(row[1].ToString() != "" & row[1].ToString() != "0")
                            {
                                newGridRow["Delivery"] = String.Format("{0:C}", row[4]);
                                //do nothing
                                //}else
                                // {
                                //     newGridRow["Del £"] = String.Format("{0:C}", row[3]);
                                //    newGridRow["Del Count"] = row[1];
                                //    newGridRow["Del Total"] = String.Format("{0:C}", row[4]);
                            }
                            
                            break;

                        case "Discount %":
                            discountPercentForDisplay = row[1].ToString();
                            break;

                        case "DiscountStartAmount":
                            discountRateForDisplay = row[1].ToString();
                            break;


                        case "TotalTicketPrice":
                            newGridRow["Ticket Total"] = String.Format("{0:C}", row[4]);
                            break;
                    }
                }

                newGridRow["Discount"] = discountPercentForDisplay + "%" + " | £" + discountRateForDisplay;

                gridTableOfBillibletickets.Rows.Add(newGridRow);
            }

            //display the new grid
            dataGridUnbilledTickets.ItemsSource = new DataView(gridTableOfBillibletickets);
            //dataGridUnbilledTickets.Columns[singleDeliveryPriceColumn].Visibility = Visibility.Hidden; //hide the single delivery price
            dataGridUnbilledTickets.Columns[indexColumn].Visibility = Visibility.Hidden; //hide the index
            dataGridUnbilledTickets.Columns[0].IsReadOnly = false;
            for(int col = 1; col < totalColumns; col++)
            {
                dataGridUnbilledTickets.Columns[col].IsReadOnly = true;
            }
            dataGridUnbilledTickets.Columns[ticketFileColumn].Visibility = Visibility.Hidden; //hide the ticket file
            //dataGridJSShortCodes.ItemsSource = new DataView(shortCodesData);

        }

        private void LoadPriceLists()
        {
            DataTable PriceListsTable = new();
            PriceListsTable.Columns.AddRange(new DataColumn[3] { new DataColumn("Name"), new DataColumn("Date"), new DataColumn("Path") });

            //check if the pricelists fold exists
            if (!Directory.Exists(glob_PriceListsFolder))
            {
                Directory.CreateDirectory(glob_PriceListsFolder);
            }

            string[] priceLists = Directory.GetFiles(glob_PriceListsFolder, "*", SearchOption.TopDirectoryOnly);
            XmlDocument xmlDocument = new();
            
            foreach (string priceList in priceLists)
            {
                DataRow row = PriceListsTable.NewRow();
                row["Path"] = priceList;

                xmlDocument.Load(priceList);
                XmlNode nameNode = xmlDocument.DocumentElement.SelectSingleNode("Name");
                XmlNode dateNode = xmlDocument.DocumentElement.SelectSingleNode("Date");

                row["Name"] = nameNode.InnerText;
                row["Date"] = dateNode.InnerText;

                PriceListsTable.Rows.Add(row);

                xmlDocument.RemoveAll();
            }

            DataView priceView = PriceListsTable.DefaultView;
            priceView.Sort = "Date DESC";
            glob_PriceListsTable = priceView.ToTable();

            foreach(DataRow row in glob_PriceListsTable.Rows)
            {
                listBoxPriceLists.Items.Add(row["Name"] + "   -   " + row["Date"]);
            }

            if (listBoxPriceLists.Items.Count > 0)
            {
                listBoxPriceLists.SelectedIndex = 0;
            }
        }

        private void LoadTicketsIntoDataSet()
        {
            //scan through the tickets loading everything unbilled
            string[] ticketFiles = Directory.GetFiles(glob_ticketsFilePath, "*.xml", SearchOption.TopDirectoryOnly);
            
            //DataSet unbilledTicketsDataSet = new();
            XmlDocument ticketDocument = new();
            foreach (string ticketFile in ticketFiles)
            {
                ticketDocument.Load(ticketFile);
                unbilledTicketsDataSet.Tables.Add(new DataTable(ticketFile));
                unbilledTicketsDataSet.Tables[ticketFile].Columns.Add("Desc");
                //unbilledTicketsDataSet.Tables[ticketFile].Columns.Add("Code");
                unbilledTicketsDataSet.Tables[ticketFile].Columns.Add("Val");
                unbilledTicketsDataSet.Tables[ticketFile].Columns.Add("Count");


                XmlElement root = ticketDocument.DocumentElement;
                XmlNodeList nodes = root.SelectNodes("Ticket");

                foreach (XmlNode node in nodes) //loop the ticket node
                {
                    foreach (XmlNode childNode in node.ChildNodes) //loop each child element of the ticket
                    {
                        if(childNode.Name != "Items")
                        {
                            DataRow dataRow = unbilledTicketsDataSet.Tables[ticketFile].NewRow();
                            dataRow["Desc"] = childNode.Name;
                            dataRow["Val"] = childNode.InnerText;
                            dataRow["Count"] = "0";
                            unbilledTicketsDataSet.Tables[ticketFile].Rows.Add(dataRow);
                        }
                        else
                        {
                            foreach (XmlNode itemNode in childNode.ChildNodes)
                            {
                                DataRow itemRow = unbilledTicketsDataSet.Tables[ticketFile].NewRow();

                                itemRow["Desc"] = "Item";

                                foreach (XmlNode itemChildNode in itemNode.ChildNodes)
                                {
                                    if(itemChildNode.Name == "ItemType")
                                    {
                                        itemRow["Val"] = itemChildNode.InnerText;
                                    }
                                    else if(itemChildNode.Name == "ItemQty")
                                    {
                                        itemRow["Count"] = itemChildNode.InnerText;
                                    }

                                }

                                unbilledTicketsDataSet.Tables[ticketFile].Rows.Add(itemRow);

                            }
                            //we need to loop through the items
                        }
                    }
                }
            }
            
        }

        private List<DateTime> SortDateOut()
        {

            List<DateTime> sortedDates = new();

            //get the date
            int billingMonth;
            DateTime today = DateTime.Today;
            if (today.Day > 7)
            {
                billingMonth = today.Month;
            }
            else
            {
                billingMonth = today.Month - 1;
            }

            int billingYear = today.Year;
            if (billingMonth == 0)
            {
                billingMonth = 12;
                billingYear = billingYear - 1;
            }

            DateTime startDate = new DateTime(billingYear, billingMonth, 1, 0, 0, 0);
            int daysInBillingMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
            DateTime endDate = new DateTime(billingYear, billingMonth, daysInBillingMonth, 23, 59, 59);

            startDatePicker.SelectedDate = startDate;
            endDatePicker.SelectedDate = endDate;

            sortedDates.Add(startDate);
            sortedDates.Add(endDate);

            return sortedDates;
        }

        private void buttonCreateNewPriceList_Click(object sender, RoutedEventArgs e)
        {
            //put code in
            NewPriceList newpricelist = new NewPriceList();
            this.Close();
            newpricelist.ShowDialog();
            
          
        }

        private void startDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            btnProcessBilling.IsEnabled = false;
            if (!firstRun)
            {
                //PopulateBilledTicketsDataGrid();
            }
            
        }

        private void endDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            btnProcessBilling.IsEnabled = false;
            if (!firstRun)
            {
                //PopulateBilledTicketsDataGrid();
            }
        }

        private void btnRefreshDates_Click(object sender, RoutedEventArgs e)
        {
            PopulateBilledTicketsDataGrid();
            btnProcessBilling.IsEnabled = true;
        }

        private void listBoxPriceLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StorePriceListDataInTable();
            btnProcessBilling.IsEnabled = false;
        }

        private void StorePriceListDataInTable()
        {
            //on the side, build the data columns for the current price list
            if(priceListUsed.Columns.Count == 0)
            {
                priceListUsed.Columns.Add("Code");
                priceListUsed.Columns.Add("Desc");
                priceListUsed.Columns.Add("Price");
            } else
            {
                priceListUsed.Rows.Clear();            
            }
            

            //get the price list selected
            int selectedPriceList = listBoxPriceLists.SelectedIndex;

            int rowCount = 0;

            //get the path of the pricelist to be used
            string priceListFilePath = (string)glob_PriceListsTable.Rows[selectedPriceList][2];

            //generate a table from the data in the file
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(priceListFilePath);

            XmlElement root = xmlDocument.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("Item");

            foreach(XmlNode node in nodes)
            {

                DataRow newRow = priceListUsed.NewRow();

                foreach (XmlNode childNode in node)
                {
                    
                    switch(childNode.Name){
                        case "Code":
                            newRow[0] = childNode.InnerText;
                            break;

                        case "Desc":
                            newRow[1] = childNode.InnerText;
                            break;

                        case "Price":
                            newRow[2] = childNode.InnerText;
                            break;
                    }
                }

                priceListUsed.Rows.Add(newRow);
                rowCount++;
            }
          
        }

        private DataTable AmendCustomerTableName(DataTable ticketTable, string customerNumber)
        {
            DataTable dt = new();
            dt.TableName = customerNumber;
            DataColumn ccdsCol1 = new DataColumn("Desc", typeof(string));
            DataColumn ccdsCol2 = new DataColumn("qty", typeof(double));

            dt.Columns.Add(ccdsCol1);
            dt.Columns.Add(ccdsCol2);

            foreach (DataRow row in ticketTable.Rows)
            {
                if (row[0].ToString() == "Discount %")
                {
                    DataRow newRow = dt.NewRow();
                    newRow[0] = "Discount %";
                    newRow[1] = double.Parse(row[1].ToString());
                    dt.Rows.Add(newRow);
                }
                if (row[0].ToString() == "DiscountStartAmount")
                {
                    DataRow newRow = dt.NewRow();
                    newRow[0] = "DiscountStartAmount";
                    newRow[1] = double.Parse(row[1].ToString());
                    dt.Rows.Add(newRow);
                }
                if (row[0].ToString() == "TotalItemsPrice")
                {
                    DataRow newRow = dt.NewRow();
                    newRow[0] = "TotalItemsPrice";
                    newRow[1] = double.Parse(row[4].ToString());
                    dt.Rows.Add(newRow);
                    //return dt;
                }
            }
            dt.TableName = customerNumber;

            return dt;
        }

        private void btnProcessBilling_Click(object sender, RoutedEventArgs e)
        {

            DataTable UserSelectedTicketsTable = GetTicketsUserWants();
            
            
                                                 
            DataSet XeroDataset = new();
            DataSet CustomersCombinedDataSet = new();




            //DataTable XeroTable = CreateXeroTemplate();
            //we want to work out the discount for each customer
            //so lets go through the tickets, grabbing the items total price and adding it to the customer as a new table
            //foreach (DataTable ticketTable in )

            foreach (DataRow ticketToBillRow in UserSelectedTicketsTable.Rows)
            {
                string ticketToProcess = ticketToBillRow[0].ToString();
                foreach (DataTable ticketTable in billableTickets.Tables)
                {
                    int customerNumber = 0;
                    string customerNumberString = "";

                    if (ticketTable.TableName == ticketToProcess)
                    {
                        //find the customer number of the ticket
                        foreach(DataRow ticketRow in ticketTable.Rows)
                        {
                            if(ticketRow[0].ToString() == "CN")
                            {
                                customerNumber = int.Parse(ticketRow[1].ToString());
                                customerNumberString = "CN" + customerNumber.ToString();
                                break;
                            }
                        }

                        bool customerFound = false;

                        if (CustomersCombinedDataSet.Tables.Count > 0)//make sure we have some table
                        {
                            
                            foreach(DataTable customer in CustomersCombinedDataSet.Tables)
                            {
                                //get the name of the table
                                if(customer.TableName == customerNumberString)
                                {

                                    //add the total items price
                                    //customer.Merge(ticketTable);
                                    foreach(DataRow dataRow in ticketTable.Rows)
                                    {
                                        if(dataRow[0].ToString() == "TotalItemsPrice")
                                        {
                                            DataRow newRow = customer.NewRow();
                                            newRow[0] = "TotalItemsPrice";
                                            newRow[1] = double.Parse(dataRow[4].ToString());
                                            customer.Rows.Add(newRow);
                                            break;
                                        }

                                    }
                                    customerFound = true;
                                }
                            }

                        } else
                        {
                            //create the first table
                            CustomersCombinedDataSet.Tables.Add(AmendCustomerTableName(ticketTable, customerNumberString).Copy());
                            customerFound = true;
                        }

                        if (!customerFound)
                        {
                            CustomersCombinedDataSet.Tables.Add(AmendCustomerTableName(ticketTable, customerNumberString).Copy());
                        }

                    }
                }
            }

            //create the discout ticket for each customer
            foreach (DataTable discountTable in CustomersCombinedDataSet.Tables)
            {
                double itemTotalPrice = 0.00f;
                double discountPerc = 0.00f;
                double discountStart = 0.00f;

                //loop each row to get the total
                foreach(DataRow itemPriceRow in discountTable.Rows)
                {
                    if(itemPriceRow[0].ToString() == "Discount %")
                    {
                        discountPerc = double.Parse(itemPriceRow[1].ToString());
                    }
                    if(itemPriceRow[0].ToString() == "DiscountStartAmount")
                    {
                        discountStart = double.Parse(itemPriceRow[1].ToString());
                    }
                    if (itemPriceRow[0].ToString() == "TotalItemsPrice")
                    {
                        itemTotalPrice = itemTotalPrice + Math.Round(double.Parse(itemPriceRow[1].ToString()),2,MidpointRounding.AwayFromZero);
                    }
                        
                }

                if (itemTotalPrice > discountStart)
                {

                    double finalDiscount = Math.Round((itemTotalPrice - discountStart) * discountPerc / 100,2,MidpointRounding.AwayFromZero) * -1;

                    //finished looping the rows so create a ticket
                    DataTable newDiscountTicket = new();
                    DataColumn col1 = new DataColumn("Desc", typeof(string));
                    DataColumn col2 = new DataColumn("Val", typeof(string));
                    DataColumn col3 = new DataColumn("Count", typeof(string));
                    DataColumn col4 = new DataColumn("ItemPrice", typeof(double));
                    DataColumn col5 = new DataColumn("ItemTotalPrice", typeof(double));

                    newDiscountTicket.Columns.Add(col1);
                    newDiscountTicket.Columns.Add(col2);
                    newDiscountTicket.Columns.Add(col3);
                    newDiscountTicket.Columns.Add(col4);
                    newDiscountTicket.Columns.Add(col5);

                    DataRow CNRow = newDiscountTicket.NewRow();
                    string custNum = discountTable.TableName;
                    newDiscountTicket.TableName = custNum;
                    CNRow[0] = "CN";
                    CNRow[1] = custNum.Substring(2, custNum.Length - 2);
                    newDiscountTicket.Rows.Add(CNRow);

                    DataRow itemRow = newDiscountTicket.NewRow();
                    itemRow[0] = "Item";
                    itemRow[1] = "Discount";
                    itemRow[2] = "1";
                    itemRow[3] = finalDiscount;
                    itemRow[4] = finalDiscount;
                    newDiscountTicket.Rows.Add(itemRow);

                    //add the table to the unbilled tickets
                    billableTickets.Tables.Add(newDiscountTicket);

                    //add the name of the ticket to the user selected list
                    DataRow newUserTicket = UserSelectedTicketsTable.NewRow();
                    newUserTicket[0] = discountTable.TableName;
                    UserSelectedTicketsTable.Rows.Add(newUserTicket);
                }
            }

            //loop through all the tickets processing the ones in the above table
            //bool issueFound = false;

            foreach (DataRow ticketToBillRow in UserSelectedTicketsTable.Rows)
            {
                string ticketToProcess = ticketToBillRow[0].ToString();

                foreach(DataTable ticketTable in billableTickets.Tables)
                {
                    if(ticketTable.TableName == ticketToProcess)
                    {
                        DataTable tempTable = XeroCSVTable(ticketTable).Copy();
                        
                        if(tempTable.Rows[0][0].ToString() != "Issue")
                        {
                            XeroDataset.Tables.Add(tempTable); //pass the current tickt to be processed

                            //move the ticket file to the billed tickets folder
                            MoveTickettoBilledFolder(ticketToProcess);
                        } 


                    }
                }

            }



            //Merge each customer into its own table first

            //create one table with all the data in it
            DataTable XeroFinalTable = CreateXeroTemplate();

            foreach(DataTable dsTable in XeroDataset.Tables)
            {
                XeroFinalTable.Merge(dsTable);
            }

            //create the csv file for xero
            if(XeroFinalTable.Rows.Count > 0)
            {
                CreateCSVFile(XeroFinalTable, 1); //create the import file
            }
            if(glob_TicketIssues.Rows.Count > 0)
            {
                CreateCSVFile(glob_TicketIssues, 2); //create the report file
            }


            //close the window

            //show creation report
            XeroImportIssue xeroImportIssue = new(glob_TicketIssues);
            xeroImportIssue.Show();

            this.Close();

        }

        private void CreateCSVFile(DataTable XeroTable, int csvFileToCreate) //1 = xero import file, 2 = xero report file
        {
            string saveFilePath = glob_ticketsFilePath + "\\Xero";
            string selectedFileName;

            //check if the folder exists
            if (!Directory.Exists(saveFilePath))
            {
                //create the directory
                Directory.CreateDirectory(saveFilePath);
            }

            if(csvFileToCreate == 1)
            {
                selectedFileName = "\\Xero_";
            } else
            {
                selectedFileName = "\\Report_";
            }

            //saveFilePath = saveFilePath + "\\Xero_" + DateTime.Now.ToString("dd-MM-yy_HH-mm-ss") + ".csv";
            saveFilePath = saveFilePath + selectedFileName + DateTime.Now.ToString("dd-MM-yy_HH-mm-ss") + ".csv";


            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = XeroTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in XeroTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(saveFilePath, sb.ToString());
        }

        private void MoveTickettoBilledFolder(string filePath)
        {
            //check if the folder exists
            string billedTickedsPath = glob_ticketsFilePath + "\\Billed";
            if (!Directory.Exists(billedTickedsPath))
            {
                //create the directory
                Directory.CreateDirectory(billedTickedsPath);
            }


            //extract the filename
            int startIndex = filePath.IndexOf("TK-");

            if (startIndex < 0)
            {
                return;
            } else
            {
                int endIndex = filePath.IndexOf(".xml") + 4;
                string filename = filePath.Substring(startIndex, endIndex - startIndex);

                File.Move(filePath, billedTickedsPath + "\\" + filename);
            }
            


        }

        DataTable XeroCSVTable(DataTable currentTicketTable)
        { 
            DataTable dt = CreateXeroTemplate(); //create a new table with the xero remplate
                                                 //
            string customerContactFirstName = "";
            string customerContactSecondName = "";
            string customerEMail = "";
            string customerAddress1 = "";
            string customerAddress2 = "";
            string customerAddress3 = "";
            string customerAddressTown = "";
            string customerAddressCounty = "";
            string customerPostCode = "";
            string customerCN = "0";
            string ticketNumber = "";


            foreach (DataRow ticketTableRow in currentTicketTable.Rows) //find the customer name
            {
                //get the ticket number
                if(ticketTableRow[0].ToString() == "TicketNumber")
                {
                    ticketNumber = "[Ticket:" + int.Parse(ticketTableRow[1].ToString()).ToString() + "] ";
                }

                if(ticketTableRow[0].ToString() == "CN")
                {
                    customerCN = ticketTableRow[1].ToString(); //get the CN numer

                    foreach(DataRow customerDetailsRow in customerDetails.Tables["CN" + customerCN].Rows) //loop through each customer details tbale
                    {
                        switch (customerDetailsRow[0].ToString())
                        {
                            case "FirstName":
                                customerContactFirstName = customerDetailsRow[1].ToString();
                                break;

                            case "LastName":
                                customerContactSecondName = customerDetailsRow[1].ToString();
                                break;

                            case "EmailPrimary":
                                customerEMail = customerDetailsRow[1].ToString();
                                break;

                            case "AddressLn1":
                                customerAddress1 = customerDetailsRow[1].ToString();
                                break;

                            case "AddressLn2":
                                customerAddress2 = customerDetailsRow[1].ToString();
                                break;

                            case "AddressLn3":
                                customerAddress3 = customerDetailsRow[1].ToString();
                                break;

                            case "AddressTown":
                                customerAddressTown = customerDetailsRow[1].ToString();
                                break;

                            case "AddressCounty":
                                customerAddressCounty = customerDetailsRow[1].ToString();
                                break;

                            case "AddressPostCode":
                                customerPostCode = customerDetailsRow[1].ToString();
                                break;
                        }
                        //break;
                    }
                    break; //customer found so stop scanning
                }
            }

            //generate the invoice number
            string invoiceNumber = "CN" + customerCN + "-" + DateTime.Now.ToString("MM") + "-" + DateTime.Now.ToString("yyyy");
            string lastMonth = DateTime.Now.AddMonths(-1).ToString("MMM");
            string lastYear;
            if(lastMonth == "Dec")
            {
                lastYear = DateTime.Now.AddYears(-1).ToString("yy");
            } else
            {
                lastYear = DateTime.Now.ToString("yy");
            }
            string reference = lastMonth + "-" + lastYear;
            string invoiceDate = DateTime.Now.ToString("dd/MM/yyyy");
            string invoiceDueDate = DateTime.Now.AddDays(7).ToString("dd/MM/yyyy");

            bool ticketIssue = false;

            foreach(DataRow dr in currentTicketTable.Rows) //loop through each row in the ticket table
            {
                //DataRow newRow = dt.NewRow();
                //ticketIssue = false;

                switch (dr[0].ToString())
                {

                    case "DeliveryCount":
                        if (int.Parse(dr[1].ToString()) > 0)
                        {                        
                            DataRow newDeliveryRow = dt.NewRow(); //create new data table row
                            newDeliveryRow[0] = customerContactFirstName + " " + customerContactSecondName; //add the customer name
                            newDeliveryRow[1] = customerEMail; //add the customer email
                            newDeliveryRow[2] = customerAddress1; //add customer address ..
                            newDeliveryRow[3] = customerAddress2;
                            newDeliveryRow[4] = customerAddress3;
                            newDeliveryRow[6] = customerAddressTown;
                            newDeliveryRow[7] = customerAddressCounty;
                            newDeliveryRow[8] = customerPostCode;
                            newDeliveryRow[10] = invoiceNumber; //get the invoice numer
                            newDeliveryRow[11] = reference; //get the reference
                            newDeliveryRow[12] = invoiceDate; //invoice date
                            newDeliveryRow[13] = invoiceDueDate; //invoice due date
                            newDeliveryRow[16] = ticketNumber + "Delivery"; //add the ticket number and delivery
                            newDeliveryRow[17] = dr[1].ToString(); //amount of deliverys
                            newDeliveryRow[18] = dr[3].ToString(); //price of the delivery run (per item)
                            if((int.Parse(newDeliveryRow[17].ToString()) > 0) && (double.Parse(newDeliveryRow[18].ToString()) <= 0.00d)) //we have delivery but no price
                            {                              
                                ticketIssue = AssignTicketIssueToTable(ticketNumber.ToString(), CONST_noDelPrice, 3);
                            }
                                                           
                            newDeliveryRow[20] = "200-032"; //delivery xero code
                            newDeliveryRow[21] = "No VAT"; //no vat included
                            dt.Rows.Add(newDeliveryRow);//add the row to the table
                        break;
                        } else
                        {
                            break;
                        }

                    case "Item":
                        DataRow newItemRow = dt.NewRow();
                        newItemRow[0] = customerContactFirstName + " " + customerContactSecondName;
                        newItemRow[1] = customerEMail;
                        newItemRow[2] = customerAddress1;
                        newItemRow[3] = customerAddress2;
                        newItemRow[4] = customerAddress3;
                        newItemRow[6] = customerAddressTown;
                        newItemRow[7] = customerAddressCounty;
                        newItemRow[8] = customerPostCode;
                        newItemRow[10] = invoiceNumber;
                        newItemRow[11] = reference;
                        newItemRow[12] = invoiceDate;
                        newItemRow[13] = invoiceDueDate;
                        newItemRow[16] = ticketNumber + dr[1].ToString();
                        newItemRow[17] = dr[2].ToString();
                        newItemRow[18] = dr[3].ToString();
                        if (dr[1].ToString() == "Discount")
                        {
                            newItemRow[20] = CONST_discountCode;
                        } else
                        {
                            bool codeFound = false;
                            foreach (DataRow priceListRow in priceListUsed.Rows)
                            {
                                if (priceListRow[1].ToString() == dr[1].ToString())
                                {
                                    newItemRow[20] = priceListRow[0].ToString();// the code
                                    codeFound = true;
                                    break; //exit this for each loop emmediatly
                                }
                            }
                            if (!codeFound)
                            {
                                ticketIssue = AssignTicketIssueToTable(ticketNumber.ToString(), CONST_noXeroCode, 2);
                            }
                        }
                        newItemRow[21] = CONST_vatLable;
                        dt.Rows.Add(newItemRow);//add the row to the table
                        break;
                }
            }
            
            if(dt.Rows.Count <= 0)
            {
                //we have a blank ticket
                ticketIssue = AssignTicketIssueToTable(ticketNumber.ToString(), CONST_emptyTicket, 1);
            }

            if (!ticketIssue)
            {
                //deal with the discount generation for reporting
                string ticketToCN = ticketNumber.ToString();

                if(ticketNumber == "" && dt.Rows[0]["*AccountCode"].ToString() == "200-999")
                {
                    ticketToCN = "CN" + customerCN.ToString() + " - Discount";
                }
                AssignTicketIssueToTable(ticketToCN, CONST_ok, 100);
                return dt;
            } else
            {
                dt.Clear(); //dont add this ticket to the billed tickets so clear it all out
                DataRow issueRow = dt.NewRow(); //create new row in the table to say there was a problem with this ticket
                issueRow[0] = "Issue";
                dt.Rows.Add(issueRow);
                return dt;
            }
        }


        DataTable CreateXeroTemplate()
        {
            DataTable XeroTemplate = new DataTable();
            XeroTemplate.Columns.Add("*ContactName", typeof(string));
            XeroTemplate.Columns.Add("EmailAddress", typeof(string));
            XeroTemplate.Columns.Add("POAddressLine1", typeof(string));
            XeroTemplate.Columns.Add("POAddressLine2", typeof(string));
            XeroTemplate.Columns.Add("POAddressLine3", typeof(string));
            XeroTemplate.Columns.Add("POAddressLine4", typeof(string));
            XeroTemplate.Columns.Add("POCity", typeof(string));
            XeroTemplate.Columns.Add("PORegion", typeof(string));
            XeroTemplate.Columns.Add("POPostalCode", typeof(string));
            XeroTemplate.Columns.Add("POCountry", typeof(string));
            XeroTemplate.Columns.Add("*InvoiceNumber", typeof(string));
            XeroTemplate.Columns.Add("Reference", typeof(string));
            XeroTemplate.Columns.Add("*InvoiceDate", typeof(string));
            XeroTemplate.Columns.Add("*DueDate", typeof(string));
            XeroTemplate.Columns.Add("Total", typeof(string));
            XeroTemplate.Columns.Add("InventoryItemCode", typeof(string));
            XeroTemplate.Columns.Add("*Description", typeof(string));
            XeroTemplate.Columns.Add("*Quantity", typeof(string));
            XeroTemplate.Columns.Add("*UnitAmount", typeof(string));
            XeroTemplate.Columns.Add("Discount", typeof(string));
            XeroTemplate.Columns.Add("*AccountCode", typeof(string));
            XeroTemplate.Columns.Add("*TaxType", typeof(string));
            XeroTemplate.Columns.Add("TaxAmount", typeof(string));
            XeroTemplate.Columns.Add("TrackingName1", typeof(string));
            XeroTemplate.Columns.Add("TrackingOption1", typeof(string));
            XeroTemplate.Columns.Add("TrackingName2", typeof(string));
            XeroTemplate.Columns.Add("TrackingOption2", typeof(string));
            XeroTemplate.Columns.Add("Currency", typeof(string));
            XeroTemplate.Columns.Add("BrandingTheme", typeof(string));
           

            return XeroTemplate;

        }

        

        DataTable GetTicketsUserWants()
        {

            DataTable dt = new();
            DataColumn ticketName = new DataColumn("Ticket", typeof(string));
            dt.Columns.Add(ticketName);

            DataView dataview = (DataView)dataGridUnbilledTickets.ItemsSource;

            DataTable dataGridAsTable = dataview.Table.Copy();           

            foreach(DataRow datarow in dataGridAsTable.Rows)
            {
                if(datarow[0].ToString() == "True")
                {
                    DataRow newDtRow = dt.NewRow();
                    newDtRow[0] = datarow[10].ToString();
                    dt.Rows.Add(newDtRow);
                }
            }
            return dt;            

        }

    }
}
