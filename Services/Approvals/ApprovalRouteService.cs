using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HRManagement.Services.Approvals
{
    public class ApprovalRouteService : IApprovalRouteService
    {
        private readonly HrmsDbContext _context;
        private readonly ITopLevelResolver _topLevelResolver;

        public ApprovalRouteService(HrmsDbContext context, ITopLevelResolver topLevelResolver)
        {
            _context = context;
            _topLevelResolver = topLevelResolver;
        }

        public async Task<int?> GetApproverIdAsync(int employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId);

            if (employee == null) return null;

            // Rule 1: Employee has ManagerId
            if (employee.ManagerId.HasValue)
            {
                var managerUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmployeeId == employee.ManagerId.Value && u.IsActive);
                if (managerUser != null) return managerUser.UserId;
            }

            // Rule 2: Top-level Fallback
            var isTopLevel = await _topLevelResolver.IsTopLevelEmployeeAsync(employeeId);
            if (isTopLevel)
            {
                var topFallback = await _topLevelResolver.GetTopLevelFallbackUserIdAsync();
                if (topFallback.HasValue) return topFallback;
            }

            // Rule 3: Default Fallback (configured in system settings)
            var defaultFallback = await _topLevelResolver.GetDefaultFallbackUserIdAsync();
            if (defaultFallback.HasValue) return defaultFallback;

            // Rule 4: System Ultimate Fallback (Find first active Admin as last resort)
            var adminUser = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.IsActive && 
                    (u.Username == "admin" || u.UserRoles.Any(ur => ur.Role.RoleName == "ADMIN")));
            
            return adminUser?.UserId;
        }

        public async Task<(bool IsAuthorized, string? Message)> CanSubmitRequestAsync(int employeeId)
        {
            var approverId = await GetApproverIdAsync(employeeId);
            
            if (approverId.HasValue)
            {
                return (true, null);
            }

            return (false, "You do not have a valid approval route. This may be because you have no manager and no fallback approver is configured in system settings. Please contact HR/Admin.");
        }
    }
}
