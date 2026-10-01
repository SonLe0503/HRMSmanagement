using Hangfire;

namespace HRManagement.Services.Emails
{
    /// <summary>
    /// Đưa email vào hàng đợi Hangfire thay vì gửi ngay trong request.
    /// Email thật được gửi ở nền bởi <see cref="EmailService"/> và tự thử lại nếu SMTP lỗi.
    /// </summary>
    public class QueuedEmailService : IEmailService
    {
        private readonly IBackgroundJobClient _jobs;

        public QueuedEmailService(IBackgroundJobClient jobs)
        {
            _jobs = jobs;
        }

        public Task SendAsync(string to, string subject, string body)
        {
            _jobs.Enqueue<EmailService>(s => s.SendAsync(to, subject, body));
            return Task.CompletedTask;
        }

        public Task SendWithAttachmentAsync(
            string to,
            string subject,
            string body,
            byte[] fileBytes,
            string fileName,
            string contentType)
        {
            _jobs.Enqueue<EmailService>(s => s.SendWithAttachmentAsync(to, subject, body, fileBytes, fileName, contentType));
            return Task.CompletedTask;
        }
    }
}
