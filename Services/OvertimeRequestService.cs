using HRManagement.Common;
using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services
{
    public class OvertimeRequestService : IOvertimeRequestService
    {
        private readonly HrmsDbContext _context;

        public OvertimeRequestService(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<object>> CreateAsync(CreateOvertimeRequestDTO dto, int employeeId)
        {
            if (dto.OvertimeDate.Date < DateTime.Today)
                return ApiResponse<object>.FailResponse("Overtime date must be today or future");

            if (dto.StartTime >= dto.EndTime)
                return ApiResponse<object>.FailResponse("Start time must be before end time");

            var overtimeDateOnly = DateOnly.FromDateTime(dto.OvertimeDate);

            var exists = await _context.OvertimeRequests
                .AnyAsync(x => x.EmployeeId == employeeId
                            && x.OvertimeDate == overtimeDateOnly
                            && x.Status != "Rejected");

            if (exists)
                return ApiResponse<object>.FailResponse("You already have overtime request for this date");

            var overtime = new OvertimeRequest
            {
                EmployeeId = employeeId,
                OvertimeDate = overtimeDateOnly,
                StartTime = TimeOnly.FromTimeSpan(dto.StartTime),
                EndTime = TimeOnly.FromTimeSpan(dto.EndTime),
                Reason = dto.Reason,
                Status = "Pending"
            };

            _context.OvertimeRequests.Add(overtime);
            await _context.SaveChangesAsync();

            return ApiResponse<object>.SuccessResponse(new
            {
                overtime.OvertimeRequestId,
                overtime.Status
            }, "Overtime request created successfully", 201);
        }
        public async Task<ApiResponse<object>> CancelAsync(int requestId, int employeeId)
        {
            var request = await _context.OvertimeRequests
                .FirstOrDefaultAsync(x => x.OvertimeRequestId == requestId);

            if (request == null)
                return ApiResponse<object>.FailResponse("Request not found", 404);

            if (request.EmployeeId != employeeId)
                return ApiResponse<object>.FailResponse("Not allowed", 403);

            if (request.Status != "Pending")
                return ApiResponse<object>.FailResponse("Request cannot be cancelled");

            request.Status = "Cancelled";

            await _context.SaveChangesAsync();

            return ApiResponse<object>.SuccessResponse(null, "Request cancelled successfully");
        }

    }
}
