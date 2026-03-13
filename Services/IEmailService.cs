namespace HRManagement.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
        Task SendWithAttachmentAsync(
            string to,
            string subject,
            string body,
            byte[] fileBytes,
            string fileName,
            string contentType);
    }
}
