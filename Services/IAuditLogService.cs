namespace HRManagement.Services
{
    public interface IAuditLogService
    {
        System.Threading.Tasks.Task LogAsync(
        string tableName,
        string action,
        int recordId,
        int? userId,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null);
    }
}
