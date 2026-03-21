using System.Text.Json;
using HRManagement.DTOs.LeaveRequest;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly HrmsDbContext _context;

        public LeaveRequestService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult<LeaveRequestResponseDTO>> CreateLeaveRequestAsync(int userId, CreateLeaveRequestDTO dto)
        {
            // MSG-23 Required fields
            if (dto.LeaveTypeID <= 0 || dto.StartDate == default || dto.EndDate == default || dto.NumberOfDays <= 0)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-23", "Please fill in all required fields.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

            if (user == null)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-106", "Access Denied.");
            }

            if (user.EmployeeId == null)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-104", "Unable to retrieve user data due to database connection error.");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == user.EmployeeId);

            if (employee == null)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-104", "Unable to retrieve user data due to database connection error.");
            }

            if (employee.EmploymentStatus != "Active")
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-106", "Access Denied.");
            }

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(x => x.LeaveTypeId == dto.LeaveTypeID && x.IsActive);

            if (leaveType == null)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-08", "Invalid leave type.");
            }

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            // BR-21 MSG-24
            if (dto.EndDate < dto.StartDate)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-24", "End date must be after start date.");
            }

            // BR-21 MSG-26
            if (dto.StartDate < today || dto.EndDate < today)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-26", "Cannot request leave for past dates.");
            }

            // MSG-25 overlap leave
            bool hasOverlap = await _context.LeaveRequests.AnyAsync(x =>
                x.EmployeeId == employee.EmployeeId &&
                (x.Status == "Pending" || x.Status == "Approved") &&
                dto.StartDate <= x.EndDate &&
                dto.EndDate >= x.StartDate);

            if (hasOverlap)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-25", "Selected dates overlap with existing approved leave.");
            }

            int targetYear = dto.StartDate.Year;

            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employee.EmployeeId &&
                    x.LeaveTypeId == dto.LeaveTypeID &&
                    x.Year == targetYear);

            if (leaveBalance == null)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-46", "Unable to retrieve leave balance information.");
            }

            decimal currentBalance = leaveBalance.TotalEntitlement - leaveBalance.UsedDays;
            decimal remainingAfterRequest = currentBalance - dto.NumberOfDays;

            // BR-23 MSG-27
            if (currentBalance < dto.NumberOfDays && !dto.SubmitAnyway)
            {
                var warningData = new LeaveRequestResponseDTO
                {
                    EmployeeID = employee.EmployeeId,
                    LeaveTypeID = dto.LeaveTypeID,
                    LeaveTypeName = leaveType.LeaveTypeName,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    NumberOfDays = dto.NumberOfDays,
                    Reason = dto.Reason,
                    CurrentBalance = currentBalance,
                    RemainingAfterRequest = remainingAfterRequest,
                    MessageCode = "MSG-27",
                    Message = "Insufficient leave balance."
                };

                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-27", "Insufficient leave balance.", warningData);
            }

            // BR-27 MSG-28
            if (employee.ManagerId == null)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-28", "No approver configured for your leave requests.");
            }

            var managerEmployee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == employee.ManagerId);

            if (managerEmployee == null)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-28", "No approver configured for your leave requests.");
            }

            var approverUser = await _context.Users
                .FirstOrDefaultAsync(x => x.EmployeeId == managerEmployee.EmployeeId && x.IsActive);

            if (approverUser == null)
            {
                return ServiceResult<LeaveRequestResponseDTO>.Fail("MSG-28", "No approver configured for your leave requests.");
            }

            string requestNumber = await GenerateRequestNumberAsync();

            var leaveRequest = new LeaveRequest
            {
                RequestNumber = requestNumber,
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = dto.LeaveTypeID,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                NumberOfDays = dto.NumberOfDays,
                Reason = dto.Reason,
                Status = "Pending",
                SubmittedDate = DateTime.Now
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            var notification = new Notification
            {
                RecipientUserId = approverUser.UserId,
                NotificationType = "Leave",
                Title = "New Leave Request",
                Message = $"{employee.FullName} submitted a leave request from {dto.StartDate:dd/MM/yyyy} to {dto.EndDate:dd/MM/yyyy}.",
                RelatedEntity = "LeaveRequest",
                RelatedEntityId = leaveRequest.LeaveRequestId,
                IsRead = false,
                SentDate = DateTime.Now
            };

            _context.Notifications.Add(notification);

            var auditLog = new AuditLog
            {
                TableName = "LeaveRequests",
                Action = "INSERT",
                RecordId = leaveRequest.LeaveRequestId,
                UserId = userId,
                NewValues = JsonSerializer.Serialize(new
                {
                    leaveRequest.LeaveRequestId,
                    leaveRequest.RequestNumber,
                    leaveRequest.EmployeeId,
                    leaveRequest.LeaveTypeId,
                    leaveRequest.StartDate,
                    leaveRequest.EndDate,
                    leaveRequest.NumberOfDays,
                    leaveRequest.Reason,
                    leaveRequest.Status
                }),
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            var response = new LeaveRequestResponseDTO
            {
                LeaveRequestID = leaveRequest.LeaveRequestId,
                RequestNumber = leaveRequest.RequestNumber,
                EmployeeID = leaveRequest.EmployeeId,
                LeaveTypeID = leaveRequest.LeaveTypeId,
                LeaveTypeName = leaveType.LeaveTypeName,
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                NumberOfDays = leaveRequest.NumberOfDays,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status,
                SubmittedDate = leaveRequest.SubmittedDate,
                MessageCode = "MSG-22",
                Message = "Leave request submitted successfully.",
                CurrentBalance = currentBalance,
                RemainingAfterRequest = remainingAfterRequest
            };

            return ServiceResult<LeaveRequestResponseDTO>.Ok("MSG-22", "Leave request submitted successfully.", response);
        }

        private async Task<string> GenerateRequestNumberAsync()
        {
            var today = DateTime.Now;
            string prefix = $"LR-{today:yyyyMMdd}";

            int countToday = await _context.LeaveRequests
                .CountAsync(x => x.SubmittedDate.Date == today.Date);

            return $"{prefix}-{(countToday + 1):D3}";
        }
        public async Task<ServiceResult<string>> ApproveLeaveRequestAsync(int managerUserId, int leaveRequestId, ApproveLeaveRequestDTO dto)
        {
            var managerUser = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == managerUserId && x.IsActive);

            if (managerUser == null || managerUser.EmployeeId == null)
            {
                return ServiceResult<string>.Fail("MSG-41", "You do not have authority to approve this request.");
            }

            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId);

            if (leaveRequest == null)
            {
                return ServiceResult<string>.Fail("MSG-42", "This request has already been processed.");
            }

            if (leaveRequest.Status != "Pending")
            {
                return ServiceResult<string>.Fail("MSG-42", "This request has already been processed.");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == leaveRequest.EmployeeId);

            if (employee == null)
            {
                return ServiceResult<string>.Fail("MSG-42", "This request has already been processed.");
            }

            // BR-27: manager must be the approver of this employee
            if (employee.ManagerId != managerUser.EmployeeId)
            {
                return ServiceResult<string>.Fail("MSG-41", "You do not have authority to approve this request.");
            }

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(x => x.LeaveTypeId == leaveRequest.LeaveTypeId);

            if (leaveType == null)
            {
                return ServiceResult<string>.Fail("MSG-42", "This request has already been processed.");
            }

            int targetYear = leaveRequest.StartDate.Year;

            var leaveBalance = await _context.LeaveBalances
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == leaveRequest.EmployeeId &&
                    x.LeaveTypeId == leaveRequest.LeaveTypeId &&
                    x.Year == targetYear);

            if (leaveBalance == null)
            {
                return ServiceResult<string>.Fail("MSG-46", "Unable to retrieve leave balance information.");
            }

            decimal currentBalance = leaveBalance.TotalEntitlement - leaveBalance.UsedDays;
            decimal newBalance = currentBalance - leaveRequest.NumberOfDays;

            // MSG-43
            if (newBalance < 0)
            {
                return ServiceResult<string>.Fail("MSG-43", "Warning: Approving this leave request will result in negative leave balance for the employee.");
            }

            leaveRequest.Status = "Approved";
            leaveRequest.ApprovedDate = DateTime.Now;
            leaveRequest.ApprovedBy = managerUserId;
            leaveRequest.ReviewedDate = DateTime.Now;
            leaveRequest.ReviewedBy = managerUserId;
            leaveRequest.ReviewerComments = dto.Comments;

            leaveBalance.UsedDays += leaveRequest.NumberOfDays;
            leaveBalance.LastUpdated = DateTime.Now;

            var employeeUser = await _context.Users
                .FirstOrDefaultAsync(x => x.EmployeeId == leaveRequest.EmployeeId && x.IsActive);

            if (employeeUser != null)
            {
                var notification = new Notification
                {
                    RecipientUserId = employeeUser.UserId,
                    NotificationType = "Leave",
                    Title = "Leave Request Approved",
                    Message = $"Your leave request {leaveRequest.RequestNumber} has been approved.",
                    RelatedEntity = "LeaveRequest",
                    RelatedEntityId = leaveRequest.LeaveRequestId,
                    IsRead = false,
                    SentDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
            }

            var auditLog = new AuditLog
            {
                TableName = "LeaveRequests",
                Action = "UPDATE",
                RecordId = leaveRequest.LeaveRequestId,
                UserId = managerUserId,
                NewValues = JsonSerializer.Serialize(new
                {
                    leaveRequest.LeaveRequestId,
                    leaveRequest.Status,
                    leaveRequest.ApprovedDate,
                    leaveRequest.ApprovedBy,
                    leaveRequest.ReviewerComments,
                    UpdatedLeaveBalanceUsedDays = leaveBalance.UsedDays
                }),
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("MSG-40", "Request approved successfully.", null);
        }
        public async Task<ServiceResult<string>> RejectLeaveRequestAsync(int managerUserId, int leaveRequestId, RejectLeaveRequestDTO dto)
        {
            var managerUser = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == managerUserId && x.IsActive);

            if (managerUser == null || managerUser.EmployeeId == null)
            {
                return ServiceResult<string>.Fail("MSG-41", "You do not have authority to approve this request.");
            }

            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId);

            if (leaveRequest == null)
            {
                return ServiceResult<string>.Fail("MSG-42", "This request has already been processed.");
            }

            if (leaveRequest.Status != "Pending")
            {
                return ServiceResult<string>.Fail("MSG-42", "This request has already been processed.");
            }

            if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            {
                return ServiceResult<string>.Fail("MSG-45", "Please provide a reason for rejection.");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.EmployeeId == leaveRequest.EmployeeId);

            if (employee == null)
            {
                return ServiceResult<string>.Fail("MSG-42", "This request has already been processed.");
            }

            // BR-27
            if (employee.ManagerId != managerUser.EmployeeId)
            {
                return ServiceResult<string>.Fail("MSG-41", "You do not have authority to approve this request.");
            }

            leaveRequest.Status = "Rejected";
            leaveRequest.RejectionReason = dto.RejectionReason;
            leaveRequest.ReviewedDate = DateTime.Now;
            leaveRequest.ReviewedBy = managerUserId;
            leaveRequest.ReviewerComments = dto.RejectionReason;

            var employeeUser = await _context.Users
                .FirstOrDefaultAsync(x => x.EmployeeId == leaveRequest.EmployeeId && x.IsActive);

            if (employeeUser != null)
            {
                var notification = new Notification
                {
                    RecipientUserId = employeeUser.UserId,
                    NotificationType = "Leave",
                    Title = "Leave Request Rejected",
                    Message = $"Your leave request {leaveRequest.RequestNumber} has been rejected.",
                    RelatedEntity = "LeaveRequest",
                    RelatedEntityId = leaveRequest.LeaveRequestId,
                    IsRead = false,
                    SentDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
            }

            var auditLog = new AuditLog
            {
                TableName = "LeaveRequests",
                Action = "UPDATE",
                RecordId = leaveRequest.LeaveRequestId,
                UserId = managerUserId,
                NewValues = JsonSerializer.Serialize(new
                {
                    leaveRequest.LeaveRequestId,
                    leaveRequest.Status,
                    leaveRequest.RejectionReason,
                    leaveRequest.ReviewedDate,
                    leaveRequest.ReviewedBy
                }),
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("MSG-44", "Request rejected successfully.", null);
        }
        public async Task<ServiceResult<string>> CancelLeaveRequestAsync(int userId, int leaveRequestId, CancelLeaveRequestDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

            if (user == null || user.EmployeeId == null)
            {
                return ServiceResult<string>.Fail("MSG-106", "Access Denied.");
            }

            var leaveRequest = await _context.LeaveRequests
                .FirstOrDefaultAsync(x => x.LeaveRequestId == leaveRequestId);

            if (leaveRequest == null)
            {
                return ServiceResult<string>.Fail("MSG-38", "Cannot cancel this request. It has already been approved/rejected/cancelled.");
            }

            if (leaveRequest.EmployeeId != user.EmployeeId)
            {
                return ServiceResult<string>.Fail("MSG-106", "Access Denied.");
            }

            if (leaveRequest.Status != "Pending")
            {
                return ServiceResult<string>.Fail("MSG-38", "Cannot cancel this request. It has already been approved/rejected/cancelled.");
            }

            leaveRequest.Status = "Cancelled";
            leaveRequest.ReviewerComments = dto.Reason;
            leaveRequest.ReviewedDate = DateTime.Now;
            leaveRequest.ReviewedBy = userId;

            var auditLog = new AuditLog
            {
                TableName = "LeaveRequests",
                Action = "UPDATE",
                RecordId = leaveRequest.LeaveRequestId,
                UserId = userId,
                NewValues = JsonSerializer.Serialize(new
                {
                    leaveRequest.LeaveRequestId,
                    leaveRequest.Status,
                    CancelReason = dto.Reason
                }),
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            return ServiceResult<string>.Ok("MSG-37", "Request cancelled successfully.", null);
        }
        public async Task<ServiceResult<List<MyLeaveRequestItemDTO>>> GetMyLeaveRequestsAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

            if (user == null || user.EmployeeId == null)
            {
                return ServiceResult<List<MyLeaveRequestItemDTO>>.Fail("MSG-106", "Access Denied.");
            }

            var requests = await _context.LeaveRequests
                .Where(x => x.EmployeeId == user.EmployeeId)
                .Join(
                    _context.LeaveTypes,
                    request => request.LeaveTypeId,
                    leaveType => leaveType.LeaveTypeId,
                    (request, leaveType) => new MyLeaveRequestItemDTO
                    {
                        LeaveRequestID = request.LeaveRequestId,
                        RequestNumber = request.RequestNumber,
                        LeaveTypeID = request.LeaveTypeId,
                        LeaveTypeName = leaveType.LeaveTypeName,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        NumberOfDays = request.NumberOfDays,
                        Reason = request.Reason,
                        Status = request.Status,
                        SubmittedDate = request.SubmittedDate
                    }
                )
                .OrderByDescending(x => x.SubmittedDate)
                .ToListAsync();

            return ServiceResult<List<MyLeaveRequestItemDTO>>.Ok("", "Success", requests);
        }
    }
}