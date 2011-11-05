using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TeboCam
{
    public class mail
    {

        public static ArrayList attachments = new ArrayList();
        public static List<int> emailTimeSent = new List<int>();

        public static void addAttachment(string file)
        {
            try
            {
                attachments.Add(file);
            }
            catch (Exception)
            {
                bubble.logAddLine("Error adding file to email: " + file);
            }
        }

        public static void clearAttachments()
        {
            try
            {
                attachments.Clear();
            }
            catch (Exception)
            {
                bubble.logAddLine("Error adding clearing attachments.");
            }
        }




        public static bool validEmail(string emailAddress)
        {

            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return regex.match(pattern, emailAddress);

        }

        public static void sendEmail(string by, string to, string subj, string body, string replyTo, bool hasAttachments,int curTime)
        {
            string emailUser = config.getProfile(bubble.profileInUse).emailUser;
            string emailPass = config.getProfile(bubble.profileInUse).emailPass;
            string smtpHost = config.getProfile(bubble.profileInUse).smtpHost;
            int smtpPort = config.getProfile(bubble.profileInUse).smtpPort;
            bool EnableSsl = config.getProfile(bubble.profileInUse).EnableSsl;


            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            string msgBody = string.Empty;
            System.Net.Mail.SmtpClient smtp = new SmtpClient();

            mail.From = new System.Net.Mail.MailAddress(by, config.getProfile(bubble.profileInUse).sentByName);

            string[] emails = to.Split(';');

            foreach (string email in emails)
            {

                mail.To.Add(email);

            }

            // mail.To.Add(to);

            mail.Subject = subj;
            mail.Body = body;
            if (!hasAttachments && bubble.emailTestOk != 9)
            {
                mail.Body += Environment.NewLine + "No images attached option selected.";
            }
            mail.IsBodyHtml = true;// This is to enable HTML in your email body
            mail.ReplyTo = new MailAddress(replyTo); // This is optional, it allows you to add Reply To email address.



            if (hasAttachments)
            {
                int tmpCnt = 0;

                foreach (string file in attachments)
                {
                    try
                    {
                        tmpCnt++;
                        bubble.logAddLine("Adding file to email... " + tmpCnt.ToString());
                        mail.Attachments.Add(new Attachment(file));
                    }

                    catch (System.Exception ex)
                    {
                        bubble.logAddLine("Error adding file to email... " + tmpCnt.ToString());
                    }

                }

            }



            smtp.Host = smtpHost;
            smtp.Port = smtpPort;
            smtp.EnableSsl = EnableSsl;
            smtp.Credentials = new System.Net.NetworkCredential(emailUser, emailPass);

            try
            {
                smtp.Send(mail);
                emailTimeSent.Add(curTime);
                bubble.emailTestOk = 1;
                bubble.logAddLine("Email sent.");
            }

            catch (System.Exception ex)
            {
                bubble.emailTestOk = 2;
                bubble.logAddLine("Error in sending email.");
            }
        }

    }


}

