using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Xml.Linq;

namespace LaundryManager
{
    /// <summary>
    /// Interaction logic for NewPriceList.xaml
    /// </summary>
    public partial class NewPriceList : Window
    {

        //global variables
        string glob_ItemsListFilePath = "";
        string glob_PriceListsFolder = "";
        DataSet glob_currentItemsDataSet = new();
        public NewPriceList()
        {
            InitializeComponent();
            glob_ItemsListFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\ShortCodes.xml";
            glob_PriceListsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\PriceLists";

            //create the pricelist folder if it does not exist
            if (!Directory.Exists(glob_PriceListsFolder))
            {
                Directory.CreateDirectory(glob_PriceListsFolder);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Billing billing = new Billing();
            billing.ShowDialog();
            base.OnClosed(e);
        }


        private void Price_List_Window_Loaded(object sender, RoutedEventArgs e)
        {

            GetAllItems();
            GetCurrentPriceOnItems();
            SetPriceListNameTextBox();
            ItemsPricingDataGrid.ItemsSource = new DataView(glob_currentItemsDataSet.Tables[0]);
           
        }

        private void SetPriceListNameTextBox()
        {
            string name = DateTime.Now.ToString("MMM") + " " + DateTime.Now.ToString("yyyy");
            TxtBoxPriceListName.Text = name;
        }

        private void GetAllItems()
        {

            string currentCode = "";

            //DataSet currentItems = new();
            DataTable itemsTable = new();

            glob_currentItemsDataSet.Tables.Add(itemsTable);

            itemsTable.Columns.AddRange(new DataColumn[3] { new DataColumn("Code"), new DataColumn("Item"), new DataColumn("Price") });


            XmlDocument itemsXmlDoc = new();
            itemsXmlDoc.Load(glob_ItemsListFilePath);

            XmlElement root = itemsXmlDoc.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("*"); //select all nodes

            foreach (XmlNode node in nodes) //loop each code
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    if(item.Name == "Code")
                    {
                        currentCode = item.InnerText;
                    } else
                    {
                        itemsTable.Rows.Add(currentCode, item.InnerText);
                    }
                        

                }
            }
        
        }

        private void GetCurrentPriceOnItems()
        {
            //scan the directory for files and read the date for each price list using the latest pricelist
            string[] priceLists = Directory.GetFiles(glob_PriceListsFolder, "*", SearchOption.TopDirectoryOnly);
            DateTime latestDate = DateTime.Parse("01-01-2000 00:00:00");
            DateTime FileDate = DateTime.Parse("01-01-2000 00:00:00");
            string latestPriceList = "";

            XmlDocument xmlDocument = new();

            foreach (string priceList in priceLists)
            {
                xmlDocument.Load(priceList);

                XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("Date");

                FileDate = DateTime.Parse(xmlNode.InnerText);

                if(FileDate > latestDate)
                {
                    latestDate = FileDate;
                    latestPriceList = priceList;
                }

                //<PriceList>
                //      <date>01-02-2023 14:40:34</date>
                //</PriceList>


                xmlDocument.RemoveAll();

                //var xmlStr = XElement.Parse(priceList);
                //var priceListDate = xmlStr.Elements("PriceList").Where(x => x.Element("Date").Value.Equals()

            }

            if (latestPriceList != "")
            {
                xmlDocument.Load(latestPriceList);

                XmlNodeList Items = xmlDocument.DocumentElement.SelectNodes("Item");

                string xmlCodeFound = "";
                string xmlPriceFound = "";

                foreach (XmlNode Item in Items)
                {
                    xmlCodeFound = "";
                    xmlPriceFound = "";

                    //XmlNode xmlCode
                    xmlCodeFound = Item.SelectSingleNode("Code").InnerText;
                    xmlPriceFound = Item.SelectSingleNode("Price").InnerText;

                    foreach (DataRow row in glob_currentItemsDataSet.Tables[0].Rows)
                    {
                        if (xmlCodeFound == row["Code"].ToString())
                        {
                            row["Price"] = xmlPriceFound;
                        }
                    }
                }


            }

        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            //grab the date time
            DateTime saveTime = DateTime.Now;
            string fileName = glob_PriceListsFolder + "\\" + saveTime.Day.ToString() + saveTime.Month.ToString() + saveTime.Year.ToString() + saveTime.Hour.ToString() + saveTime.Minute.ToString() + saveTime.Second.ToString() + ".xml";
            string currentTimeStamp = saveTime.ToString();

            //built new xml file
            XmlDocument xmlDocument = new XmlDocument();

            XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xmlDocument.DocumentElement;
            xmlDocument.InsertBefore(xmlDeclaration, root);

            XmlElement PriceListNode = xmlDocument.CreateElement(string.Empty, "PriceList", string.Empty);
            xmlDocument.AppendChild(PriceListNode);

            XmlElement NameNode = xmlDocument.CreateElement(string.Empty, "Name", string.Empty);
            XmlText nameTxt = xmlDocument.CreateTextNode(TxtBoxPriceListName.Text);
            NameNode.AppendChild(nameTxt);
            PriceListNode.AppendChild(NameNode);

            XmlElement DateNode = xmlDocument.CreateElement(string.Empty, "Date", string.Empty);
            XmlText dateTxt = xmlDocument.CreateTextNode(currentTimeStamp);
            DateNode.AppendChild(dateTxt);
            PriceListNode.AppendChild(DateNode);

            foreach(DataRow row in glob_currentItemsDataSet.Tables[0].Rows)
            {
                XmlElement ItemNode = xmlDocument.CreateElement(string.Empty, "Item", string.Empty);
                PriceListNode.AppendChild(ItemNode);
                XmlElement CodeNode = xmlDocument.CreateElement(string.Empty, "Code", string.Empty);
                XmlText CodeTxt = xmlDocument.CreateTextNode(row["Code"].ToString());
                CodeNode.AppendChild(CodeTxt);
                ItemNode.AppendChild(CodeNode);
                XmlElement ItemDSNode = xmlDocument.CreateElement(string.Empty, "Desc", string.Empty);
                XmlText ItemTxt = xmlDocument.CreateTextNode(row["Item"].ToString());
                ItemDSNode.AppendChild(ItemTxt);
                ItemNode.AppendChild(ItemDSNode);
                XmlElement PriceNode = xmlDocument.CreateElement(string.Empty, "Price", string.Empty);
                XmlText PriceTxt = xmlDocument.CreateTextNode(row["Price"].ToString());
                PriceNode.AppendChild(PriceTxt);
                ItemNode.AppendChild(PriceNode);
            }

            xmlDocument.Save(fileName);
            this.Close();

        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
