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
    class Messages
    {
        //location of the xml file holding the contents of messages
        static string glob_FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\MailMessages.xml";

        public List<string> GetMessage(int messageID)
        {
            List<string> Message = new List<string>();
            String messageString = "";
            bool SaveMessage = false;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(glob_FilePath);

            XmlElement root = xmlDocument.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("Message");
            foreach (XmlNode node in nodes)                         //loop the message nodes
            {
                if(SaveMessage == true)
                {
                    Message.Add(messageString);
                    return Message;
                }
                foreach (XmlNode childnode in node.ChildNodes)      //loop each item in a message node
                {
                    //start saving the message if saveMessage is true
                    if(SaveMessage == true)
                    {
                        if (childnode.Name == "Salutation")
                        {
                            Message.Add(childnode.InnerText);
                        } else if (childnode.Name.Substring(0,4).ToLower() == "line") //are we on a line
                        {
                            if(childnode.InnerText == "")
                            {
                                messageString = messageString + "\r\n";
                            } else
                            {
                                messageString = messageString + childnode.InnerText;
                            }
                            
                        }
                    }

                    if(childnode.Name == "ID")                      //find the ID node
                    {
                        if(Int32.TryParse(childnode.InnerText, out int idValue))
                        {
                            if(idValue == messageID)        //is this the message we want
                            {
                                SaveMessage = true;
                            }
                        }
                        
                    }
                }
            }

            return Message;


        }


    }
}