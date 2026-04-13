using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HRManagement.Services.Approvals
{
    public class TopLevelResolver : ITopLevelResolver
    {
        private readonly HrmsDbContext _context;

        public TopLevelResolver(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTopLevelEmployeeAsync(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null || employee.Position == null)
            {
                return false;
            }

            return employee.Position.IsTopLevel;
        }

        public async Task<int?> GetTopLevelFallbackUserIdAsync()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "Approval.TopLevelFallbackUserId");

            if (setting != null && int.TryParse(setting.SettingValue, out var userId))
            {
                return userId;
            }

            return null;
        }

        public async Task<int?> GetDefaultFallbackUserIdAsync()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "Approval.DefaultFallbackUserId");

            if (setting != null && int.TryParse(setting.SettingValue, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
