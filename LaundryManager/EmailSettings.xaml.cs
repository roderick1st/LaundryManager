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
            CheckTextBoxs();
        }

        private void LoadFormData()
        {
            //location of the file
            if (File.Exists(glob_MailServerSettingsFilePath))
            {
                //read in the data
                CheckFileIsUpToDate();
                ReadSettingsFile();
            } else
            {
                //create the file
                CreateSettingsFile(1);
                ReadSettingsFile();
            }


        }

        private void CheckFileIsUpToDate()
        {
            //take the settings from the email box and save them to the xml file
            XmlDocument document = new XmlDocument();
            document.Load(glob_MailServerSettingsFilePath);
            XmlNodeList nodes = document.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "SettingsRootNode")
                {
                    //settiings node
                    foreach (XmlNode childnode in node)
                    {
                        if (childnode.Name == "Settings")
                        {
                            //check if all nodes exist
                            int nodeCount = 0;
                            foreach (XmlNode grandChild in childnode)
                            {
                                nodeCount++;
                            }

                            if (nodeCount < 6)
                            {
                                CreateSettingsFile(2);
                            }
                        }
                    }
                }
            }
        }

        private void ReadSettingsFile()
        {
            MailServerSettings mailSettings = new();

            DataTable dt = mailSettings.MailServerSettingsTable(glob_MailServerSettingsFilePath);        

            ESMailServerTextBox.Text = dt.Rows[0]["MailServer"].ToString();
            ESPortTextBox.Text = dt.Rows[0]["SMTPPort"].ToString();
            ESFromTextBox.Text = dt.Rows[0]["ToAddress"].ToString();
            ESReplyTextBox.Text = dt.Rows[0]["ReplyAddress"].ToString();
            ESUnameTextBox.Text = dt.Rows[0]["Username"].ToString();
            ESPassTextBox.Text = dt.Rows[0]["Password"].ToString();
        }

        private void CreateSettingsFile(int calledFrom)
        {
            if(calledFrom != 1)
            {
                if (File.Exists(glob_MailServerSettingsFilePath))
                {
                    File.Delete(glob_MailServerSettingsFilePath);
                }
            }

            XmlDocument settingsFileDoc = new();
            XmlDeclaration xmlDeclaration = settingsFileDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = settingsFileDoc.DocumentElement;
            settingsFileDoc.InsertBefore(xmlDeclaration, root);

            XmlElement settingsRootNode = settingsFileDoc.CreateElement(string.Empty, "SettingsRootNode", string.Empty);
            settingsFileDoc.AppendChild(settingsRootNode);

            XmlElement settingsNode = settingsFileDoc.CreateElement(string.Empty, "Settings", string.Empty);
            settingsRootNode.AppendChild(settingsNode);

            XmlElement mailServerNode = settingsFileDoc.CreateElement(string.Empty, "MailServer", string.Empty);
            //XmlText mailSever_txt = settingsFileDoc.CreateTextNode("smtp.averysradnage.co.uk");
            //mailServerNode.AppendChild(mailSever_txt);
            settingsNode.AppendChild(mailServerNode);

            XmlElement mailServerPortNode = settingsFileDoc.CreateElement(string.Empty, "SMTPPort", string.Empty);
            //XmlText mailSeverPort_txt = settingsFileDoc.CreateTextNode("587");
            //mailServerPortNode.AppendChild(mailSeverPort_txt);
            settingsNode.AppendChild(mailServerPortNode);

            XmlElement toEmailNode = settingsFileDoc.CreateElement(string.Empty, "ToAddress", string.Empty);
            //XmlText toEmail_txt = settingsFileDoc.CreateTextNode("theteam@woldslaundryservices.co.uk");
            //toEmailNode.AppendChild(toEmail_txt);
            settingsNode.AppendChild(toEmailNode);

            XmlElement replyEmailNode = settingsFileDoc.CreateElement(string.Empty, "ReplyAddress", string.Empty);
            //XmlText replyEmail_txt = settingsFileDoc.CreateTextNode("team@woldslaundryservices.co.uk");
            //replyEmailNode.AppendChild(replyEmail_txt);
            settingsNode.AppendChild(replyEmailNode);

            XmlElement usernameNode = settingsFileDoc.CreateElement(string.Empty, "Username", string.Empty);
            //XmlText replyEmail_txt = settingsFileDoc.CreateTextNode("team@woldslaundryservices.co.uk");
            //replyEmailNode.AppendChild(replyEmail_txt);
            settingsNode.AppendChild(usernameNode);

            XmlElement passwordNode = settingsFileDoc.CreateElement(string.Empty, "Password", string.Empty);
            //XmlText replyEmail_txt = settingsFileDoc.CreateTextNode("team@woldslaundryservices.co.uk");
            //replyEmailNode.AppendChild(replyEmail_txt);
            settingsNode.AppendChild(passwordNode);

            settingsFileDoc.Save(glob_MailServerSettingsFilePath);
        }

        private void ESSaveButton_Click(object sender, RoutedEventArgs e)
        {

            ProcessEmailSettings();

        }
        private void ProcessEmailSettings()
        {
            //take the settings from the email box and save them to the xml file
            XmlDocument document = new XmlDocument();
            document.Load(glob_MailServerSettingsFilePath);
            XmlNodeList nodes = document.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.Name == "SettingsRootNode")
                {
                    //settiings node
                    foreach (XmlNode childnode in node)
                    {
                        if (childnode.Name == "Settings")
                        {                           
                            foreach (XmlNode grandChild in childnode)
                            {
                                if (grandChild.Name == "MailServer")
                                {
                                    grandChild.InnerText = ESMailServerTextBox.Text;
                                }
                                if (grandChild.Name == "SMTPPort")
                                {
                                    grandChild.InnerText = ESPortTextBox.Text;
                                }
                                if (grandChild.Name == "ToAddress")
                                {
                                    grandChild.InnerText = ESFromTextBox.Text;
                                }
                                if (grandChild.Name == "ReplyAddress")
                                {
                                    grandChild.InnerText = ESReplyTextBox.Text;
                                }
                                if (grandChild.Name == "Username")
                                {
                                    grandChild.InnerText = ESUnameTextBox.Text;
                                }
                                if (grandChild.Name == "Password")
                                {
                                    grandChild.InnerText = ESPassTextBox.Text;
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

        private void CheckTextBoxs()
        {
            if (ESMailServerTextBox.Text.Length < 1)
            {
                ESMailServerTextBox.Text = "smtp.averysradnage.co.uk";
            }
            if (ESFromTextBox.Text.Length < 1)
            {
                ESFromTextBox.Text = "theteam@woldslaundryservices.co.uk";
            }
            if (ESReplyTextBox.Text.Length < 1)
            {
                ESReplyTextBox.Text = "team@woldslaundryservices.co.uk";
            }
            if (ESPortTextBox.Text.Length < 1)
            {
                ESPortTextBox.Text = "587";
            }
            if (ESUnameTextBox.Text.Length < 1)
            {
                ESUnameTextBox.Text = "woldslaundry.services@averysradnage.co.uk";
            }
            if (ESPassTextBox.Text.Length < 1)
            {
                ESPassTextBox.Text = "@AveryGaunt11@";
            }

        }

        private void ESMailServerTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESMailServerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESFromTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESFromTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESReplyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESReplyTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESPortTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESPortTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESUnameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESUnameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESPassTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
        }

        private void ESPassTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckTextBoxs();
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
