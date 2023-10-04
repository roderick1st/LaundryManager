using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LaundryManager
{
    class CustomerDetailsFile
    {
        //string glob_FilePath = "E:\\c_Sharp_Solutions\\LaundryManager\\LaundryManager\\Data\\CustomerDetails.xml";
        static string glob_FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\CustomerDetails.xml";

        public List<string> GetCustomerNumbers(bool onlyActiveCustomers)
        {
            bool addToList = true;
            
            Debug.WriteLine("Loading");

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(glob_FilePath);

            List<string> cnList = new List<string>();

            string buildCustomerString = "";
            XmlElement root = xmlDocument.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("Customer");
            foreach (XmlNode node in nodes)
            {
                
                    foreach (XmlNode childnode in node.ChildNodes)
                    {
                    switch (childnode.Name)
                    {
                        case "CN":                 
                            buildCustomerString = "CN " + childnode.InnerText;
                            break;

                        case "FirstName":
                            buildCustomerString = buildCustomerString + " - " + childnode.InnerText;
                            break;

                        case "LastName":
                            buildCustomerString = buildCustomerString + " " + childnode.InnerText;
                            break;

                        case "Active": //only present the customer if they are active.
                            if ((childnode.InnerText == "no") & (onlyActiveCustomers == true))
                            {
                                addToList = false;
                            }
                            else
                            {
                                addToList = true;
                            }
                            break;
                    }
                }
                if(addToList == true)//only add to the list if wanted is the user active and only active users requested
                {
                    cnList.Add(buildCustomerString);
                }
                
            }
            return cnList;        
        }

        public List<string> GetSelectedCustomerDetails(string customerSelected)
        {

            //lets pull the customer number infor that we want to search for
            string currentCustomerNumber = customerSelected.Substring(3,customerSelected.IndexOf("-")-4);// get the customer number
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(glob_FilePath);
            //string buildDetailsString = "Customer Number ";
            XmlElement root = xmlDocument.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("Customer");

            //create list to hold our information
            List<string> custInfoList = new List<string>();

            foreach (XmlNode node in nodes)
            {               
                if((node.FirstChild.Name.ToString() == "CN") & (node.FirstChild.InnerText == currentCustomerNumber))
                {
                    //found the customer so loop through the rest of the nodes getting all the data missing blanks and sorting names out
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        custInfoList.Add(childNode.InnerText);                     
                    }
                }
            }

            return custInfoList;
            
        }

        public void EditCustomer(List<string> customerDetailsList)
        {

            bool enableEdit = false;
            bool legacyRepairComplete = false;
            int recordCounter;
            int nodeFixcounter;

            while (!legacyRepairComplete)
            {
                recordCounter = 0;
                legacyRepairComplete = true;
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(glob_FilePath);

                XmlElement root = xmlDocument.DocumentElement;
                XmlNodeList nodes = root.SelectNodes("Customer");
                foreach (XmlNode node in nodes)//for each customer
                {
                    nodeFixcounter = 2;

                    foreach (XmlNode childnode in node.ChildNodes)//look at each child node
                    {
                        //find the same customer number in the list
                        if ((childnode.Name == "CN") & (childnode.InnerText == customerDetailsList[0]))
                        {
                            enableEdit = true;
                        }
                        if (enableEdit)
                        {
                            childnode.InnerText = customerDetailsList[recordCounter];

                            //deal with legacy issues
                           
                            /*
                            if (childnode.Name == "option2")
                            {
                                legacyRepairComplete = false;
                                XmlNode replacementNode = xmlDocument.CreateElement(string.Empty, "DiscountPrice", string.Empty);
                                replacementNode.InnerXml = childnode.InnerXml; //copy the contents
                                node.InsertBefore(replacementNode, childnode);
                                node.RemoveChild(childnode);

                            }
                            if (childnode.Name == "option3")
                            {
                                legacyRepairComplete = false;
                                XmlNode replacementNode = xmlDocument.CreateElement(string.Empty, "DiscountStartAmount", string.Empty);
                                replacementNode.InnerXml = childnode.InnerXml; //copy the contents
                                node.InsertBefore(replacementNode, childnode);
                                node.RemoveChild(childnode);

                            }
                            if (childnode.Name == "option4")
                            {
                                legacyRepairComplete = false;
                                XmlNode replacementNode = xmlDocument.CreateElement(string.Empty, "DeliveryCharge", string.Empty);
                                replacementNode.InnerXml = childnode.InnerXml; //copy the contents
                                node.InsertBefore(replacementNode, childnode);
                                node.RemoveChild(childnode);

                            }
                            */

                            recordCounter++;
                        }

                    }
                    enableEdit = false; //dont keep editing past the one we want to edit
                }
                //save the file
                xmlDocument.Save(glob_FilePath);
        }
        }

        public void AddNewCustomer(List<string> customerDetailsList)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(glob_FilePath);

            //build the data
            XmlNode CustomersNode = xmlDocument.SelectSingleNode("Customers");
            XmlNode newCustomerNode = xmlDocument.CreateNode(XmlNodeType.Element, "Customer", null);
            XmlNode newCNNode = xmlDocument.CreateNode(XmlNodeType.Element, "CN", null);
            XmlText newCNText = xmlDocument.CreateTextNode(customerDetailsList[0]);
            XmlNode newFirstNameNode = xmlDocument.CreateNode(XmlNodeType.Element, "FirstName", null);
            XmlText newFirstNameText = xmlDocument.CreateTextNode(customerDetailsList[1]);
            XmlNode newLastNameNode = xmlDocument.CreateNode(XmlNodeType.Element, "LastName", null);
            XmlText newLastNameText = xmlDocument.CreateTextNode(customerDetailsList[2]);
            XmlNode newAddressLn1Node = xmlDocument.CreateNode(XmlNodeType.Element, "AddressLn1", null);
            XmlText newAddressLn1Text = xmlDocument.CreateTextNode(customerDetailsList[3]);
            XmlNode newAddressLn2Node = xmlDocument.CreateNode(XmlNodeType.Element, "AddressLn2", null);
            XmlText newAddressLn2Text = xmlDocument.CreateTextNode(customerDetailsList[4]);
            XmlNode newAddressLn3Node = xmlDocument.CreateNode(XmlNodeType.Element, "AddressLn3", null);
            XmlText newAddressLn3Text = xmlDocument.CreateTextNode(customerDetailsList[5]);
            XmlNode newAddressTownNode = xmlDocument.CreateNode(XmlNodeType.Element, "AddressTown", null);
            XmlText newAddressTownText = xmlDocument.CreateTextNode(customerDetailsList[6]);
            XmlNode newAddressCountyNode = xmlDocument.CreateNode(XmlNodeType.Element, "AddressCounty", null);
            XmlText newAddressCountyText = xmlDocument.CreateTextNode(customerDetailsList[7]);
            XmlNode newAddressPostCodeNode = xmlDocument.CreateNode(XmlNodeType.Element, "AddressPostCode", null);
            XmlText newAddressPostCodeText = xmlDocument.CreateTextNode(customerDetailsList[8]);
            XmlNode newPhonePrimaryNode = xmlDocument.CreateNode(XmlNodeType.Element, "PhonePrimary", null);
            XmlText newPhonePrimaryText = xmlDocument.CreateTextNode(customerDetailsList[9]);
            XmlNode newPhoneSecondaryNode = xmlDocument.CreateNode(XmlNodeType.Element, "PhoneSecondary", null);
            XmlText newPhoneSecondaryText = xmlDocument.CreateTextNode(customerDetailsList[10]);
            XmlNode newPhoneTertiaryNode = xmlDocument.CreateNode(XmlNodeType.Element, "PhoneTertiary", null);
            XmlText newPhoneTertiaryText = xmlDocument.CreateTextNode(customerDetailsList[11]);
            XmlNode newEmailPrimaryNode = xmlDocument.CreateNode(XmlNodeType.Element, "EmailPrimary", null);
            XmlText newEmailPrimaryText = xmlDocument.CreateTextNode(customerDetailsList[12]);
            XmlNode newEmailSecondaryNode = xmlDocument.CreateNode(XmlNodeType.Element, "EmailSecondary", null);
            XmlText newEmailSecondaryText = xmlDocument.CreateTextNode(customerDetailsList[13]);
            XmlNode newEmailTertiaryNode = xmlDocument.CreateNode(XmlNodeType.Element, "EmailTertiary", null);
            XmlText newEmailTertiaryText = xmlDocument.CreateTextNode(customerDetailsList[14]);
            XmlNode newActiveNode = xmlDocument.CreateNode(XmlNodeType.Element, "Active", null);
            XmlText newActiveText = xmlDocument.CreateTextNode(customerDetailsList[15]);
            XmlNode newNotesNode = xmlDocument.CreateNode(XmlNodeType.Element, "Notes", null);
            XmlText newNotesText = xmlDocument.CreateTextNode(customerDetailsList[16]);
            XmlNode newPriceStructureNode = xmlDocument.CreateNode(XmlNodeType.Element, "DiscountPercent", null);
            XmlText newPriceStructureText = xmlDocument.CreateTextNode(customerDetailsList[17]);
            XmlNode newDeliveryChargeNode = xmlDocument.CreateNode(XmlNodeType.Element, "DiscountStartAmount", null);
            XmlText newDeliveryChargeText = xmlDocument.CreateTextNode(customerDetailsList[18]);
            XmlNode newoption4Node = xmlDocument.CreateNode(XmlNodeType.Element, "DeliveryCharge", null);
            XmlText newoption4Text = xmlDocument.CreateTextNode(customerDetailsList[19]);
            XmlNode newoption5Node = xmlDocument.CreateNode(XmlNodeType.Element, "option5", null);
            XmlText newoption5Text = xmlDocument.CreateTextNode(customerDetailsList[20]);
            XmlNode newoption6Node = xmlDocument.CreateNode(XmlNodeType.Element, "option6", null);
            XmlText newoption6Text = xmlDocument.CreateTextNode(customerDetailsList[21]);

            //append the data
            newCustomerNode.AppendChild(newCNNode); newCNNode.AppendChild(newCNText);
            newCustomerNode.AppendChild(newFirstNameNode); newFirstNameNode.AppendChild(newFirstNameText);
            newCustomerNode.AppendChild(newLastNameNode); newLastNameNode.AppendChild(newLastNameText);
            newCustomerNode.AppendChild(newAddressLn1Node); newAddressLn1Node.AppendChild(newAddressLn1Text);
            newCustomerNode.AppendChild(newAddressLn2Node); newAddressLn2Node.AppendChild(newAddressLn2Text);
            newCustomerNode.AppendChild(newAddressLn3Node); newAddressLn3Node.AppendChild(newAddressLn3Text);
            newCustomerNode.AppendChild(newAddressTownNode); newAddressTownNode.AppendChild(newAddressTownText);
            newCustomerNode.AppendChild(newAddressCountyNode);newAddressCountyNode.AppendChild(newAddressCountyText);
            newCustomerNode.AppendChild(newAddressPostCodeNode); newAddressPostCodeNode.AppendChild(newAddressPostCodeText);
            newCustomerNode.AppendChild(newPhonePrimaryNode); newPhonePrimaryNode.AppendChild(newPhonePrimaryText);
            newCustomerNode.AppendChild(newPhoneSecondaryNode); newPhoneSecondaryNode.AppendChild(newPhoneSecondaryText);
            newCustomerNode.AppendChild(newPhoneTertiaryNode); newPhoneTertiaryNode.AppendChild(newPhoneTertiaryText);
            newCustomerNode.AppendChild(newEmailPrimaryNode); newEmailPrimaryNode.AppendChild(newEmailPrimaryText);
            newCustomerNode.AppendChild(newEmailSecondaryNode); newEmailSecondaryNode.AppendChild(newEmailSecondaryText);
            newCustomerNode.AppendChild(newEmailTertiaryNode); newEmailTertiaryNode.AppendChild(newEmailTertiaryText);
            newCustomerNode.AppendChild(newActiveNode); newActiveNode.AppendChild(newActiveText);
            newCustomerNode.AppendChild(newNotesNode); newNotesNode.AppendChild(newNotesText);
            newCustomerNode.AppendChild(newPriceStructureNode); newPriceStructureNode.AppendChild(newPriceStructureText);
            newCustomerNode.AppendChild(newDeliveryChargeNode); newDeliveryChargeNode.AppendChild(newDeliveryChargeText);
            newCustomerNode.AppendChild(newoption4Node); newoption4Node.AppendChild(newoption4Text);
            newCustomerNode.AppendChild(newoption5Node); newoption5Node.AppendChild(newoption5Text);
            newCustomerNode.AppendChild(newoption6Node); newoption6Node.AppendChild(newoption6Text);


            CustomersNode.AppendChild(newCustomerNode);

            //save the file
            xmlDocument.Save(glob_FilePath);

        }

        
    }

    class CNHandling
    {
        public int ExtractCN(string unsortedString) //make sure this does not get string with more than one set of numbers
        {
      
       
            string number = new(unsortedString.SkipWhile(c => !char.IsDigit(c))
                                              .TakeWhile(c => char.IsDigit(c))
                                              .ToArray());

            if(int.TryParse(number, out int returnNumber))
            {
                return returnNumber;
            } else
            {
                return 0;
            }

            
        }
    }

}
