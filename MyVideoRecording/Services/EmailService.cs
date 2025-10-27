using System.Net;
using System.Net.Mail;

namespace MyVideoRecording.Services
{
    public class EmailService
    {
        public void SendEmail(string subject, string body)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(SmtpConfig.FromEmail);
            mail.To.Add(SmtpConfig.ToEmail);
            mail.CC.Add(SmtpConfig.CcEmail);
            mail.Subject = subject;
            mail.Body = body;

            var smtpClient = GetSmtpClient();
            smtpClient.Send(mail);
        }

        private SmtpClient GetSmtpClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            SmtpClient smtpClient = new SmtpClient("smtp.elasticemail.com")
            {
                Port = 2525,
                Credentials = new NetworkCredential(SmtpConfig.UserName, SmtpConfig.Password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 100000
            };
            return smtpClient;
        }
    }
}
