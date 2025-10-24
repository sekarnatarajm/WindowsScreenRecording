using System.Net;
using System.Net.Mail;

namespace LBScreenRecording.Services
{
    public class EmailService
    {
        public void SendEmail(string subject, string body)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(AppConstant.FromEmail);
            mail.To.Add(AppConstant.ToEmail);
            mail.CC.Add(AppConstant.CcEmail);
            mail.Subject = subject; // "Lilbrahmas Screen Recording Error";
            mail.Body = body; // "This is a test email sent from C# Windows application.";

            var smtpClient = GetSmtpClient();
            smtpClient.Send(mail);
        }

        public SmtpClient GetSmtpClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12; // Or just Tls12 for broader compatibility
            SmtpClient smtpClient = new SmtpClient("smtp.elasticemail.com") // Or your SMTP server
            {
                Port = 2525,
                Credentials = new NetworkCredential(AppConstant.UserName, AppConstant.Password), // Use App Password for Gmail
                EnableSsl = true,   
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 100000 // Increase timeout if needed (e.g., 20 seconds)
            };
            return smtpClient;
        }
    }
}
