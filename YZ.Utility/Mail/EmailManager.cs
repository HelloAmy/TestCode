using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace YZ.Utility
{
    public class EmailManager
    {
        private static EmailSettings _emailSettings;
        public static void InitEmailSettings(EmailSettings settings)
        {
            _emailSettings = settings;
        }

        public static EmailSettings GetSettings()
        {
            if (_emailSettings == null)
            {
                //使用默认配置
                _emailSettings = ConfigManager.GetXmlCachedConfig<EmailSettings>();
            }

            return _emailSettings;
        }

        public static SmtpClient CreateSmtpClient()
        {
            if (_emailSettings == null)
            {
                //使用默认配置
                _emailSettings = ConfigManager.GetXmlCachedConfig<EmailSettings>();
            }
            SmtpClient client = new SmtpClient(_emailSettings.Server, _emailSettings.Port ?? 25);
            client.EnableSsl = _emailSettings.EnableSsl ?? false;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            return client;
        }

        public static void SendEmail(MailMessage mailMessage)
        {
            if (mailMessage == null)
            {
                throw new ArgumentNullException("mailMessage");
            }
            var client = CreateSmtpClient();
            client.Send(mailMessage);
        }

        public static void SendEmail(string from, string recipients, string subject, string body)
        {
            Guard.IsNotNullOrEmpty(from, "from");
            Guard.IsNotNullOrEmpty(recipients, "recipients");
            Guard.IsNotNullOrEmpty(subject, "subject");
            var client = CreateSmtpClient();
            client.Send(from, recipients, subject, body);
        }

        /// <summary>
        /// 获取邮件模版
        /// </summary>
        /// <param name="domain">模块名，对应文件夹名，比如SO,MKT等</param>
        /// <param name="templateName">模版文件名，带后缀</param>
        /// <returns></returns>
        public static EmailTemplate GetEmailTemplate(string domain, string templateName)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory + "Configuration\\EmailTemplate";
            string fileName = Path.Combine(baseDir, domain, templateName);
            if (!File.Exists(fileName))
            {
                throw new ArgumentException(string.Format(@"邮件模版{0}{1}不存在。", domain + @"\", templateName));
            }

            return SerializeHelper.LoadFromXml<EmailTemplate>(fileName);
        }

        /// <summary>
        /// 发送处理后的邮件模版
        /// </summary>
        /// <param name="template"></param>
        public static void SendEmailTemplate(EmailTemplate template, Action<EmailTemplate, Exception> onCallback = null)
        {
            var emailSettings = GetSettings();
            Exception sendException = null;
            try
            {
                Guard.IsNotNull(template, "template");
                Guard.IsNotNullOrEmpty(template.To, "template.To");
                Guard.IsNotNullOrEmpty(template.Subject, "template.Subject");
                Guard.IsNotNullOrEmpty(template.Body, "template.Body");
                //处理模版中的公共变量
                var appSettings = ConfigManager.GetXmlCachedConfig<ApplicationSettings>();
                template.Body = template.Body.Replace("#Sys_AppName#", appSettings.AppName)
                    .Replace("{#Sys_AppUrl#}", appSettings.AppUrl)
                    .Replace("{#Sys_LoginUrl#}", appSettings.AppUrl)
                    .Replace("{#Sys_AppLogoUrl#}", appSettings.AppLogoUrl)
                    .Replace("{#Sys_CompanyName#}", appSettings.CompanyName)
                    .Replace("{#Sys_CompanyAddress#}", appSettings.CompanyAddress)
                    .Replace("{#Sys_SupportTeam#}", appSettings.AppSupportTeam)
                    .Replace("{#Sys_SupportDate#}", DateTime.Now.ToString("yyyy-MM-dd"));
                if (string.IsNullOrWhiteSpace(template.From))
                {
                    template.From = emailSettings.EmailSender;
                }
                MailMessage msg = new MailMessage();
                msg.Priority = MailPriority.High;
                msg.From = new MailAddress(template.From);
                string toAddress = "";
                if (emailSettings.IsProduction)
                {
                    toAddress = template.To;
                }
                else
                {
                    toAddress = emailSettings.TestingReceiverEmail ?? "";
                }
                var entries = toAddress.Split(new char[] { ';', ',' });
                foreach (var item in entries)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        msg.To.Add(new MailAddress(item));
                    }
                }
                msg.Subject = template.Subject;
                msg.Body = template.Body;
                msg.IsBodyHtml = template.IsBodyHtml;
                if (!string.IsNullOrWhiteSpace(template.Cc))
                {
                    var ccArray = template.Cc.Split(new char[] { ';', ',' });
                    foreach (var c in ccArray)
                    {
                        msg.CC.Add(c);
                    }
                }

                SendEmail(msg);
            }
            catch (Exception ex)
            {
                //log exception
                Logger.WriteEventLog("发送邮件异常:" + ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                sendException = ex;
            }

            if (onCallback != null)
            {
                onCallback(template, sendException);
            }
        }
    }
}
