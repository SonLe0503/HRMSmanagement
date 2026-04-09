using HRManagement.DTOs.LeaveRequest;
using HRManagement.DTOs.OvertimeRequest;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using HRManagement.Services.Approvals;
using System.Text.Json;

namespace HRManagement.Services.Overtimes
{
    public class OvertimeRequestService : IOvertimeRequestService
    {
        private readonly HrmsDbContext _context;
        private readonly IApprovalRouteService _approvalRouteService;
        private readonly ITopLevelResolver _topLevelResolver;

        public OvertimeRequestService(
            HrmsDbContext context, 
            IApprovalRouteService approvalRouteService,
            ITopLevelResolver topLevelResolver)
        {
            _context = context;
            _approvalRouteService = approvalRouteService;
            _topLevelResolver = topLevelResolver;
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
            var shiftAssignment = await _context.ShiftAssignments
                .Include(x => x.Shift)
                .FirstOrDefaultAsync(x => 
                x.EmployeeId == employee.EmployeeId && 
                x.AssignmentDate == dto.OvertimeDate &&
                x.Status == "Active");

            if (shiftAssignment != null)
            {
                var baseDate = dto.OvertimeDate.ToDateTime(TimeOnly.MinValue);

                var shift = shiftAssignment.Shift;

                DateTime shiftStartDt = baseDate.Add(shift.StartTime.ToTimeSpan());
                DateTime shiftEndDt = baseDate.Add(shift.EndTime.ToTimeSpan());

                DateTime otStartDt = baseDate.Add(dto.StartTime.ToTimeSpan());
                DateTime otEndDt = baseDate.Add(dto.EndTime.ToTimeSpan());

                if (shift.IsOvernight == true)
                {
                    shiftEndDt = shiftEndDt.AddDays(1);

                    if (otEndDt <= otStartDt)
                    {
                        otEndDt = otEndDt.AddDays(1);
                    }
                }

                bool isOverlap = otStartDt < shiftEndDt && otEndDt > shiftStartDt;

                if (isOverlap)
                {
                    return ServiceResult<string>.Fail(
                        "MSG-OT-01",
                        "Overtime must be outside working hours."
                    );
                }
            }

            var authResult = await _approvalRouteService.CanSubmitRequestAsync(employee.EmployeeId);
            if (!authResult.IsAuthorized)
            {
                return ServiceResult<string>.Fail("MSG-41", authResult.Message ?? "Invalid approval route.");
            }

            var approverId = await _approvalRouteService.GetApproverIdAsync(employee.EmployeeId);
            string initialStatus = approverId.HasValue ? "Pending" : "Approved";

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
                Status = initialStatus,
                SubmittedDate = DateTime.Now,
                ReviewedDate = initialStatus == "Approved" ? DateTime.Now : null,
                ReviewedBy = null,
                ApprovedDate = initialStatus == "Approved" ? DateTime.Now : null,
                ApprovedBy = null // System
            };

            _context.OvertimeRequests.Add(overtimeRequest);

            if (approverId.HasValue)
            {
                var notification = new Notification
                {
                    RecipientUserId = approverId.Value,
                    NotificationType = "Overtime",
                    Title = "New Overtime Request",
                    Message = $"A new overtime request {requestNumber} is waiting for approval.",
                    RelatedEntity = "OvertimeRequest",
                    RelatedEntityId = overtimeRequest.OvertimeRequestId,
                    IsRead = false,
                    SentDate = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

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

                if (manager == null)
                    return ServiceResult<string>.Fail("MSG-41", "Manager has no approval authority.");

                var request = await _context.OvertimeRequests
                    .FirstOrDefaultAsync(x => x.OvertimeRequestId == requestId);

                if (request == null || request.Status != "Pending")
                    return ServiceResult<string>.Fail("MSG-42", "Request already processed.");

                var employee = await _context.Employees
                    .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId);

                // Validation using ApprovalRouteService (Supports Direct Manager + Top-level Fallback)
                var expectedApproverId = await _approvalRouteService.GetApproverIdAsync(employee.EmployeeId);
                if (expectedApproverId == null || expectedApproverId != managerUserId)
                {
                    return ServiceResult<string>.Fail("MSG-41", "You do not have authority to process this request.");
                }

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
                NewValues = JsonSerializer.Serialize(new
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

                if (manager == null)
                    return ServiceResult<string>.Fail("MSG-41", "Manager has no approval authority.");

                var request = await _context.OvertimeRequests
                    .FirstOrDefaultAsync(x => x.OvertimeRequestId == requestId);

                if (request == null || request.Status != "Pending")
                    return ServiceResult<string>.Fail("MSG-42", "Request already processed.");

                var employee = await _context.Employees
                    .FirstOrDefaultAsync(x => x.EmployeeId == request.EmployeeId);

                // Validation using ApprovalRouteService (Supports Direct Manager + Top-level Fallback)
                var expectedApproverId = await _approvalRouteService.GetApproverIdAsync(employee.EmployeeId);
                if (expectedApproverId == null || expectedApproverId != managerUserId)
                {
                    return ServiceResult<string>.Fail("MSG-41", "You do not have authority to process this request.");
                }

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
                NewValues = JsonSerializer.Serialize(new
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
                NewValues = JsonSerializer.Serialize(new
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

            if (managerUser == null)
            {
                return ServiceResult<List<PendingOvertimeRequestDTO>>.Fail("MSG-106", "Access Denied.");
            }

            var fallbackUserId = await _topLevelResolver.GetTopLevelFallbackUserIdAsync();

            var pendingRequests = await _context.OvertimeRequests
                .Where(or => or.Status == "Pending")
                .Include(or => or.Employee)
                    .ThenInclude(e => e.Position)
                .Where(or =>
                    (managerUser.EmployeeId.HasValue && or.Employee.ManagerId == managerUser.EmployeeId) ||
                    (or.Employee.ManagerId == null && or.Employee.Position.IsTopLevel && fallbackUserId == managerUserId)
                )
                .Select(x => new PendingOvertimeRequestDTO
                {
                    OvertimeRequestId = x.OvertimeRequestId,
                    RequestNumber = x.RequestNumber,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = x.Employee.FullName,
                    OvertimeDate = x.OvertimeDate,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    TotalHours = x.TotalHours,
                    Reason = x.Reason,
                    TaskDescription = x.TaskDescription,
                    Status = x.Status,
                    SubmittedDate = x.SubmittedDate,
                    IsTopLevel = x.Employee.Position.IsTopLevel
                })
                .OrderByDescending(x => x.SubmittedDate)
                .ToListAsync();

            return ServiceResult<List<PendingOvertimeRequestDTO>>.Ok("", "Success", pendingRequests);
        }
    }
}