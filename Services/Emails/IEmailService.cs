namespace HRManagement.Services.Emails
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}
