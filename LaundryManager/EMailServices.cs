using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LaundryManager
{
    class EMailServices
    {
        public string SendEmail(List<string> eMailAddresses, string emSubject, string emBody)
        {
            string to = "";
            foreach(string email in eMailAddresses)
            {
                to = to + "," + email;
            }
            to = to.Substring(1, to.Length - 1);            
            string from = "team@woldslaundryservices.co.uk";
            MailMessage message = new MailMessage(from, to);
            message.Subject = emSubject;
            message.Body = emBody;
            SmtpClient client = new SmtpClient("mta.averysradnage.co.uk");
            try
            {
                client.Send(message);
                return "Message Sent";
            } catch(Exception e)
            {
                return getIP();
            }            
        }

        private string getIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            bool correctIP = false;

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if(ip.ToString() == "192.168.17.10")
                    {
                        correctIP = true;
                    }
                }
            }

            if(correctIP == true)
            {
                return "FAILED: Other EMail Issue";
            } 
            else
            {
                return "FAILED: IP should be 192.168.17.10";
            }
       

        }

    }
}
