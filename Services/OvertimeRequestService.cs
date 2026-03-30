using HRManagement.DTOs.LeaveRequest;
using HRManagement.DTOs.OvertimeRequest;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HRManagement.Services
{
    public class OvertimeRequestService : IOvertimeRequestService
    {
        private readonly HrmsDbContext _context;

        public OvertimeRequestService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<string>> CreateOvertimeRequestAsync(int userId, CreateOvertimeRequestDTO dto)
        {
            if (dto.OvertimeDate == default || dto.StartTime == default || dto.EndTime == default)
            {
                return ServiceResult<string>.Fail("MSG-30", "Required fields are missing.");
            }

            if (dto.EndTime <= dto.StartTime)
            {
                return ServiceResult<string>.Fail("MSG-31", "Start time must be before end time.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

            if (user == null || user.EmployeeId == null)
            {
                return ServiceResult<string>.Fail("MSG-106", "Access Denied.");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == user.EmployeeId);

            if (employee == null)
            {
                return ServiceResult<string>.Fail("MSG-104", "Employee not found.");
            }

            // Calculate hours
            var start = dto.StartTime.ToTimeSpan();
            var end = dto.EndTime.ToTimeSpan();
            var totalHours = (decimal)(end - start).TotalHours;

            if (totalHours <= 0)
            {
                return ServiceResult<string>.Fail("MSG-31", "Invalid overtime hours.");
            }

            if (totalHours > 12)
            {
                return ServiceResult<string>.Fail("MSG-33", "Overtime hours exceed daily limit.");
            }

            string requestNumber = $"OT-{DateTime.Now:yyyyMMddHHmmss}";

            var overtimeRequest = new OvertimeRequest
            {
                RequestNumber = requestNumber,
                EmployeeId = employee.EmployeeId,
                OvertimeDate = dto.OvertimeDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                TotalHours = totalHours,
                Reason = dto.Reason,
                TaskDescription = dto.TaskDescription,
                Status = "Pending",
                SubmittedDate = DateTime.Now
            };

            _context.OvertimeRequests.Add(overtimeRequest);

            var auditLog = new AuditLog
            {
                TableName = "OvertimeRequests",
                Action = "INSERT",
                RecordId = overtimeRequest.OvertimeRequestId,
                UserId = userId,
                NewValues = JsonSerializer.Serialize(new
                {
                    overtimeRequest.OvertimeRequestId,
                    overtimeRequest.EmployeeId,
                    overtimeRequest.OvertimeDate,
                    overtimeRequest.StartTime,
                    overtimeRequest.EndTime,
                    overtimeRequest.TotalHours,
                    overtimeRequest.Status
                }),
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("MSG-29", "Overtime request submitted successfully.", null);
        }
        public async Task<ServiceResult<string>> ApproveOvertimeRequestAsync(int managerUserId, int requestId, ApproveOvertimeRequestDTO dto)
        {
            var manager = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == managerUserId && x.IsActive);

            if (manager == null || manager.EmployeeId == null)
                return ServiceResult<string>.Fail("MSG-41", "Manager has no approval authority.");

            var request = await _context.OvertimeRequests
                .FirstOrDefaultAsync(x => x.OvertimeRequestId == requestId);

            if (request == null || request.Status != "Pending")
                return ServiceResult<string>.Fail("MSG-42", "Request already processed.");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId);

            if (employee == null || employee.ManagerId != manager.EmployeeId)
                return ServiceResult<string>.Fail("MSG-41", "Manager has no approval authority.");

            request.Status = "Approved";
            request.ReviewedDate = DateTime.Now;
            request.ReviewedBy = managerUserId;
            request.ApprovedDate = DateTime.Now;
            request.ApprovedBy = managerUserId;

            var auditLog = new AuditLog
            {
                TableName = "OvertimeRequests",
                Action = "UPDATE",
                RecordId = request.OvertimeRequestId,
                UserId = managerUserId,
                NewValues = System.Text.Json.JsonSerializer.Serialize(new
                {
                    request.Status,
                    request.ReviewedDate,
                    request.ReviewedBy,
                    request.ApprovedDate,
                    request.ApprovedBy
                }),
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("MSG-40", "Overtime request approved.", null);
        }
        public async Task<ServiceResult<string>> RejectOvertimeRequestAsync(int managerUserId, int requestId, RejectOvertimeRequestDTO dto)
        {
            var manager = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == managerUserId && x.IsActive);

            if (manager == null || manager.EmployeeId == null)
                return ServiceResult<string>.Fail("MSG-41", "Manager has no approval authority.");

            var request = await _context.OvertimeRequests
                .FirstOrDefaultAsync(x => x.OvertimeRequestId == requestId);

            if (request == null || request.Status != "Pending")
                return ServiceResult<string>.Fail("MSG-42", "Request already processed.");

            request.Status = "Rejected";
            request.ReviewedDate = DateTime.Now;
            request.ReviewedBy = managerUserId;
            request.RejectionReason = dto.Reason;

            var auditLog = new AuditLog
            {
                TableName = "OvertimeRequests",
                Action = "UPDATE",
                RecordId = request.OvertimeRequestId,
                UserId = managerUserId,
                NewValues = System.Text.Json.JsonSerializer.Serialize(new
                {
                    request.Status,
                    request.RejectionReason
                }),
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("MSG-44", "Overtime request rejected.", null);
        }
        // cancel request
        public async Task<ServiceResult<string>> CancelOvertimeRequestAsync(int userId, int requestId, CancelOvertimeRequestDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

            if (user == null || user.EmployeeId == null)
                return ServiceResult<string>.Fail("MSG-106", "Access denied.");

            var request = await _context.OvertimeRequests
                .FirstOrDefaultAsync(x => x.OvertimeRequestId == requestId);

            if (request == null)
                return ServiceResult<string>.Fail("MSG-104", "Request not found.");

            if (request.EmployeeId != user.EmployeeId)
                return ServiceResult<string>.Fail("MSG-106", "Access denied.");

            if (request.Status != "Pending")
                return ServiceResult<string>.Fail("MSG-38", "Cannot cancel processed request.");

            request.Status = "Cancelled";
            request.RejectionReason = dto.Reason;

            var auditLog = new AuditLog
            {
                TableName = "OvertimeRequests",
                Action = "UPDATE",
                RecordId = request.OvertimeRequestId,
                UserId = userId,
                NewValues = System.Text.Json.JsonSerializer.Serialize(new
                {
                    request.Status,
                    request.RejectionReason
                }),
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("MSG-37", "Overtime request cancelled successfully.", null);
        }
        // get my request
        public async Task<ServiceResult<List<MyOvertimeRequestDTO>>> GetMyOvertimeRequestsAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

            if (user == null || user.EmployeeId == null)
                return ServiceResult<List<MyOvertimeRequestDTO>>.Fail("MSG-106", "Access denied.");

            var requests = await _context.OvertimeRequests
                .Where(x => x.EmployeeId == user.EmployeeId)
                .OrderByDescending(x => x.SubmittedDate)
                .Select(x => new MyOvertimeRequestDTO
                {
                    OvertimeRequestId = x.OvertimeRequestId,
                    RequestNumber = x.RequestNumber,
                    OvertimeDate = x.OvertimeDate,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    TotalHours = x.TotalHours,
                    Reason = x.Reason,
                    Status = x.Status,
                    SubmittedDate = x.SubmittedDate
                })
                .ToListAsync();

            return ServiceResult<List<MyOvertimeRequestDTO>>.Ok("", "Success", requests);
        }
        public async Task<ServiceResult<List<PendingOvertimeRequestDTO>>> GetPendingOvertimeRequestsAsync(int managerUserId)
        {
            var managerUser = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == managerUserId && x.IsActive);

            if (managerUser == null || managerUser.EmployeeId == null)
            {
                return ServiceResult<List<PendingOvertimeRequestDTO>>.Fail("MSG-106", "Access Denied.");
            }

            var pendingRequests = await _context.OvertimeRequests
                .Where(or => or.Status == "Pending")
                .Join(
                    _context.Employees,
                    or => or.EmployeeId,
                    e => e.EmployeeId,
                    (or, e) => new { or, e }
                )
                .Where(x => x.e.ManagerId == managerUser.EmployeeId)
                .Select(x => new PendingOvertimeRequestDTO
                {
                    OvertimeRequestId = x.or.OvertimeRequestId,
                    RequestNumber = x.or.RequestNumber,
                    EmployeeId = x.e.EmployeeId,
                    EmployeeName = x.e.FullName,
                    OvertimeDate = x.or.OvertimeDate,
                    StartTime = x.or.StartTime,
                    EndTime = x.or.EndTime,
                    TotalHours = x.or.TotalHours,
                    Reason = x.or.Reason,
                    TaskDescription = x.or.TaskDescription,
                    Status = x.or.Status,
                    SubmittedDate = x.or.SubmittedDate
                })
                .OrderByDescending(x => x.SubmittedDate)
                .ToListAsync();

            return ServiceResult<List<PendingOvertimeRequestDTO>>.Ok("", "Success", pendingRequests);
        }
    }
}