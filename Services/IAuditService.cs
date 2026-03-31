namespace HRManagement.Services
{
    public interface IAuditService
    {
        Task TrackAsync(int? userId, string action, string description);
    }
}
