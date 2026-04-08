namespace HRManagement.Services.Audits
{
    public interface IAuditService
    {
        Task TrackAsync(int? userId, string action, string description);
    }
}

