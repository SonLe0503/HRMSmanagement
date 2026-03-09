using System.Security.Claims;
using HRManagement.DTOs;
using HRManagement.DTOs.Common;
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

        public async Task<ApiResult<LeaveBalanceResponseDTO>> GetMyLeaveBalanceAsync(ClaimsPrincipal user)
        {
            var employeeId = await GetEmployeeIdFromUserAsync(user);
            if (!employeeId.HasValue)
            {
                return ApiResult<LeaveBalanceResponseDTO>.Fail("AUTH-01", "Invalid employee identity.");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId.Value);

            if (employee == null)
            {
                return ApiResult<LeaveBalanceResponseDTO>.Fail("AUTH-02", "Employee not found.");
            }

            var currentYear = DateTime.Now.Year;
            var today = DateOnly.FromDateTime(DateTime.Today);

            var balances = await _context.LeaveBalances
                .Include(x => x.LeaveType)
                .Where(x => x.EmployeeId == employeeId.Value && x.Year == currentYear)
                .ToListAsync();

            if (!balances.Any())
            {
                return ApiResult<LeaveBalanceResponseDTO>.Fail(
                    "MSG-46",
                    "Unable to retrieve leave balance information. Please contact HR.");
            }

            var pendingRequests = await _context.LeaveRequests
                .Include(x => x.LeaveType)
                .Where(x => x.EmployeeId == employeeId.Value && x.Status == "Pending")
                .ToListAsync();

            var approvedHistory = await _context.LeaveRequests
                .Include(x => x.LeaveType)
                .Where(x => x.EmployeeId == employeeId.Value && x.Status == "Approved")
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();

            var upcomingLeaves = approvedHistory
                .Where(x => x.StartDate >= today)
                .OrderBy(x => x.StartDate)
                .ToList();

            var balanceItems = balances.Select(b => new LeaveBalanceItemDTO
            {
                LeaveTypeId = b.LeaveTypeId,
                LeaveTypeCode = b.LeaveType.LeaveTypeCode,
                LeaveTypeName = b.LeaveType.LeaveTypeName,
                Year = b.Year,
                TotalEntitlement = b.TotalEntitlement,
                UsedDays = b.UsedDays,
                RemainingBalance = b.RemainingDays ?? 0,
                PendingDays = pendingRequests
                    .Where(r => r.LeaveTypeId == b.LeaveTypeId)
                    .Sum(r => r.NumberOfDays),
                CarriedForward = b.CarriedForward,
                ExpiryDate = GetCarryForwardExpiryDate(b)
            }).ToList();

            var historyItems = approvedHistory.Select(r => new LeaveHistoryItemDTO
            {
                LeaveRequestId = r.LeaveRequestId,
                RequestNumber = r.RequestNumber,
                LeaveTypeId = r.LeaveTypeId,
                LeaveTypeName = r.LeaveType.LeaveTypeName,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                NumberOfDays = r.NumberOfDays,
                Status = r.Status
            }).ToList();

            var upcomingItems = upcomingLeaves.Select(r => new UpcomingLeaveItemDTO
            {
                LeaveRequestId = r.LeaveRequestId,
                RequestNumber = r.RequestNumber,
                LeaveTypeName = r.LeaveType.LeaveTypeName,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                NumberOfDays = r.NumberOfDays
            }).ToList();

            var response = new LeaveBalanceResponseDTO
            {
                EmployeeId = employeeId.Value,
                Year = currentYear,
                Balances = balanceItems,
                LeaveHistory = historyItems,
                UpcomingLeaves = upcomingItems,
                MessageCode = null,
                Message = null
            };

            var auditLog = new AuditLog
            {
                TableName = "LeaveBalance",
                Action = "View",
                RecordId = employeeId.Value,
                UserId = GetUserId(user),
                OldValues = null,
                NewValues = $"Viewed leave balance for employee {employeeId.Value}",
                ActionDate = DateTime.UtcNow,
                Ipaddress = ""
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            return ApiResult<LeaveBalanceResponseDTO>.Ok(response);
        }

        private DateTime? GetCarryForwardExpiryDate(LeaveBalance balance)
        {
            if (balance.CarriedForward <= 0)
                return null;

            return new DateTime(balance.Year, 3, 31);
        }

        private async Task<int?> GetEmployeeIdFromUserAsync(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? user.FindFirst("nameid")?.Value
                              ?? user.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return null;

            var dbUser = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

            return dbUser?.EmployeeId;
        }

        private int? GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? user.FindFirst("nameid")?.Value
                              ?? user.FindFirst("sub")?.Value;

            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }
    }
}