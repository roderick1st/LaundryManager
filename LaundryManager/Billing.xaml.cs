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
            
            /*
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

            */
        }

        private void Billing_Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
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

        private void AddMessagetoBar(string message)
        {
            lblMessageBar.Content = message;
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
                int delRowIndex = -1;
                int delRowCounter = 0;
                

                foreach (DataRow row in ticketTable.Rows) //loop each row in the table
                {

                    //find the CN number 
                    if(row.Field<string>("Desc") == "CN")
                    {
                        string customerNumber = "CN" + row.Field<string>("Val").ToString();
                        //look up the customer in the customer table
                        //Add required data to the processing ticket
                        foreach(DataRow custDetailsRow in customerDetails.Tables[customerNumber].Rows)
                        {      
                            
                            switch (custDetailsRow.Field<string>("Property")){
                                //case "DiscountPrice":
                                case "DiscountPercent":
                                    DiscountRow["Desc"] = "Discount %";
                                    DiscountRow["Val"] = custDetailsRow.Field<string>("Value");

                                    if(!double.TryParse(custDetailsRow.Field<string>("Value"), out calcDiscountPercent))
                                    {
                                        calcDiscountPercent = 0.00f;
                                        AddMessagetoBar("PopulateBilledTicketsDataGrid - Row 312");
                                    }
                                    break;

                                case "DiscountStartAmount":
                                    startRow["Desc"] = custDetailsRow.Field<string>("Property");
                                    startRow["Val"] = custDetailsRow.Field<string>("Value");

                                    if(!double.TryParse(custDetailsRow.Field<string>("Value"), out calcDiscountStart))
                                    {
                                        calcDiscountStart = 0.00f;
                                        AddMessagetoBar("PopulateBilledTicketsDataGrid - Row 324");
                                    }
                                    break;

                                case "DeliveryCharge":

                                    startRow["Desc"] = "Delivery";
                                    startRow["Val"] = custDetailsRow.Field<string>("Value");
                                    //if(!double.TryParse("0", out deliveryCharge))
                                    if (!double.TryParse(custDetailsRow.Field<string>("Value"), out deliveryCharge))
                                    {
                                        deliveryCharge = 0.00f;
                                        AddMessagetoBar("PopulateBilledTicketsDataGrid - Row 334");
                                    }
                                    break;
                            }

                            
                        }

                    }

                    if(row.Field<string>("Desc") == "DeliveryCount")
                    {
                        delRowIndex = delRowCounter; //store the location of the delivery information
                    }
                    
                    //find the item
                    if(row.Field<string>("Desc") == "Item")
                    {
                        foreach(DataRow priceListRow in priceListUsed.Rows) //loop through the price list table
                        {
                            if(priceListRow.Field<string>("Desc") == row.Field<string>("Val")) //if the item in the ticket table = the price list table item
                            {
                                //float fItemPrice = float.Parse(priceListRow.Field<string>("Price"));
                                if(!double.TryParse(priceListRow.Field<string>("Price"), out double pricelistrowval)){
                                    row["ItemPrice"] = 0.00f;
                                    AddMessagetoBar(priceListRow[1].ToString() + " has price of : " + priceListRow[2].ToString());
                                } else
                                {
                                    row["ItemPrice"] = pricelistrowval;
                                }
                                //row["ItemPrice"] = double.Parse(priceListRow.Field<string>("Price"));
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
                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                double calcDeliveryCharge = 0.00f;
                if (delRowIndex > -1) //was there any delivery information
                {
                    ticketTable.Rows[delRowIndex][3] = deliveryCharge;
                    calcDeliveryCharge = double.Parse(ticketTable.Rows[delRowIndex][1].ToString()) * deliveryCharge;
                    ticketTable.Rows[delRowIndex][4] = calcDeliveryCharge;
                } 
 
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
            gridTableOfBillibletickets.Clear();
            gridTableOfBillibletickets.Columns.Clear();
            DataColumn TicketNumCol = new DataColumn("Ticket", typeof(string));
            DataColumn TicketDateCol = new DataColumn("Date", typeof(string));
            DataColumn CustomerNumberCol = new DataColumn("CN", typeof(int));
            DataColumn ItemTotalCol = new DataColumn("Item Total", typeof(string));
            DataColumn DiscountCol = new DataColumn("Discount", typeof(string));
            DataColumn DiscountPercCol = new DataColumn("Discount %", typeof(string));
            //DataColumn DeliveryCharge = new DataColumn("Deliverys", typeof(string));
            DataColumn SingleDeliveryCharge = new DataColumn("Del £", typeof(string));
            DataColumn Deliveries = new DataColumn("Del Count", typeof(string));
            DataColumn DeliveryTotal = new DataColumn("Del Total", typeof(string));
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
            gridTableOfBillibletickets.Columns.Add(ItemTotalCol);
            gridTableOfBillibletickets.Columns.Add(DiscountPercCol);
            gridTableOfBillibletickets.Columns.Add(DiscountCol);
            gridTableOfBillibletickets.Columns.Add(SingleDeliveryCharge);
            gridTableOfBillibletickets.Columns.Add(Deliveries);
            gridTableOfBillibletickets.Columns.Add(DeliveryTotal);
            gridTableOfBillibletickets.Columns.Add(TicketTotalPrice);
            gridTableOfBillibletickets.Columns.Add(UnbilledIndex);
            gridTableOfBillibletickets.Columns.Add(TicketFile);
            

            foreach (DataTable billableTicket in billableTickets.Tables)
            {
                DataRow newGridRow = gridTableOfBillibletickets.NewRow();

                newGridRow["Unbilled Index"] = unbilledIndex++;
                newGridRow["Ticket File"] = billableTicket.TableName;
                newGridRow["Bill Ticket"] = true;

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
                            bool cnParseSuccess = int.TryParse(row[1].ToString(), out int cnASint);
                            newGridRow["CN"] = cnASint;
                            break;

                        case "TotalItemsPrice":
                            newGridRow["Item Total"] = String.Format("{0:C}", row[4]);
                            break;

                        case "DeliveryCount":
                            if(row[1].ToString() == "" || row[1].ToString() == "0")
                            {
                                //do nothing
                            }else
                            {
                                newGridRow["Del £"] = String.Format("{0:C}", row[3]);
                                newGridRow["Del Count"] = row[1];
                                newGridRow["Del Total"] = String.Format("{0:C}", row[4]);
                            }
                            
                            break;

                        case "Discount %":
                            newGridRow["Discount %"] = row[1];
                            break;

                        case "TotalDiscount":
                            if(row[4].ToString() == "" || row[4].ToString() == "0")
                            {
                                //do nothing
                            } else
                            {
                                newGridRow["Discount"] = String.Format("{0:C}", row[4]);
                            }
                            
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
            dataGridUnbilledTickets.Columns[7].Visibility = Visibility.Hidden; //hide the single delivery price
            dataGridUnbilledTickets.Columns[11].Visibility = Visibility.Hidden; //hide the index
            dataGridUnbilledTickets.Columns[0].IsReadOnly = false;
            for(int col = 1; col < 11; col++)
            {
                dataGridUnbilledTickets.Columns[col].IsReadOnly = true;
            }
            dataGridUnbilledTickets.Columns[12].Visibility = Visibility.Hidden; //hide the ticket file
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

        private void btnProcessBilling_Click(object sender, RoutedEventArgs e)
        {

            DataTable UserSelectedTicketsTable = GetTicketsUserWants();
            DataSet XeroDataset = new();

            //DataTable XeroTable = CreateXeroTemplate();



            //loop through all the tickets processing the ones in the above table
            foreach (DataRow ticketToBillRow in UserSelectedTicketsTable.Rows)
            {
                string tickToProcess = ticketToBillRow[0].ToString();

                foreach(DataTable ticketTable in billableTickets.Tables)
                {
                    if(ticketTable.TableName == tickToProcess)
                    {

                        XeroDataset.Tables.Add(XeroCSVTable(ticketTable)); //pass the current tickt to be processed 

                        //move the ticket file to the billed tickets folder
                        MoveTickettoBilledFolder(tickToProcess);
                        
                    }
                }

            }

            //create one table with all the data in it
            DataTable XeroFinalTable = CreateXeroTemplate();

            foreach(DataTable dsTable in XeroDataset.Tables)
            {
                XeroFinalTable.Merge(dsTable);
            }

            //create the csv file for xero
            if(XeroFinalTable.Rows.Count > 0)
            {
                CreateCSVFile(XeroFinalTable);
            }


            //close the window
            MessageBox.Show("Xero File Created", "File Created");
            this.Close();

        }

        private void CreateCSVFile(DataTable XeroTable)
        {
            string saveFilePath = glob_ticketsFilePath + "\\Xero";

            //check if the folder exists
            if (!Directory.Exists(saveFilePath))
            {
                //create the directory
                Directory.CreateDirectory(saveFilePath);
            }

            saveFilePath = saveFilePath + "\\Xero_" + DateTime.Now.ToString("dd-MM-yy_HH-mm-ss") + ".csv";


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
            int endIndex = filePath.IndexOf(".xml") + 4;
            string filename = filePath.Substring(startIndex,endIndex-startIndex);

            File.Move(filePath, billedTickedsPath + "\\" + filename);


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


            foreach (DataRow ticketTableRow in currentTicketTable.Rows) //find the customer name
            {
                if(ticketTableRow[0].ToString() == "CN")
                {
                    customerCN = ticketTableRow[1].ToString(); //get the CN numer

                    foreach(DataRow customerDetailsRow in customerDetails.Tables["CN" + customerCN].Rows)
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

            foreach(DataRow dr in currentTicketTable.Rows) //loop through each row in the ticket table
            {
                //DataRow newRow = dt.NewRow();

                switch (dr[0].ToString())
                {
                    case "DeliveryCount":  
                        DataRow newDeliveryRow = dt.NewRow();
                        newDeliveryRow[0] = customerContactFirstName + " " + customerContactSecondName;
                        newDeliveryRow[1] = customerEMail;
                        newDeliveryRow[2] = customerAddress1;
                        newDeliveryRow[3] = customerAddress2;
                        newDeliveryRow[4] = customerAddress3;
                        newDeliveryRow[6] = customerAddressTown;
                        newDeliveryRow[7] = customerAddressCounty;
                        newDeliveryRow[8] = customerPostCode;
                        newDeliveryRow[10] = invoiceNumber;
                        newDeliveryRow[11] = reference;
                        newDeliveryRow[12] = invoiceDate;
                        newDeliveryRow[13] = invoiceDueDate;
                        newDeliveryRow[16] = "Delivery";
                        newDeliveryRow[17] = dr[1].ToString();
                        newDeliveryRow[18] = dr[3].ToString();
                        newDeliveryRow[20] = "200-032";
                        newDeliveryRow[21] = "No VAT";
                        dt.Rows.Add(newDeliveryRow);//add the row to the table
                        //newRow = clearRow(dt);
                        break;

                    case "TotalDiscount":
                        DataRow newDiscountRow = dt.NewRow();
                        newDiscountRow[0] = customerContactFirstName + " " + customerContactSecondName;
                        newDiscountRow[1] = customerEMail;
                        newDiscountRow[2] = customerAddress1;
                        newDiscountRow[3] = customerAddress2;
                        newDiscountRow[4] = customerAddress3;
                        newDiscountRow[6] = customerAddressTown;
                        newDiscountRow[7] = customerAddressCounty;
                        newDiscountRow[8] = customerPostCode;
                        newDiscountRow[10] = invoiceNumber;
                        newDiscountRow[11] = reference;
                        newDiscountRow[12] = invoiceDate;
                        newDiscountRow[13] = invoiceDueDate;
                        newDiscountRow[16] = "Discount";
                        newDiscountRow[17] = "1";
                        newDiscountRow[18] = dr[4].ToString();
                        newDiscountRow[20] = "200-999";
                        newDiscountRow[21] = "No VAT";
                        dt.Rows.Add(newDiscountRow);//add the row to the table
                        break;

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
                        newItemRow[16] = dr[1].ToString();
                        newItemRow[17] = dr[2].ToString();
                        newItemRow[18] = dr[3].ToString();
                        foreach(DataRow priceListRow in priceListUsed.Rows)
                        {
                            if(priceListRow[1].ToString() == dr[1].ToString())
                            {
                                newItemRow[20] = priceListRow[0].ToString();
                                break; //exit this for each loop emmediatly
                            }
                        }
                        //newItemRow[20] = "200-999"; //need to add a code to the datatable
                        newItemRow[21] = "No VAT";
                        dt.Rows.Add(newItemRow);//add the row to the table
                        break;
                }
            }
            

            return dt;
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
