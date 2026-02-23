using HRManagement.Models;
using HRManagement.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services;

public class AttendanceService : IAttendanceService
{
    private readonly HrmsDbContext _context;

    public AttendanceService(HrmsDbContext context)
    {
        _context = context;
    }

    public async Task<string> AssignShiftAsync(CreateShiftAssignmentDTO dto)
    {
        var overlappingShift = await _context.ShiftAssignments
            .AnyAsync(s => s.EmployeeId == dto.EmployeeId &&
                           s.Status == "Active" &&
                           ((dto.EndDate == null || s.StartDate <= dto.EndDate) &&
                            (s.EndDate == null || s.EndDate >= dto.StartDate)));

        if (overlappingShift)
        {
            return "MSG-ATT-01"; 
        }

        var newAssignment = new ShiftAssignment
        {
            EmployeeId = dto.EmployeeId,
            ShiftId = dto.ShiftId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AssignmentDate = DateOnly.FromDateTime(DateTime.Now),
            Status = "Active",
            CreatedDate = DateTime.Now
        };

        _context.ShiftAssignments.Add(newAssignment);
        await _context.SaveChangesAsync();

        return "MSG-SUC-01";
    }

    public async Task<AttendanceResponseDTO> CheckInAsync(CheckInRequestDTO dto)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var now = DateTime.Now;

        var assignment = await _context.ShiftAssignments
            .Include(s => s.Shift)
            .FirstOrDefaultAsync(s => s.EmployeeId == dto.EmployeeId &&
                                      s.Status == "Active" &&
                                      today >= s.StartDate &&
                                      (s.EndDate == null || today <= s.EndDate));

        if (assignment == null)
        {
            return new AttendanceResponseDTO { Message = "MSG-VAL-04", Status = "Error" };
        }

        var existingRecord = await _context.AttendanceRecords
            .AnyAsync(a => a.EmployeeId == dto.EmployeeId && a.AttendanceDate == today);

        if (existingRecord)
        {
            return new AttendanceResponseDTO { Message = "MSG-VAL-03", Status = "Error" };
        }

        int lateMinutes = 0;
        string finalStatus = "Present";

        var scheduledStart = today.ToDateTime(assignment.Shift.StartTime);
        var diff = now - scheduledStart;

        if (diff.TotalMinutes > 5) 
        {
            lateMinutes = (int)diff.TotalMinutes;
            finalStatus = "Late";
        }

        var record = new AttendanceRecord
        {
            EmployeeId = dto.EmployeeId,
            AttendanceDate = today,
            ShiftId = assignment.ShiftId,
            CheckInTime = now,
            Status = finalStatus,
            LateMinutes = lateMinutes,
            Location = dto.Location,
            Remarks = dto.Remarks,
            CreatedDate = DateTime.Now
        };

        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync();

        return new AttendanceResponseDTO
        {
            AttendanceId = record.AttendanceId,
            AttendanceDate = today,
            CheckInTime = now,
            Status = finalStatus,
            LateMinutes = lateMinutes,
            Message = "MSG-SUC-04"
        };
    }

    public async Task<AttendanceResponseDTO> CheckOutAsync(int employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var record = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == today);

        if (record == null || record.CheckInTime == null)
        {
            return new AttendanceResponseDTO { Message = "MSG-VAL-04", Status = "Error" };
        }

        record.CheckOutTime = DateTime.Now;

        var duration = record.CheckOutTime.Value - record.CheckInTime.Value;
        record.WorkingHours = (decimal)duration.TotalHours;

        await _context.SaveChangesAsync();

        return new AttendanceResponseDTO { Message = "MSG-SUC-04", Status = record.Status };
    }

    public async Task<List<AttendanceHistoryDTO>> GetHistoryAsync(int employeeId)
    {
        return await _context.AttendanceRecords
            .Include(a => a.Shift)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.AttendanceDate)
            .Select(a => new AttendanceHistoryDTO
            {
                Date = a.AttendanceDate,
                ShiftName = a.Shift != null ? a.Shift.ShiftName : "No Shift",
                CheckIn = a.CheckInTime,
                CheckOut = a.CheckOutTime,
                TotalHours = a.WorkingHours,
                Status = a.Status
            }).ToListAsync();
    }
}