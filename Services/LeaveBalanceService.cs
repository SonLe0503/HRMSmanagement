using HRManagement.DTOs.LeaveBalance;
using HRManagement.DTOs.LeaveRequest;
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

        public async Task<ServiceResult<List<MyLeaveBalanceDTO>>> GetMyLeaveBalanceAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

            if (user == null || user.EmployeeId == null)
            {
                return ServiceResult<List<MyLeaveBalanceDTO>>
                    .Fail("MSG-106", "Access denied.");
            }

            var balances = await _context.LeaveBalances
                .Where(x => x.EmployeeId == user.EmployeeId)
                .Join(
                    _context.LeaveTypes,
                    lb => lb.LeaveTypeId,
                    lt => lt.LeaveTypeId,
                    (lb, lt) => new MyLeaveBalanceDTO
                    {
                        LeaveTypeId = lt.LeaveTypeId,
                        LeaveTypeName = lt.LeaveTypeName,
                        TotalEntitlement = lb.TotalEntitlement,
                        UsedDays = lb.UsedDays,
                        RemainingDays = lb.RemainingDays ?? 0,
                        CarriedForward = lb.CarriedForward,
                        Year = lb.Year
                    })
                .ToListAsync();

            return ServiceResult<List<MyLeaveBalanceDTO>>
                .Ok("", "Success", balances);
        }
        public async Task<ServiceResult<string>> AdjustLeaveBalanceAsync(int hrUserId, AdjustLeaveBalanceDTO dto)
        {
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == dto.EmployeeId &&
                    x.LeaveTypeId == dto.LeaveTypeId &&
                    x.Year == DateTime.Now.Year);

            if (balance == null)
                return ServiceResult<string>.Fail("MSG-104", "Leave balance not found.");

            if (dto.AdjustmentType == "Add")
            {
                balance.TotalEntitlement += dto.NumberOfDays;
                balance.RemainingDays += dto.NumberOfDays;
            }
            else if (dto.AdjustmentType == "Deduct")
            {
                balance.TotalEntitlement -= dto.NumberOfDays;
                balance.RemainingDays -= dto.NumberOfDays;
            }
            else
            {
                return ServiceResult<string>.Fail("MSG-49", "Invalid adjustment type.");
            }

            balance.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("MSG-47", "Leave balance updated successfully.", null);
        }
    }
}