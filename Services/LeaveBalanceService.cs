using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class LeaveBalanceService : ILeaveBalanceService
    {
        private readonly HrmsDbContext _context;

        public LeaveBalanceService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<LeaveBalanceDTO>> GetLeaveBalanceAsync(int employeeId)
        {
            var balances = await _context.LeaveBalances
                .Include(lb => lb.LeaveType)
                .Where(lb => lb.EmployeeId == employeeId)
                .Select(lb => new LeaveBalanceDTO
                {
                    LeaveType = lb.LeaveType.LeaveTypeName,
                    TotalEntitlement = lb.TotalEntitlement,
                    UsedDays = lb.UsedDays,
                    RemainingDays = lb.RemainingDays ?? 0,
                    CarriedForward = lb.CarriedForward,
                    Year = lb.Year
                })
                .ToListAsync();

            return balances;
        }
        public async System.Threading.Tasks.Task AdjustLeaveBalanceAsync(AdjustLeaveBalanceDTO dto, int hrUserId)
        {
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == dto.EmployeeId &&
                    x.LeaveTypeId == dto.LeaveTypeId &&
                    x.Year == DateTime.Now.Year);

            if (balance == null)
                throw new Exception("Leave balance not found");

            if (dto.AdjustmentType == "Add")
                balance.RemainingDays += dto.Days;
            else
                balance.RemainingDays -= dto.Days;

            balance.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

}
