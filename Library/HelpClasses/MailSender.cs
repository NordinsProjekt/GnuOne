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
        public static void SendObject(string myInfo, string email, MySettings _settings, string subject,string recieverPublicKey,string senderPrivKey)
        {
            MegaCrypt tempCrypt = new MegaCrypt(myInfo);
            tempCrypt.RSAEncryptIt(senderPrivKey,recieverPublicKey);
            var crypt = AesCryption.Encrypt(tempCrypt.body, _settings.Secret);
            string[] message = new string[] { tempCrypt.body,Convert.ToBase64String(tempCrypt.signature), Convert.ToBase64String(tempCrypt.aesKey) };
            SendEmail(_settings, email, subject, message);
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
            string crypt = AesCryption.Encrypt($"{cryptMessage}XYXY/(/(XYXY7", mySettings.Secret);

            message.Subject = sb.ToString();
            message.Body = new TextPart("plain")
            {
                Text = crypt
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

        private static void SendEmail(MySettings mySettings, string emailTo, string subject, string[] cryptMessage)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString() + "/()/");
            sb.Append(subject);

            var mailAddress = mySettings.Email;
            var password = mySettings.Password;
            password = AesCryption.Decrypt(password, mySettings.Secret);

            MimeMessage message = new MimeMessage();
            string crypt = AesCryption.Encrypt($"{cryptMessage[0]}XYXY/(/(XYXY7{cryptMessage[1]}XYXY/(/(XYXY7{cryptMessage[2]}", mySettings.Secret);
            message.Subject = sb.ToString();
            message.Body = new TextPart("plain")
            {
                Text = crypt
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
