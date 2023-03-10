using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace LaundryManager
{
    /// <summary>
    /// Interaction logic for JobSheets.xaml
    /// </summary>
    public partial class JobSheetsForm : Window
    {

        //some global variables
        string globJS_CurrentCustomer = "";
        int glob_newTicketNumber = 0;
        string glob_ticketsFilePath = "";
        string glob_ActiveTicket = "";
        string glob_PriceFilePath = "";
        string glob_ShortCodeFilePath = "";

        //bool triggeredByCustomerChange = false;

        DataTable ticketDataTable = new(); //holds ticket data for viewing

        List<string> customerDetails = new(); //holds the list of customers - populated at start of form
        List<string> currentTicketList = new(); //holds current list of tickets to display in the list box

        DataTable ticketInformation = new(); //holds descriptions of each ticket
        DataTable shortCodesData = new(); //holds the current shortcodes data

        bool glob_HandleShorCodeCommit;



        public JobSheetsForm(string currentCustomer)
        {
            InitializeComponent();
            globJS_CurrentCustomer = currentCustomer;
            glob_ticketsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\Tickets";
            glob_PriceFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\CurrentPrices.xml";
            glob_ShortCodeFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\ShortCodes.xml";
            glob_HandleShorCodeCommit = true;
        }

        private void Job_Sheet_Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;

            //load the known customers into the list box
            PopulateCustomerComboBox();

            //select the active customer in the list box
            SelectActiveCustomer(globJS_CurrentCustomer);

            //populate the columns in the ticketinformation
            AddColumnsToTicketInformation();

            //get the file data from the ticket files
            GetTicketFileData();

            //load the tickets list box
            PopulateTicketListBox();

            //populate the short codes data grid
            PopulateShortCodesDataGrid();
        }

        private void PopulateShortCodesDataGrid()
        {

            XmlDocument xmlShortCodeDoc = new();
            XmlElement root;

            //check for the existense of the short codes file
            if (!File.Exists(glob_ShortCodeFilePath))
            {
                CreateNewXMLFile(glob_ShortCodeFilePath);
                //XmlDocument xmlShortCodeDoc = new();
                XmlDeclaration xmlDeclaration = xmlShortCodeDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlShortCodeDoc.AppendChild(xmlDeclaration);
                root = xmlShortCodeDoc.CreateElement(string.Empty, "ShortCodeRoot", string.Empty);
                xmlShortCodeDoc.AppendChild(root);
                //XmlElement codes = xmlShortCodeDoc.CreateElement(string.Empty, "Codes", string.Empty);
                //root.AppendChild(codes);
                xmlShortCodeDoc.Save(glob_ShortCodeFilePath);
                //return; //no codes to load
            }

            //open the document for reading
            DataSet shortCodeDataSet = new();
            shortCodeDataSet.ReadXml(glob_ShortCodeFilePath);

            //add the dataset to the datatable
            if(shortCodeDataSet.Tables.Count > 0)
            {
                shortCodesData = shortCodeDataSet.Tables[0];
                
            } else
            {
                shortCodesData.Columns.Clear(); //clear the data table
                shortCodesData.Columns.Add("Code");
                shortCodesData.Columns.Add("Item");
            }

            dataGridJSShortCodes.ItemsSource = new DataView(shortCodesData);
            dataGridJSShortCodes.Columns[0].Width = 50;
            dataGridJSShortCodes.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);




        }

        private void dataGridJSShortCodes_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

            //commit the edit to the datatable
            if (glob_HandleShorCodeCommit)
            {
                glob_HandleShorCodeCommit = false;
                dataGridJSShortCodes.CommitEdit();            
            }
            
        
            //var text = e.EditAction;
            
            //new item added to the data grid so lets save it to the xml file
            XmlDocument shortcodeDoc = new();
            shortcodeDoc.Load(glob_ShortCodeFilePath);

            XmlNodeList codesXmlList = shortcodeDoc.GetElementsByTagName("Codes");

            //remove the codes nodes
            while (codesXmlList.Count > 0)
            {
                XmlNode code = codesXmlList[0];
                code.ParentNode.RemoveChild(code);
            }

            string shortcodeCode;
            string shortcodeItem;

            //shortcodeDoc.RemoveAll();

            //insert the new data in the table
            for (int row = 0; row < shortCodesData.Rows.Count; row++)
            {
                //Grab the data from the row
                shortcodeCode = shortCodesData.Rows[row].Field<string>("Code");
                shortcodeItem = shortCodesData.Rows[row].Field<string>("Item");

                if((shortcodeCode != "")&&(shortcodeItem != "")) //make sure both fields contain data
                {
                    //add them as a new node in the xml file
                    XmlNode codesNode = shortcodeDoc.CreateNode(XmlNodeType.Element, "Codes", null);
                    shortcodeDoc.GetElementsByTagName("ShortCodeRoot")[0].AppendChild(codesNode);
                    XmlNode codeNode = shortcodeDoc.CreateNode(XmlNodeType.Element, "Code", null);
                    XmlText codeNode_txt = shortcodeDoc.CreateTextNode(shortcodeCode);
                    codeNode.AppendChild(codeNode_txt);
                    codesNode.AppendChild(codeNode);
                    XmlNode itemNode = shortcodeDoc.CreateNode(XmlNodeType.Element, "Item", null);
                    XmlText itemNode_txt = shortcodeDoc.CreateTextNode(shortcodeItem);
                    itemNode.AppendChild(itemNode_txt);
                    codesNode.AppendChild(itemNode);
                }

            }

            //save the data to the file
            shortcodeDoc.Save(glob_ShortCodeFilePath);

            glob_HandleShorCodeCommit = true;

        }

        private void PopulateTicketListBox()
        {

            CNHandling cnHandling = new();

            List<string> tempTicketList = new();

            int custNum = cnHandling.ExtractCN(globJS_CurrentCustomer); //get the current customer number

            //generate a list based on the active customer
            currentTicketList.Clear();//empty the list

            //loop through the datatable of tickets pulling matching tickets out
            for (int record = 0; record < ticketInformation.Rows.Count; record++)
            {
                if ((Convert.ToInt32(ticketInformation.Rows[record].Field<string>("CustomerNumber")) == custNum) || (custNum == 0))
                {
                    tempTicketList.Add(ticketInformation.Rows[record].Field<string>("TicketNumberString"));
                }
            }

            currentTicketList = tempTicketList;

            listBoxJSTickets.ItemsSource = tempTicketList;
        }

        private void AddColumnsToTicketInformation()
        {
            ticketInformation.Columns.Add("TicketNumberString");
            ticketInformation.Columns.Add("TicketNumberInt");
            ticketInformation.Columns.Add("CustomerNumber");
            ticketInformation.Columns.Add("Date");
            ticketInformation.Columns.Add("Billed");
            ticketInformation.Columns.Add("FilePath");
        }

        private void SelectNewestTicket()
        {
            if (listBoxJSTickets.Items.Count > 0)
            {
                listBoxJSTickets.SelectedIndex = 0; //select the first item in the list
                listBoxJSTickets.ScrollIntoView(0); //make sure its visible
            }
        }

        private void GetTicketFileData() //populate the ticket number list
        {
            //scan through the tickets director finding the last ticket
            glob_newTicketNumber = 0; //reset the ticket counter

            //string fileName;
            //string fileNumber;
            string filePath;



            //List<string> localTicketList = new();
            currentTicketList.Clear(); //empty the list

            string[] files = Directory.GetFiles(glob_ticketsFilePath, "TK-*", SearchOption.TopDirectoryOnly);

            //int[] ticketNumbers = new int[files.Length];

            ticketInformation.Clear(); //reset the data in the table

            for (int file = 0; file < files.Length; file++)
            {
                filePath = files[file];
                //fileName = System.IO.Path.GetFileName(filePath);
                //strip the none numbers
                //fileNumber = fileName.Substring(3, fileName.Length - 3 - 4);

                //lets populate the ticket info datatable
                SaveTicketInformation(filePath);

            }

            //sort the table based on the ticket number
            DataView newView = ticketInformation.DefaultView;
            //newView.Sort = "TicketNumberInt DESC";
            newView.Sort = "TicketNumberString DESC";
            ticketInformation = newView.ToTable();
            if(ticketInformation.Rows.Count <= 0)
            {
                glob_newTicketNumber = 0;
            } else
            {
                glob_newTicketNumber = Convert.ToInt32(ticketInformation.Rows[0].Field<string>("TicketNumberInt"));
            }
            
            glob_newTicketNumber++; //increment to next ticket number            

        }


        private void SaveTicketInformation(string filePath)
        {

            DataRow ticketRow = ticketInformation.NewRow();
            ticketRow["FilePath"] = filePath;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            XmlElement root = xmlDocument.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("Ticket");

            foreach (XmlNode node in nodes) //loop the ticket node
            {

                foreach (XmlNode childNode in node.ChildNodes) //loop each element of the ticket
                {
                    switch (childNode.Name)
                    {
                        case "TicketNumber":
                            ticketRow["TicketNumberString"] = childNode.InnerText;
                            ticketRow["TicketNumberInt"] = Convert.ToInt32(childNode.InnerText);
                            break;

                        case "CN":
                            ticketRow["CustomerNumber"] = Convert.ToInt32(childNode.InnerText);
                            break;

                        case "TicketDate":
                            ticketRow["Date"] = childNode.InnerText;
                            break;

                        case "TicketBilled":
                            ticketRow["Billed"] = childNode.InnerText;
                            break;
                    }
                }
                ticketInformation.Rows.Add(ticketRow); //add the new row of data

            }
        }


        private void SelectActiveCustomer(string customer)
        {
            //loop through the lsit searching for the customer
            for (int item = 0; item < listBoxJSCustomers.Items.Count; item++)
            {
                if (listBoxJSCustomers.Items[item].ToString() == globJS_CurrentCustomer)
                {
                    listBoxJSCustomers.SelectedIndex = item;
                    listBoxJSCustomers.ScrollIntoView(listBoxJSCustomers.Items[item]);
                    break;
                }
            }
        }


        private void buttonJSNewTicket_Click(object sender, RoutedEventArgs e)
        {
            //begin to make a new ticket

            //make the file name
            //conver number to string
            string filename = glob_newTicketNumber.ToString();
            string zeroHolder = "";
            //find the length of the file name as file name needs to be 10 chars long
            for (int addZero = 0; addZero < 10 - filename.Length; addZero++)
            {
                zeroHolder += "0";
            }

            string xmlTicketNumber = zeroHolder + filename;

            filename = glob_ticketsFilePath + "\\TK-" + zeroHolder + filename + ".xml";

            CreateNewXMLFile(filename);

            //start building the basic XML
            XmlDocument xmlDocument = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(xmlDeclaration, root);

            XmlElement ticketRootNode = xmlDocument.CreateElement(string.Empty, "TicketRootNode", string.Empty);
            xmlDocument.AppendChild(ticketRootNode);

            XmlElement ticketNode = xmlDocument.CreateElement(string.Empty, "Ticket", string.Empty);
            ticketRootNode.AppendChild(ticketNode);

            XmlElement ticketNumber = xmlDocument.CreateElement(string.Empty, "TicketNumber", string.Empty);
            XmlText ticketNumber_txt = xmlDocument.CreateTextNode(xmlTicketNumber);
            ticketNumber.AppendChild(ticketNumber_txt);
            ticketNode.AppendChild(ticketNumber);


            CNHandling cnHandling = new();
            XmlElement customerNumber = xmlDocument.CreateElement(string.Empty, "CN", string.Empty);
            XmlText customerNumber_txt = xmlDocument.CreateTextNode(cnHandling.ExtractCN(globJS_CurrentCustomer).ToString());
            customerNumber.AppendChild(customerNumber_txt);
            ticketNode.AppendChild(customerNumber);

            ProgOps progOps = new();
            XmlElement ticketDate = xmlDocument.CreateElement(string.Empty, "TicketDate", string.Empty);
            XmlText ticketDate_txt = xmlDocument.CreateTextNode(progOps.StringDateTime(1));
            ticketDate.AppendChild(ticketDate_txt);
            ticketNode.AppendChild(ticketDate);

            XmlElement ticketTime = xmlDocument.CreateElement(string.Empty, "TicketTime", string.Empty);
            XmlText ticketTime_txt = xmlDocument.CreateTextNode(progOps.StringDateTime(2));
            ticketTime.AppendChild(ticketTime_txt);
            ticketNode.AppendChild(ticketTime);


            XmlElement ticketBilled = xmlDocument.CreateElement(string.Empty, "TicketBilled", string.Empty);
            XmlText ticketBilled_txt = xmlDocument.CreateTextNode("NO");
            ticketBilled.AppendChild(ticketBilled_txt);
            ticketNode.AppendChild(ticketBilled);

            XmlElement Items = xmlDocument.CreateElement(string.Empty, "Items", string.Empty);
            ticketNode.AppendChild(Items);

            xmlDocument.Save(filename);

            GetTicketFileData();
            PopulateTicketListBox();

            SelectNewestTicket();//select first ticket number if it exists

        }

        private void listBoxJSCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            handleListBoxJSCustomers_SelectionChanged();

        }

        private void handleListBoxJSCustomers_SelectionChanged()
        {
            //triggeredByCustomerChange = true;

            //find next ticket number and repopulate the ticket list ox with the new CN details
            if (listBoxJSCustomers.SelectedItem == null)
            {
                return;
            }
            globJS_CurrentCustomer = listBoxJSCustomers.SelectedItem.ToString();
            //GetNextTicketandPopulateListBox();

            if ((listBoxJSCustomers.SelectedIndex == 0) || (listBoxJSCustomers.SelectedIndex == -1))
            {
                buttonJSNewTicket.IsEnabled = false;
            }
            else
            {
                buttonJSNewTicket.IsEnabled = true;
            }

            PopulateTicketListBox();
            SelectNewestTicket(); //select the newest ticket
        }

        private void listBoxJSTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandlelistBoxJSTickets_SelectionChanged();
        }

        private void HandlelistBoxJSTickets_SelectionChanged() {

            //get the current selected item
            if (listBoxJSTickets.Items.Count <= 0)
            {
                //no tickets so just clear the table
                PopulateTicketDataGrid(true);
                return; //dont do anything as there is nothin in the list box               
            }

            PopulateTicketDataGrid(false); //lets get this data grid loaded

        }

        private void PopulateTicketDataGrid(bool ClearTableOnly)
        {
            //reset the data table
            ticketDataTable.Clear();
            while (ticketDataTable.Columns.Count > 0)
            {
                ticketDataTable.Columns.RemoveAt(ticketDataTable.Columns.Count - 1);
            }

            //test for null inthe ListBox
            if (listBoxJSTickets.SelectedItem == null)
            {
                ClearTableOnly = true;
            }

            LogCurrentTicket(""); //no ticket to log

            if (ClearTableOnly) { return; } //if we were aske to just clear the data table

            string selectedTicket = listBoxJSTickets.SelectedItem.ToString();

            DataSet dataSet = new DataSet();

            XmlReader xmlFile = XmlReader.Create(glob_ticketsFilePath + "\\TK-" + selectedTicket + ".xml", new XmlReaderSettings());
            dataSet.ReadXml(xmlFile);
            xmlFile.Close();

            LogCurrentTicket(selectedTicket);

            string[] selectedColumns = new[] { "ItemType", "ItemQty" };
            if (dataSet.Tables.Count < 3) // we dont have any items on the ticket so dont try to import them as will error
            {
                ticketDataTable.Columns.Add(new DataColumn("ItemType"));
                ticketDataTable.Columns.Add(new DataColumn("ItemQty"));
            }
            else
            {
                ticketDataTable = new DataView(dataSet.Tables[2]).ToTable(false, selectedColumns);

            }

            ticketDataTable.Columns["ItemType"].ColumnName = "Item";
            ticketDataTable.Columns["ItemQty"].ColumnName = "Quantity";


            dataGridJSTickets.ItemsSource = new DataView(ticketDataTable);

            dataGridJSTickets.Columns[0].IsReadOnly = true;
            dataGridJSTickets.Columns[0].Width = 315;
            dataGridJSTickets.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);




        }

        private void LogCurrentTicket(string TicketNumber)
        {

            glob_ActiveTicket = TicketNumber;

            if (TicketNumber == "")
            {
                labelTickedPanelHeader.Content = "NO TICKET SELECTED";
                buttonJSSaveNewTicket.IsEnabled = false;
            } else
            {
                labelTickedPanelHeader.Content = "Ticket Number : " + TicketNumber;
                buttonJSSaveNewTicket.IsEnabled = true;
            }


        }

        private void buttonJSSaveNewTicket_Click(object sender, RoutedEventArgs e)
        {
            //grab the items from the data grid and save them to the ticket in question
            //data is held in the ticketDataTable
            //ticketDataTable;
            //clear out the old items in the xml file

            string ticketFilePath = "";
            bool qtyError = false;

     
            //find the ticket filepath
            for (int rowc = 0; rowc < ticketInformation.Rows.Count; rowc++)
            {
                if (ticketInformation.Rows[rowc].Field<string>("TicketNumberString") == glob_ActiveTicket)
                {
                    ticketFilePath = ticketInformation.Rows[rowc].Field<string>("FilePath");
                    break;
                }
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(ticketFilePath);

            XmlNodeList itemXmlList = xmlDocument.GetElementsByTagName("Item");

            //Remove the item nodes already there
            while(itemXmlList.Count > 0)
            {
                XmlNode item = itemXmlList[0];
                item.ParentNode.RemoveChild(item);
            }

            //load up the pricing for items
            XmlDocument priceDoc = new XmlDocument();
            priceDoc.Load(glob_PriceFilePath);
            XmlElement priceRoot = priceDoc.DocumentElement;
            XmlNodeList priceNodes = priceRoot.SelectNodes("Items");

            string priceDesc = "";
            string priceAmount = "";

            //add the new item nodes
            string itemDescription = "";
            int itemCount = 0;

            for(int row = 0;  row < ticketDataTable.Rows.Count; row++)
            {
                itemDescription = ticketDataTable.Rows[row].Field<string>("Item");

                //lets try to short code the description
                //TODO

                try //make sure we can convert to a number 
                {
                    itemCount = Convert.ToInt32(ticketDataTable.Rows[row].Field<string>("Quantity"));
                    if (!qtyError)
                    {
                        LogCurrentTicket(glob_ActiveTicket);
                    }
                    
                }
                catch
                {
                    qtyError = true;
                    labelTickedPanelHeader.Content = "Quantity Error - Please make quantaties a number";
                }

                //create the new item node
                if (itemDescription != null)
                {

                    if (itemDescription.Length != 0)
                    {
                        XmlNode itemNode = xmlDocument.CreateNode(XmlNodeType.Element, "Item", null);   //Create new Item Node
                        XmlNode itemType = xmlDocument.CreateNode(XmlNodeType.Element, "ItemType", null);   //Create new Item Node
                        XmlText itemType_txt = xmlDocument.CreateTextNode(itemDescription);             //create item description
                        XmlNode itemQty = xmlDocument.CreateNode(XmlNodeType.Element, "ItemQty", null);   //Create new Item Node
                        XmlText itemQty_txt = xmlDocument.CreateTextNode(itemCount.ToString());             //create item description
                        XmlNode itemPrice = xmlDocument.CreateNode(XmlNodeType.Element, "ItemPrice", null);  //Create new Item Node
                        XmlText itemPrice_txt = xmlDocument.CreateTextNode("0.00");

                        foreach (XmlNode priceNode in priceNodes)
                        {
                            foreach (XmlNode priceChildNode in priceNode.ChildNodes) //loop each item
                            {

                                foreach (XmlNode priceGrandChildNode in priceChildNode.ChildNodes)
                                {
                                    if (priceGrandChildNode.Name == "Description")
                                    {
                                        priceDesc = priceGrandChildNode.InnerText;
                                    }
                                    if (priceGrandChildNode.Name == "Price")
                                    {
                                        priceAmount = priceGrandChildNode.InnerText;
                                    }
                                }

                                if (itemDescription == priceDesc)
                                {
                                    itemPrice_txt = xmlDocument.CreateTextNode(priceAmount);            //create item price

                                }
                            }
                        }

                        //XmlText itemPrice_txt = xmlDocument.CreateTextNode("10.00");                        //create item price TO DO

                        //add the new data 
                        //itemsNode.AppendChild(itemNode);
                        xmlDocument.GetElementsByTagName("Items")[0].AppendChild(itemNode);
                        itemNode.AppendChild(itemType); itemType.AppendChild(itemType_txt);
                        itemNode.AppendChild(itemQty); itemQty.AppendChild(itemQty_txt);
                        itemNode.AppendChild(itemPrice); itemPrice.AppendChild(itemPrice_txt);
                    }
                }
            }

           //Save the new xml file
           xmlDocument.Save(ticketFilePath);

           PopulateTicketDataGrid(false);

        }



        private void PopulateCustomerComboBox()
        {
            CustomerDetailsFile customerDetailsFile = new CustomerDetailsFile();    
            customerDetails.AddRange(customerDetailsFile.GetCustomerNumbers(true)); //load the local list with the customer details
            customerDetails.Insert(0, "CN0 - ALL CUSTOMERS");                                       //add fictional ALL customer entre
            listBoxJSCustomers.ItemsSource = customerDetails;
        }

        private void CreateNewXMLFile(string path)
        {
            try
            {
                using (FileStream fs = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                    glob_newTicketNumber++; //increment to the next ticket number
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonJSSaveShortCodes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dataGridJSShortCodes_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Row double clicked
            //get the row that has been clicked on *** VERY MUCH BORROWED CODE ***
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            while ((dep != null) && (dep is not DataGridCell) && (dep is not DataGridColumnHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            if (dep is DataGridColumnHeader)
            {
                DataGridColumnHeader columnHeader = dep as DataGridColumnHeader;
                // do something
            }

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;

                // navigate further up the tree
                while ((dep != null) && (dep is not DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                DataGridRow row = dep as DataGridRow;

                AddClickedItemToTicket(FindRowIndex(row));
            }
        
        
        
        }

        private void AddClickedItemToTicket(int row)
        {
            if (glob_ActiveTicket == "")
            {
                return; //dont try to add to null ticket
            }
            //we know the row clicked so lets use the underlying datatable for the information
            int rowcount = shortCodesData.Rows.Count;
            if(row >= rowcount)
            {
                return; //blank row clicked
            }

            string shortcodeItem = shortCodesData.Rows[row].Field<string>("Item");
            int number;

            //scan the datatable for same item
            for(int scanRow = 0; scanRow < ticketDataTable.Rows.Count; scanRow++)
            {
                if(ticketDataTable.Rows[scanRow].Field<string>("Item") == shortcodeItem) //found an item
                {
                    //get the current number of items
                    try
                    {
                        number = Convert.ToInt32(ticketDataTable.Rows[scanRow].Field<string>("Quantity"));
                    } catch
                    {
                        number = 0;
                    }

                    number++;
                    ticketDataTable.Rows[scanRow][1] = number.ToString();
                    //ticketDataTable.Rows[scanRow][Field<string>("Quantity")] = number.ToString();

                    //job done so exit
                    return;
                }
            }

            //add the item to the next row of the ticket
            DataRow newRow;
            newRow = ticketDataTable.NewRow();
            newRow["Item"] = shortcodeItem;
            newRow["Quantity"] = "1";

            ticketDataTable.Rows.Add(newRow);

        }

        private int FindRowIndex(DataGridRow row)
        {
            DataGrid dataGrid = ItemsControl.ItemsControlFromItemContainer(row) as DataGrid;

            int index = dataGrid.ItemContainerGenerator.IndexFromContainer(row);

            return index;
        }

        private void dataGridJSShortCodes_Click(object sender, RoutedEventArgs e)
        {
            //Row double clicked
            //get the row that has been clicked on *** VERY MUCH BORROWED CODE ***
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            //while ((dep != null) && (dep is not DataGridCell) && (dep is not DataGridColumnHeader))
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

                AddClickedItemToTicket(FindRowIndex(row));
                
            }

         

        }

        private void dataGridJSRemoveRow_Click(object sender, RoutedEventArgs e)
        {

            //Row double clicked
            //get the row that has been clicked on *** VERY MUCH BORROWED CODE ***
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            //while ((dep != null) && (dep is not DataGridCell) && (dep is not DataGridColumnHeader))
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

                //AddClickedItemToTicket(FindRowIndex(row));
                DeleteTicketRow(FindRowIndex(row));

            }

        }

        private void DeleteTicketRow(int row)
        {
            if(row < ticketDataTable.Rows.Count)
            {
         
                ticketDataTable.Rows.RemoveAt(row);
            }
            

        }
    }
}
