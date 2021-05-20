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

        //public void (string strItem)
        //{

        //}
    }
}
