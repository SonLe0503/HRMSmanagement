using ClosedXML.Excel;
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
        // Simple Overlap Check
        bool isOverlapping = await _context.ShiftAssignments
            .AnyAsync(sa => sa.EmployeeId == dto.EmployeeId && sa.Status == "Active" &&
                ((dto.EndDate == null || sa.StartDate <= dto.EndDate) &&
                 (sa.EndDate == null || sa.EndDate >= dto.StartDate)));

        if (isOverlapping) return "MSG-ATT-01";

        var assignment = new ShiftAssignment
        {
            EmployeeId = dto.EmployeeId,
            ShiftId = dto.ShiftId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AssignmentDate = DateOnly.FromDateTime(DateTime.Now),
            Status = "Active",
            CreatedDate = DateTime.Now
        };

        _context.ShiftAssignments.Add(assignment);
        await _context.SaveChangesAsync();
        return "MSG-SUC-01";
    }

    public async Task<List<AdminAttendanceDTO>> GetAdminAttendanceAsync(DateOnly? date, int? deptId, string? status)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee).ThenInclude(e => e.Department)
            .Include(a => a.Shift)
            .AsQueryable();

        if (date.HasValue) query = query.Where(a => a.AttendanceDate == date.Value);
        if (deptId.HasValue) query = query.Where(a => a.Employee.DepartmentId == deptId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);

        return await query.Select(a => new AdminAttendanceDTO
        {
            AttendanceId = a.AttendanceId,
            EmployeeCode = a.Employee.EmployeeCode,
            FullName = a.Employee.FullName,
            DepartmentName = a.Employee.Department != null ? a.Employee.Department.DepartmentName : "N/A",
            Date = a.AttendanceDate.ToString("yyyy-MM-dd"),
            ShiftName = a.Shift != null ? a.Shift.ShiftName : "No Shift",
            CheckIn = a.CheckInTime.HasValue ? a.CheckInTime.Value.ToString("HH:mm:ss") : null,
            CheckOut = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("HH:mm:ss") : null,
            Status = a.Status,
            LateMinutes = a.LateMinutes
        }).ToListAsync();
    }

    public async Task<string> UpdateAssignmentAsync(int id, UpdateShiftAssignmentDTO dto)
    {
        var assignment = await _context.ShiftAssignments.FindAsync(id);
        if (assignment == null) return "MSG-SYS-03";

        int oldShiftId = assignment.ShiftId;

        assignment.ShiftId = dto.ShiftId;
        assignment.StartDate = dto.StartDate;
        assignment.EndDate = dto.EndDate;
        assignment.Status = dto.Status;

        var log = new AuditLog
        {
            TableName = "ShiftAssignments",
            Action = "UPDATE",
            RecordId = assignment.AssignmentId,
            OldValues = $"ShiftId: {oldShiftId}",
            NewValues = $"ShiftId: {dto.ShiftId}",
            ActionDate = DateTime.Now,
            UserId = 1
        };
        _context.AuditLogs.Add(log);

        await _context.SaveChangesAsync();
        return "MSG-SUC-02";
    }

    public async Task<AttendanceImportResultDto> ImportMachineDataAsync(IFormFile file)
    {
        var result = new AttendanceImportResultDto();
        using var workbook = new XLWorkbook(file.OpenReadStream());
        var worksheet = workbook.Worksheet(1);
        var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            result.TotalRows++;
            try
            {
                // Assume Excel Columns: A = MachineID, B = Date, C = CheckInTime, D = CheckOutTime
                var machineId = row.Cell(1).GetValue<int>();
                var date = DateOnly.FromDateTime(row.Cell(2).GetDateTime());
                var checkIn = row.Cell(3).GetDateTime();
                var checkOut = row.Cell(4).GetDateTime();

                // Find Employee by MachineID (need to add this column to Employee table)
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == machineId);
                if (employee == null)
                {
                    result.Errors.Add($"Row {result.TotalRows + 1}: Employee {machineId} not found.");
                    continue;
                }

                var assignment = await _context.ShiftAssignments.Include(s => s.Shift)
                    .FirstOrDefaultAsync(sa => sa.EmployeeId == employee.EmployeeId && sa.Status == "Active");

                int lateMinutes = 0;
                if (assignment != null)
                {
                    var scheduledStart = date.ToDateTime(assignment.Shift.StartTime);
                    var diff = checkIn - scheduledStart;
                    if (diff.TotalMinutes > 5) lateMinutes = (int)diff.TotalMinutes;
                }

                var existing = await _context.AttendanceRecords
                    .FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId && a.AttendanceDate == date);

                if (existing == null)
                {
                    _context.AttendanceRecords.Add(new AttendanceRecord
                    {
                        EmployeeId = employee.EmployeeId,
                        AttendanceDate = date,
                        CheckInTime = checkIn,
                        CheckOutTime = checkOut,
                        Status = lateMinutes > 0 ? "Late" : "Present",
                        LateMinutes = lateMinutes,
                        CreatedDate = DateTime.Now
                    });
                }
                else
                {
                    existing.CheckInTime = checkIn;
                    existing.CheckOutTime = checkOut;
                    existing.LateMinutes = lateMinutes;
                }
                result.SuccessCount++;
            }
            catch (Exception)
            {
                result.Errors.Add($"Row {result.TotalRows + 1}: Data format error.");
            }
        }
        await _context.SaveChangesAsync();
        result.Message = "MSG-SUC-05";
        return result;
    }

    public async Task<List<ShiftScheduleDTO>> GetWeeklyScheduleAsync(int employeeId)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        return await _context.ShiftAssignments
            .Include(sa => sa.Shift)
            .Where(sa => sa.EmployeeId == employeeId && sa.Status == "Active")
            .Select(sa => new ShiftScheduleDTO
            {
                Date = today,
                ShiftName = sa.Shift.ShiftName,
                StartTime = sa.Shift.StartTime,
                EndTime = sa.Shift.EndTime,
                Status = sa.Status
            })
            .ToListAsync();
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

    public async Task<List<AdminAttendanceDTO>> GetAdminViewAsync(DateOnly? date, int? deptId, string? status)
    {
        var query = _context.AttendanceRecords
            .Include(a => a.Employee).ThenInclude(e => e.Department)
            .Include(a => a.Shift)
            .AsQueryable();

        var filterDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        query = query.Where(a => a.AttendanceDate == filterDate);

        if (deptId.HasValue)
            query = query.Where(a => a.Employee.DepartmentId == deptId.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        return await query.Select(a => new AdminAttendanceDTO
        {
            AttendanceId = a.AttendanceId,
            EmployeeCode = a.Employee.EmployeeCode,
            FullName = a.Employee.FullName,
            DepartmentName = a.Employee.Department != null ? a.Employee.Department.DepartmentName : "N/A",
            Date = a.AttendanceDate.ToString("yyyy-MM-dd"),
            ShiftName = a.Shift != null ? a.Shift.ShiftName : "No Shift",
            CheckIn = a.CheckInTime.HasValue ? a.CheckInTime.Value.ToString("HH:mm:ss") : null,
            CheckOut = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("HH:mm:ss") : null,
            Status = a.Status,
            LateMinutes = a.LateMinutes
        }).ToListAsync();
    }
}