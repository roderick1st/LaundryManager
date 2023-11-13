using System;
using System.Collections.Generic;
using System.Data;
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

            //get the mail server settings
            MailServerSettings mailSettings = new();
            string mailServerSettingsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\MailServerSettings.xml";
            DataTable dt = mailSettings.MailServerSettingsTable(mailServerSettingsFilePath);

            
            string mailServer = dt.Rows[0]["MailServer"].ToString();
            int smtpPort = int.Parse(dt.Rows[0]["SMTPPort"].ToString());
            string fromAddress = dt.Rows[0]["ToAddress"].ToString();
            string replyAddress = dt.Rows[0]["ReplyAddress"].ToString();
            string username = dt.Rows[0]["Username"].ToString();
            string password = dt.Rows[0]["Password"].ToString();


            //string to = "";
            //to = "rod.james.avery@googlemail.com";

            MailMessage message = new MailMessage();

            //message.From = new MailAddress(fromAddress);
            message.From = new MailAddress(fromAddress);
            foreach (string email in eMailAddresses)
            {
                message.To.Add(email);
            }

            message.ReplyToList.Add(replyAddress);// = new MailAddress(replyAddress);
            message.Subject = emSubject;
            message.Body = emBody;
            
            SmtpClient client = new SmtpClient(mailServer, smtpPort);

            //@AveryGaunt11@
            //woldslaundry.services@averysradnage.co.uk

            client.Credentials = new NetworkCredential(username, password);
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            //client.UseDefaultCredentials = true;
        
            try
            {
                client.Send(message);
                return "Message Sent";
            }
            catch (Exception e)
            {
                return getIP(e);
            }
        }

        /*public string SendEmail(List<string> eMailAddresses, string emSubject, string emBody)
        {

            //get the mail server settings
            MailServerSettings mailSettings = new();
            string mailServerSettingsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Laundry\\MailServerSettings.xml";
            DataTable dt = mailSettings.MailServerSettingsTable(mailServerSettingsFilePath);

            string mailServer = dt.Rows[0][0].ToString();
            string fromAddress = dt.Rows[0][1].ToString();

            string to = "";
            foreach(string email in eMailAddresses)
            {
                to = to + "," + email;
            }
            to = to.Substring(1, to.Length - 1);
            //to = "rod.james.avery@googlemail.com";

            MailMessage message = new MailMessage(fromAddress, to);
            message.Subject = emSubject;
            message.Body = emBody;
            //SmtpClient client = new SmtpClient("mta.averysradnage.co.uk");
            SmtpClient client = new SmtpClient(mailServer);
            try
            {
                client.Send(message);
                return "Message Sent";
            } catch(Exception e)
            {
                return getIP(e);
            }            
        }*/

        /*public string SendEmail(List<string> eMailAddresses, string emSubject, string emBody)
        {
            using(SmtpClient smtpClient = new SmtpClient())
            {
                //var basicCredential = new NetworkCredential("laundry.application@averysradnage.co.uk", "k17-VXXj2xXglIn");
                using(MailMessage message = new MailMessage())
                {
                    MailAddress fromAddress = new MailAddress("alerts@woldslaundryservices.co.uk");
                    smtpClient.Host = "192.168.5.33";
                    smtpClient.Port = 25;
                    //smtpClient.UseDefaultCredentials = false;
                    //smtpClient.Credentials = basicCredential;

                    message.From = fromAddress;
                    message.Subject = emSubject;
                    message.IsBodyHtml = false;
                    message.Body = emBody;

                    //foreach(string emailToAddress in eMailAddresses)
                    //{
                    //    message.To.Add(emailToAddress);
                    //}
                    message.To.Add("rod.james.avery@googlemail.com");

                    try
                    {
                        smtpClient.Send(message);
                        return "Message Sent";
                    }
                    catch(Exception ex)
                    {
                        return getIP();
                    }
                }
            }
        }*/

        private string getIP(Exception e)
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

            //for testing
            correctIP = true;
            ///

            if(correctIP == true)
            {
                return "FAILED: " + e.ToString();
            } 
            else
            {
                return "FAILED: IP should be 192.168.17.10";
            }
       

        }

    }
}
