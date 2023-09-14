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
        DataTable glob_PriceListsTable;
        DataSet unbilledTicketsDataSet = new(); //tickets that are unbilled
        DataSet billableTickets = new(); //tickets within the date range that are unbilled
        //DataSet finnishedTickets = new(); //because we need a new table after adding items in billabletickets
        DataSet customerDetails = new();
        DataTable priceListUsed = new();
        bool firstRun = true;


        public Billing()
        {
            InitializeComponent();
            glob_ticketsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\Tickets";
        }

        private void DataGridRowHeader_Click(object sender, RoutedEventArgs e)
        {
            //get the selected row.
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while ((dep != null) && (dep is not DataGridRowHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            if (dep is DataGridRowHeader)
            {
                DataGridRowHeader rowheader = dep as DataGridRowHeader;
                while ((dep != null) && (dep is not DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                DataGridRow row = dep as DataGridRow;
                SolidColorBrush red = new SolidColorBrush(Colors.Red);
                SolidColorBrush white = new SolidColorBrush(Colors.White);
                string sRed = red.Color.ToString();
                string sWhite = white.Color.ToString();
                string rowColour = row.Background.ToString();

                if (rowColour != sRed)
                {
                    row.Background = red;
              
                } else
                {
                    row.Background = white;
                }
                

            }

            //DataGridRow newrow = 

            
        }

        private void Billing_Window_Loaded(object sender, RoutedEventArgs e)
        {
            SortDateOut();
            LoadPriceLists();
            LoadCustomerInformation();
            LoadTicketsIntoDataSet();
            PopulateBilledTicketsDataGrid();
            firstRun = false;
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

        private void PopulateBilledTicketsDataGrid()
        {
            DateTime startDate;
            DateTime endDate;

            //float totalTicketPrice = 0.00F;

            //clear out the global datasets
            ClearDataSets();
            startDate = CheckDates(true);
            endDate = CheckDates(false);
            
            //string rowHeading;
            //loopthrough the ticket dataset and get all tickets that fall in the date range
            foreach(DataTable ticketTable in unbilledTicketsDataSet.Tables)
            {
                bool ticketInDateRange = false;
                bool ticketNotBilled = false;

                //get the date of the ticket
                foreach(DataRow row in ticketTable.Rows)
                {
                   
                    if(row.Field<string>("Desc") == "TicketDate") //Date of the ticket found
                    {
                        //get the date
                        DateTime TicketDate = DateTime.Parse(row.Field<string>("Val"));
                        int dateCompareStartResult = DateTime.Compare(TicketDate, startDate);
                        int dateCompareEndResult = DateTime.Compare(TicketDate, endDate);

                        //if(TicketDate >= startDate && TicketDate <= endDate) //not right some how
                        if(dateCompareStartResult >= 0 && dateCompareEndResult <= 0)
                        {
                            ticketInDateRange = true;
                            //ticket can be included in billing
                            //billableTickets.Tables.Add(ticketTable.Copy());
                            //break; //get out of the current ticket.
                        }
                    }
                      if(row.Field<string>("Desc") == "TicketBilled")
                    {
                        if(row.Field<string>("Val") == "NO")
                        {
                            ticketNotBilled = true;
                        }
                        
                    }
                    
                }

                if(ticketInDateRange && ticketNotBilled)
                {
                    billableTickets.Tables.Add(ticketTable.Copy());

                    //we have added the ticket as unbilled and within the date range
                    //so lets check the deliverys billing amount and the discount amount
                }
            }

            //all the tickets we are interested in are in billableTickets
            //loop each ticket, get the CN number and pull the relavant pricing data from the customerDetails table
           

            //create the finnished tickets dataset

            
            //current price list is stored in PriceListUsed - loaded when we clicked on the price list listbox
            //foreach(DataTable ticketTable in unbilledTicketsDataSet.Tables)
            foreach (DataTable ticketTable in billableTickets.Tables)
                {

                //DataTable newFinnishedTable = ticketTable.Copy();
                //finnishedTicket.Tables.Add(ticketTable.Copy()); //copy the table

                //check to see if the columns already exist
                DataColumnCollection columnNames = ticketTable.Columns;

                if (!columnNames.Contains("ItemPrice"))
                {
                    ticketTable.Columns.Add("ItemPrice", typeof(double));
                }
                if (!columnNames.Contains("ItemTotalPrice"))
                {
                    ticketTable.Columns.Add("ItemTotalPrice", typeof (double));
                }

                //totalTicketPrice = 0.00F; //reset the price to 0 for the next ticket
                bool totalTicketPriceExists = false;
                int totalTicketPriceRowIndex = 0;

                DataRow DiscountRow = ticketTable.NewRow();
                DataRow startRow = ticketTable.NewRow();
                double deliveryCharge = 0.00f;
                double calcDiscountStart = 0.00f;
                double calcDiscountPercent = 0.00f;
                double calcDiscountTotal = 0.00f;
                double calcFinalTicketPrice = 0.00f;
                double totalItemsPrice = 0.00F;
                double totalTicketPrice = 0.00f;
                int delRowIndex = 0;
                int delRowCounter = 0;
                

                foreach (DataRow row in ticketTable.Rows) //loop each row in the table
                {

                    //find the CN number 
                    if(row.Field<string>("Desc") == "CN")
                    {
                        string customerNumber = "CN" + row.Field<string>("Val").ToString();
                        //look up the customer in the customer table
                        foreach(DataRow custDetailsRow in customerDetails.Tables[customerNumber].Rows)
                        {      
                            
                            switch (custDetailsRow.Field<string>("Property")){
                                case "DiscountPrice":
                                    //DiscountRow["Desc"] = custDetailsRow.Field<string>("Property");
                                    DiscountRow["Desc"] = "Discount %";
                                    DiscountRow["Val"] = custDetailsRow.Field<string>("Value");
                                    calcDiscountPercent = double.Parse(custDetailsRow.Field<string>("Value"));
                                    break;

                                case "DiscountStartAmount":
                                    startRow["Desc"] = custDetailsRow.Field<string>("Property");
                                    startRow["Val"] = custDetailsRow.Field<string>("Value");
                                    calcDiscountStart = double.Parse(custDetailsRow.Field<string>("Value"));
                                    break;

                                case "DeliveryCharge":
                                    deliveryCharge = double.Parse(custDetailsRow.Field<string>("Value"));
                                    break;
                            }

                            
                        }

                    }

                    if(row.Field<string>("Desc") == "DeliveryCount")
                    {
                        delRowIndex = delRowCounter;
                    }
                    
                    //find the item
                    if(row.Field<string>("Desc") == "Item")
                    {
                        foreach(DataRow priceListRow in priceListUsed.Rows) //loop through the price list table
                        {
                            if(priceListRow.Field<string>("Desc") == row.Field<string>("Val")) //if the item in the ticket table = the price list table item
                            {
                                //float fItemPrice = float.Parse(priceListRow.Field<string>("Price"));                    
                                row["ItemPrice"] = double.Parse(priceListRow.Field<string>("Price"));
                                double itemPrice = row.Field<double>("ItemPrice");
                                int itemQty = int.Parse(row.Field<string>("Count"));

                                //double totalPriceone = Math.Round(6.342, 2, MidpointRounding.ToPositiveInfinity);

                                double totalPrice = Math.Round((itemPrice * itemQty),2,MidpointRounding.AwayFromZero);
                                row["ItemTotalPrice"] = totalPrice;
                                totalItemsPrice = (double)(totalItemsPrice + totalPrice);
                            }

                        }
                    }

                    //check to see if we have a totalticketprice row
                    if (row.Field<string>("Desc") == "TotalItemsPrice")
                    {
                        totalTicketPriceExists = true;
                        totalTicketPriceRowIndex = ticketTable.Rows.IndexOf(row);
                    }

                    delRowCounter++;

                }

                //add the new data to the billable tickets tables             

                //check if the total ticket price row exists
                if (!totalTicketPriceExists)
                {
                    //add the total price to the ticket
                    DataRow totalItemsPriceRow = ticketTable.NewRow();
                    totalItemsPriceRow["Desc"] = "TotalItemsPrice";
                    totalItemsPriceRow["ItemTotalPrice"] = totalItemsPrice;
                    ticketTable.Rows.Add(totalItemsPriceRow);
                } else
                {
                    ticketTable.Rows[totalTicketPriceRowIndex]["ItemTotalPrice"] = totalItemsPrice;
                }

                //sort out delivery charge
                ticketTable.Rows[delRowIndex][3] = deliveryCharge;
                double calcDeliveryCharge = double.Parse(ticketTable.Rows[delRowIndex][1].ToString()) * deliveryCharge;
                ticketTable.Rows[delRowIndex][4] = calcDeliveryCharge;

                //work out the discount
                ticketTable.Rows.Add(startRow);
                ticketTable.Rows.Add(DiscountRow);

                //whats the total price?
                //totalTicketPrice

                //discount start price - calDiscountStart

                //discount percent - calDiscountPercent

                //work out the discount
                if(totalItemsPrice - calcDiscountStart > 0)
                {
                    calcDiscountTotal = Math.Round((calcDiscountPercent * (totalItemsPrice - calcDiscountStart)) / 100, 2, MidpointRounding.ToZero);                   
                }

                totalTicketPrice = Math.Round(totalItemsPrice - calcDiscountTotal + calcDeliveryCharge,2, MidpointRounding.AwayFromZero);

                //add the discount information to the main table
                DataRow totalDiscountRow = ticketTable.NewRow();
                totalDiscountRow["Desc"] = "TotalDiscount";
                totalDiscountRow["ItemTotalPrice"] = calcDiscountTotal;
                ticketTable.Rows.Add(totalDiscountRow);

                //add the final price to the main table
                DataRow totalTicketPriceRow = ticketTable.NewRow();
                totalTicketPriceRow["Desc"] = "TotalTicketPrice";
                totalTicketPriceRow["ItemTotalPrice"] = totalTicketPrice;
                ticketTable.Rows.Add(totalTicketPriceRow);


            }


            //lets create a list for the actual grid on the display
            //loop the newly created billable tickets
            DataTable gridTableOfBillibletickets = new();
            DataColumn TicketNumCol = new DataColumn("Ticket", typeof(string));
            DataColumn TicketDateCol = new DataColumn("Date", typeof(string));
            DataColumn CustomerNumberCol = new DataColumn("CN", typeof(string));
            DataColumn ItemTotalCol = new DataColumn("Item Total", typeof(string));
            DataColumn DiscountCol = new DataColumn("Discount", typeof(string));
            DataColumn DiscountPercCol = new DataColumn("Discount %", typeof(string));
            DataColumn DeliveryCharge = new DataColumn("Delivery", typeof(string));
            DataColumn TicketTotalPrice = new DataColumn("Ticket Total", typeof(string));
            //DataColumn TicketLocationCol = new DataColumn("TicketID", typeof(string));

            gridTableOfBillibletickets.Columns.Add(TicketNumCol);
            gridTableOfBillibletickets.Columns.Add(TicketDateCol);
            gridTableOfBillibletickets.Columns.Add(CustomerNumberCol);
            gridTableOfBillibletickets.Columns.Add(ItemTotalCol);
            gridTableOfBillibletickets.Columns.Add(DiscountPercCol);
            gridTableOfBillibletickets.Columns.Add(DiscountCol);
            gridTableOfBillibletickets.Columns.Add(DeliveryCharge);
            gridTableOfBillibletickets.Columns.Add(TicketTotalPrice);

            foreach (DataTable billableTicket in billableTickets.Tables)
            {
                DataRow newGridRow = gridTableOfBillibletickets.NewRow();

                foreach (DataRow row in billableTicket.Rows)
                {
                    
                    switch (row[0].ToString())
                    {
                        case "TicketNumber":
                            newGridRow["Ticket"] = row[1].ToString();
                            break;

                        case "TicketDate":
                            DateTime ticketDate = DateTime.Parse((string)row[1]);
                            string formattedDate = ticketDate.ToString("dd/M/yyyy");
                            newGridRow["Date"] = formattedDate;
                            break;

                        case "CN":
                            newGridRow["CN"] = row[1].ToString();
                            break;

                        case "TotalItemsPrice":
                            newGridRow["Item Total"] = String.Format("{0:C}", row[4]);
                            break;

                        case "DeliveryCount":
                            newGridRow["Delivery"] = String.Format("{0:C}", row[4]);
                            break;

                        case "Discount %":
                            newGridRow["Discount %"] = row[1];
                            break;

                        case "TotalDiscount":
                            newGridRow["Discount"] = String.Format("{0:C}", row[4]);
                            break;

                        case "TotalTicketPrice":
                            newGridRow["Ticket Total"] = String.Format("{0:C}", row[4]);
                            break;
                    }
                }

                gridTableOfBillibletickets.Rows.Add(newGridRow);
            }

            //display the new grid
            dataGridUnbilledTickets.ItemsSource = new DataView(gridTableOfBillibletickets);
            //dataGridJSShortCodes.ItemsSource = new DataView(shortCodesData);

        }

        private void LoadPriceLists()
        {
            DataTable PriceListsTable = new();
            PriceListsTable.Columns.AddRange(new DataColumn[3] { new DataColumn("Name"), new DataColumn("Date"), new DataColumn("Path") });
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

            if(listBoxPriceLists.Items.Count > 0)
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

        private void btnProcessBilling_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
