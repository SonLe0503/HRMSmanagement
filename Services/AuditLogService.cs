using HRManagement.Models;

namespace HRManagement.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly HrmsDbContext _context;

        public AuditLogService(HrmsDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task LogAsync(
            string tableName,
            string action,
            int recordId,
            int? userId,
            string? oldValues = null,
            string? newValues = null,
            string? ipAddress = null)
        {
            var log = new AuditLog
            {
                TableName = tableName,
                Action = action,
                RecordId = recordId,
                UserId = userId,
                OldValues = oldValues,
                NewValues = newValues,
                ActionDate = DateTime.UtcNow,
                Ipaddress = ipAddress
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
