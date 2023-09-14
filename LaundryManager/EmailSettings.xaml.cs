using System;
using System.IO;
using System.Xml;
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
using System.Data;

namespace LaundryManager
{
    /// <summary>
    /// Interaction logic for EmailSettings.xaml
    /// </summary>
    public partial class EmailSettings : Window
    {

        string glob_MailServerSettingsFilePath = "";
        public EmailSettings()
        {
            InitializeComponent();
            glob_MailServerSettingsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\MailServerSettings.xml";
        }

        private void EmailSettings_Window_Loaded(object sender, RoutedEventArgs e)
        {
            //check there is a file and pull its data
            LoadFormData();
        }

        private void LoadFormData()
        {
            //location of the file
            if (File.Exists(glob_MailServerSettingsFilePath))
            {
                //read in the data
                ReadSettingsFile();
            } else
            {
                //create the file
                CreateSettingsFile();
                ReadSettingsFile();
            }


        }

        private void ReadSettingsFile()
        {
            MailServerSettings mailSettings = new();

            DataTable dt = mailSettings.MailServerSettingsTable(glob_MailServerSettingsFilePath);
            

            //DataSet myDataSet = new();
            //DataTable myDataTable = new DataTable();
            //XmlReader reader = XmlReader.Create(glob_MailServerSettingsFilePath, new XmlReaderSettings());

            //myDataSet.ReadXml(reader);
            //myDataTable = myDataSet.Tables[0];
            //reader.Close();

            ESMailServerTextBox.Text = dt.Rows[0][0].ToString();
            ESFromTextBox.Text = dt.Rows[0][1].ToString();

        }

        private void CreateSettingsFile()
        {
            XmlDocument settingsFileDoc = new();
            XmlDeclaration xmlDeclaration = settingsFileDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = settingsFileDoc.DocumentElement;
            settingsFileDoc.InsertBefore(xmlDeclaration, root);

            XmlElement settingsRootNode = settingsFileDoc.CreateElement(string.Empty, "SettingsRootNode", string.Empty);
            settingsFileDoc.AppendChild(settingsRootNode);

            XmlElement settingsNode = settingsFileDoc.CreateElement(string.Empty, "Settings", string.Empty);
            settingsRootNode.AppendChild(settingsNode);

            XmlElement mailServerNode = settingsFileDoc.CreateElement(string.Empty, "MailServer", string.Empty);
            XmlText mailSever_txt = settingsFileDoc.CreateTextNode("192.168.5.33");
            mailServerNode.AppendChild(mailSever_txt);
            settingsNode.AppendChild(mailServerNode);

            XmlElement toEmailNode = settingsFileDoc.CreateElement(string.Empty, "ToAddress", string.Empty);
            XmlText toEmail_txt = settingsFileDoc.CreateTextNode("team@woldslaundryservices.co.uk");
            toEmailNode.AppendChild(toEmail_txt);
            settingsNode.AppendChild(toEmailNode);

            settingsFileDoc.Save(glob_MailServerSettingsFilePath);
        }

        private void ESSaveButton_Click(object sender, RoutedEventArgs e)
        {
            //take the settings from the email box and save them to the xml file
            XmlDocument document = new XmlDocument();
            document.Load(glob_MailServerSettingsFilePath);
            XmlNodeList nodes = document.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if(node.Name == "SettingsRootNode")
                {
                    //settiings node
                    foreach (XmlNode childnode in node)
                    {
                        if(childnode.Name == "Settings")
                        {
                            foreach(XmlNode grandChild in childnode)
                            {
                                if(grandChild.Name == "MailServer")
                                {
                                    grandChild.InnerText = ESMailServerTextBox.Text;
                                }
                                if(grandChild.Name == "ToAddress")
                                {
                                    grandChild.InnerText = ESFromTextBox.Text;
                                }

                            }
                        }
                    }
                }
                

            }

            document.Save(glob_MailServerSettingsFilePath);
            Close();

        }

        private void ESCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    class MailServerSettings
    {
        public DataTable MailServerSettingsTable(string settingsPath)
        {
            DataSet myDataSet = new();
            DataTable myDataTable;// = new DataTable();
            XmlReader reader = XmlReader.Create(settingsPath, new XmlReaderSettings());

            myDataSet.ReadXml(reader);
            myDataTable = myDataSet.Tables[0];
            reader.Close();
            return myDataTable;
        }
    }
}
