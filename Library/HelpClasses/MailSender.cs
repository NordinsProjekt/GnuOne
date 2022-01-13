﻿using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace Library.HelpClasses
{
    public class MailSender
    {
        /// <summary>
        /// Krypterar och struktuerar upp för mailutskick
        /// </summary>
        /// <param name="email"></param>
        /// <param name="query"></param>
        /// <param name="subject"></param>
        /// 


        public static void SendObject(string jsonObject, string email, MySettings _settings, string subject)
        {
            var crypt = AesCryption.Encrypt(jsonObject, _settings.Secret);

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

        //public static void SendEmail(string email, string query, string? subject, MySettings mySettings)
        //{
        //    if (subject == null)
        //    {
        //        subject = "Posting"; //?
        //    }
        //    var sw = new StringBuilder();
        //    sw.Append(DateTime.Now.ToString() + "/()/");
        //    sw.Append(subject);

        //    string encrypt = AesCryption.Encrypt(query, mySettings.Secret);

        //    var mailAddress = mySettings.Email;
        //    var password = mySettings.Password;
        //    MimeMessage message = new MimeMessage();
        //    message.From.Add(new MailboxAddress(mySettings.userName, mailAddress));
        //    message.To.Add(MailboxAddress.Parse(email));
        //    message.Subject = sw.ToString();
        //    message.Body = new TextPart("plain")
        //    {
        //        Text = $"{encrypt}XYXY/(/(XYXY7"
        //    };
        //    SmtpClient client = new SmtpClient();
        //    try
        //    {
        //        client.CheckCertificateRevocation = false;
        //        client.Connect("smtp.gmail.com", 465, true);
        //        client.Authenticate(mailAddress, password);
        //        client.Send(message);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //    finally
        //    {
        //        client.Disconnect(true);
        //        client.Dispose();
        //    }
        //}

    }
}
