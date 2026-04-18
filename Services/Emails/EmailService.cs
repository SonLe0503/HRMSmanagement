using System.Net;
using System.Net.Mail;
using System.Text;

namespace HRManagement.Services.Emails
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var smtpHost = _configuration["Email:Smtp"];
            var smtpPort = int.Parse(_configuration["Email:Port"]);
            var smtpUser = _configuration["Email:Username"];
            var smtpPass = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:From"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }
        public async Task SendWithAttachmentAsync(
            string to,
            string subject,
            string body,
            byte[] fileBytes,
            string fileName,
            string contentType)
        {
            var smtpHost = _configuration["Email:Smtp"];
            var smtpPort = int.Parse(_configuration["Email:Port"]);
            var smtpUser = _configuration["Email:Username"];
            var smtpPass = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:From"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(to);

            var stream = new MemoryStream(fileBytes);
            var attachment = new Attachment(stream, fileName, contentType);

            mailMessage.Attachments.Add(attachment);

            await client.SendMailAsync(mailMessage);
        }
    }
}
