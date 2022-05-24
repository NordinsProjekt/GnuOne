using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using GnuOne.Data;
using Library.HelpClasses;
using Welcome_Settings;
using System.Text.Json;
using Newtonsoft.Json;

namespace Library.HelpClasses
{
    public class MailSender
    {
        //Methods to send messages from mail.
        public static void SendObject(MyFriend myInfo, string email, MySettings _settings, string subject)
        {
            var jsonMyInfoInObject = JsonConvert.SerializeObject(myInfo);
            //var crypt2 = MegaCrypt.
            var crypt = AesCryption.Encrypt(jsonMyInfoInObject, _settings.Secret);

            SendEmail(_settings, email, subject, crypt);
        }

        public static void SendObject(string myInfo, string email, MySettings _settings, string subject)
        {
            var jsonMyInfoInObject = JsonConvert.SerializeObject(myInfo);
            var crypt = AesCryption.Encrypt(jsonMyInfoInObject, _settings.Secret);

            SendEmail(_settings, email, subject, crypt);
        }

        private static void SendEmail(MySettings mySettings, string emailTo, string subject, string cryptMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString() + "/()/");
            sb.Append(subject);

            var mailAddress = mySettings.Email;
            var password = mySettings.Password;
            password = AesCryption.Decrypt(password, mySettings.Secret);

            MimeMessage message = new MimeMessage();

            message.Subject = sb.ToString();
            message.Body = new TextPart("plain")
            {
                Text = $"{cryptMessage}XYXY/(/(XYXY7"
            };

            message.From.Add(new MailboxAddress(mySettings.userName, mailAddress));
            message.To.Add(MailboxAddress.Parse(emailTo));

            SmtpClient client = new SmtpClient();
            try
            {
                client.CheckCertificateRevocation = false;
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate(mailAddress, password);
                client.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }
       
    }
}
