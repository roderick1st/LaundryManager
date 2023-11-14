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
        public string SendEmail(List<string> eMailAddresses, string emSubject, string emBody, string emAttachment = "")
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

            bool attachmentFailed = false;


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

            if(emAttachment != "")
            {
                try
                {
                    message.Attachments.Add(new Attachment(emAttachment));
                } catch
                {
                    attachmentFailed = true;
                    //do nothing with the attachment
                }
            }
            
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
                if (attachmentFailed)
                {
                    return "Message sent but attachment failed";
                } else
                {
                    return "Message Sent";
                }
                
            }
            catch (Exception e)
            {
                return getIP(e);
            }
        }

     

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
