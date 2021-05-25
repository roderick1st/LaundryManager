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

        public List<string> GetCustomerNumbers(string filePath)
        {
            
            Debug.WriteLine("Loading");

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

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
                    }
                }
                cnList.Add(buildCustomerString);
            }
            return cnList;        
        }

        public List<string> GetSelectedCustomerDetails(string filePath, string customerSelected)
        {

            //lets pull the customer number infor that we want to search for
            string currentCustomerNumber = customerSelected.Substring(3,customerSelected.IndexOf("-")-4);// get the customer number
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);
            string buildDetailsString = "Customer Number ";
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

        
    }
}
