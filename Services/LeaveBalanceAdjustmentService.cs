using System.Security.Claims;
using HRManagement.DTOs;
using HRManagement.DTOs.Common;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class LeaveBalanceAdjustmentService : ILeaveBalanceAdjustmentService
    {
        private readonly HrmsDbContext _context;

        public LeaveBalanceAdjustmentService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResult<AdjustLeaveBalanceResponseDTO>> AdjustAsync(AdjustLeaveBalanceDTO dto, ClaimsPrincipal user)
        {
            if (dto.EmployeeId <= 0 || dto.LeaveTypeId <= 0 || string.IsNullOrWhiteSpace(dto.AdjustmentType) ||
                dto.NumberOfDays <= 0 || string.IsNullOrWhiteSpace(dto.Reason))
            {
                return ApiResult<AdjustLeaveBalanceResponseDTO>.Fail(
                    "MSG-48",
                    "Please fill in all required fields (leave type, adjustment amount, reason).");
            }

            if (dto.AdjustmentType != "Add" && dto.AdjustmentType != "Deduct")
            {
                return ApiResult<AdjustLeaveBalanceResponseDTO>.Fail(
                    "MSG-49",
                    "Invalid adjustment amount. Please enter a positive number.");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == dto.EmployeeId);

            if (employee == null)
            {
                return ApiResult<AdjustLeaveBalanceResponseDTO>.Fail("EMP-404", "Employee not found.");
            }

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(x => x.LeaveTypeId == dto.LeaveTypeId && x.IsActive);

            if (leaveType == null)
            {
                return ApiResult<AdjustLeaveBalanceResponseDTO>.Fail("LEAVE_TYPE-404", "Invalid leave type.");
            }

            int year = dto.EffectiveDate.Year;

            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == dto.EmployeeId &&
                    x.LeaveTypeId == dto.LeaveTypeId &&
                    x.Year == year);

            if (leaveBalance == null)
            {
                return ApiResult<AdjustLeaveBalanceResponseDTO>.Fail(
                    "MSG-46",
                    "Unable to retrieve leave balance information. Please contact HR.");
            }

            decimal oldUsedDays = leaveBalance.UsedDays;
            decimal oldRemaining = leaveBalance.RemainingDays ?? 0;

            decimal newUsedDays = oldUsedDays;
            if (dto.AdjustmentType == "Deduct")
            {
                newUsedDays += dto.NumberOfDays;
            }
            else
            {
                newUsedDays -= dto.NumberOfDays;
            }

            if (newUsedDays < 0)
            {
                return ApiResult<AdjustLeaveBalanceResponseDTO>.Fail(
                    "MSG-49",
                    "Invalid adjustment amount. Please enter a positive number.");
            }

            leaveBalance.UsedDays = newUsedDays;
            leaveBalance.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            int? userId = GetUserId(user);

            var auditLog = new AuditLog
            {
                TableName = "LeaveBalance",
                Action = "Update",
                RecordId = leaveBalance.BalanceId,
                UserId = userId,
                OldValues = $"UsedDays={oldUsedDays}, RemainingDays={oldRemaining}",
                NewValues = $"AdjustmentType={dto.AdjustmentType}, NumberOfDays={dto.NumberOfDays}, Reason={dto.Reason}, UsedDays={leaveBalance.UsedDays}, RemainingDays={leaveBalance.RemainingDays}",
                ActionDate = DateTime.UtcNow,
                Ipaddress = ""
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            var employeeUser = await _context.Users
                .FirstOrDefaultAsync(x => x.EmployeeId == dto.EmployeeId && x.IsActive);

            if (employeeUser != null)
            {
                var notification = new Notification
                {
                    RecipientUserId = employeeUser.UserId,
                    NotificationType = "LeaveBalanceAdjustment",
                    Title = "Leave Balance Updated",
                    Message = $"Your leave balance for {leaveType.LeaveTypeName} has been adjusted.",
                    RelatedEntity = "LeaveBalance",
                    RelatedEntityId = leaveBalance.BalanceId,
                    IsRead = false,
                    SentDate = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }

            var response = new AdjustLeaveBalanceResponseDTO
            {
                EmployeeId = dto.EmployeeId,
                LeaveTypeId = dto.LeaveTypeId,
                LeaveTypeName = leaveType.LeaveTypeName,
                OldUsedDays = oldUsedDays,
                OldRemainingBalance = oldRemaining,
                NewUsedDays = leaveBalance.UsedDays,
                NewRemainingBalance = leaveBalance.RemainingDays ?? 0,
                AdjustmentType = dto.AdjustmentType,
                NumberOfDays = dto.NumberOfDays,
                Reason = dto.Reason,
                EffectiveDate = dto.EffectiveDate,
                MessageCode = "MSG-47",
                Message = "Leave balance adjusted successfully."
            };

            return ApiResult<AdjustLeaveBalanceResponseDTO>.Ok(
                response,
                "MSG-47",
                "Leave balance adjusted successfully.");
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