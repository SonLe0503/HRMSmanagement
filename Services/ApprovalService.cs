using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly HrmsDbContext _context;

        public ApprovalService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<PendingApprovalDTO>> GetPendingRequestsAsync(int managerEmployeeId)
        {
            var teamEmployeeIds = await _context.Employees
                .Where(e => e.ManagerId == managerEmployeeId)
                .Select(e => e.EmployeeId)
                .ToListAsync();

            var leaveRequests = await _context.LeaveRequests
                .Where(r => r.Status == "Pending" && teamEmployeeIds.Contains(r.EmployeeId))
                .Select(r => new PendingApprovalDTO
                {
                    RequestId = r.LeaveRequestId,
                    EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                    RequestType = "Leave",
                    SubmissionDate = r.SubmittedDate,
                    StartDate = r.StartDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = r.EndDate.ToDateTime(TimeOnly.MinValue),
                    TotalUnits = r.NumberOfDays,
                    Reason = r.Reason,
                    IsUrgent = r.StartDate <= DateOnly.FromDateTime(DateTime.Now.AddDays(2))
                })
                .ToListAsync();

            var overtimeRequests = await _context.OvertimeRequests
                .Where(r => r.Status == "Pending" && teamEmployeeIds.Contains(r.EmployeeId))
                .Select(r => new PendingApprovalDTO
                {
                    RequestId = r.OvertimeRequestId,
                    EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                    RequestType = "Overtime",
                    SubmissionDate = r.SubmittedDate,
                    StartDate = r.OvertimeDate.ToDateTime(TimeOnly.MinValue),
                    EndDate = null,
                    TotalUnits = r.TotalHours,
                    Reason = r.Reason,
                    IsUrgent = r.OvertimeDate <= DateOnly.FromDateTime(DateTime.Now.AddDays(1))
                })
                .ToListAsync();

            return leaveRequests
                .Concat(overtimeRequests)
                .OrderByDescending(r => r.SubmissionDate)
                .ToList();
        }
    }
}
