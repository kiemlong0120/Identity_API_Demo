using System.Net;
using System.Net.Mail;

namespace Identity_API_Demo.Services
{
    public class SendMailService : ISendMailService
    {
        #region Init

        public IConfiguration _configuration;
        #endregion

        #region Constructor 
        public SendMailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        public string SendMail(string toEmail, string fromEmail, string subject, string body)
        {
            try
            {
                #region Gửi mail bằng smtp gmail, (using System.Net;, using System.Net.Mail;)
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                string username = _configuration.GetValue<string>("GmailAppsecure:UserName");
                string password = _configuration.GetValue<string>("GmailAppsecure:Password");
                client.Credentials = new NetworkCredential()
                {
                    UserName = username,
                    Password = password
                };
                MailMessage confirmMail = new MailMessage();
                confirmMail.To.Add(toEmail);
                confirmMail.From = new MailAddress(fromEmail);
                confirmMail.IsBodyHtml = true;
                confirmMail.Subject = subject;
                confirmMail.Body = body;
                client.Send(confirmMail);
                #endregion
                return $"To: {toEmail},\n From: {fromEmail},\n Subject: {subject},\n Message: {body}";
            }
            catch (Exception)
            {
                return null;
            }
            

        }
    }
}
