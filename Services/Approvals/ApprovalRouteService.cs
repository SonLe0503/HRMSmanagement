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
                return managerUser?.UserId;
            }

            // Rule 2: Employee has NO ManagerId
            var isTopLevel = await _topLevelResolver.IsTopLevelEmployeeAsync(employeeId);
            if (isTopLevel)
            {
                // Fallback from system settings
                return await _topLevelResolver.GetTopLevelFallbackUserIdAsync();
            }

            // Not top-level and no manager = no approver
            return null;
        }

        public async Task<(bool IsAuthorized, string? Message)> CanSubmitRequestAsync(int employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId);

            if (employee == null) return (false, "Employee profile not found.");

            if (employee.ManagerId.HasValue) return (true, null);

            var isTopLevel = await _topLevelResolver.IsTopLevelEmployeeAsync(employeeId);
            if (isTopLevel) return (true, null);

            return (false, "You do not have a valid approval route. Please contact HR/Admin to update your reporting line.");
        }
    }
}
