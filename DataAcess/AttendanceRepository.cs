using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly HrmsDbContext _context;
        public AttendanceRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<AttendanceRecord?> GetAttendanceByEmployeeAndDateAsync(int employeeId, DateOnly attendanceDate)
        {
            return await _context.AttendanceRecords
                .Include(a => a.Employee)
                .Include(a => a.Shift)
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == attendanceDate);
        }

        public async Task<AttendanceRecord?> GetAttendanceByIdAsync(int attendanceId)
        {
            return await _context.AttendanceRecords
                .Include(a => a.Employee)
                .Include(a => a.Shift)
                .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);
        }

        public async Task<List<AttendanceRecord>> GetAttendanceByDateAsync(DateOnly date)
        {
            return await _context.AttendanceRecords
                .Include(a => a.Employee)
                .Include(a => a.Shift)
                .Where(a => a.AttendanceDate == date)
                .OrderBy(a => a.Employee.FullName)
                .ToListAsync();
        }

        public async Task<List<AttendanceRecord>> SearchAttendanceAsync(DateOnly? fromDate, DateOnly? toDate, int? employeeId, string? status)
        {
            var query = _context.AttendanceRecords
                .Include(a => a.Employee)
                .Include(a => a.Shift)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.AttendanceDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AttendanceDate <= toDate.Value);

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(a => a.Status == status);

            return await query
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Employee.FullName)
                .ToListAsync();
        }

        public async Task AddAttendanceAsync(AttendanceRecord attendance)
        {
            await _context.AttendanceRecords.AddAsync(attendance);
        }

        public Task UpdateAttendanceAsync(AttendanceRecord attendance)
        {
            _context.AttendanceRecords.Update(attendance);
            return Task.CompletedTask;
        }

        public async Task AddAttendanceLogAsync(AttendanceLog log)
        {
            await _context.AttendanceLogs.AddAsync(log);
        }

        public async Task<List<AttendanceLog>> GetLogsByEmployeeAndDateAsync(int employeeId, DateOnly date)
        {
            var from = date.ToDateTime(TimeOnly.MinValue);
            var to = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

            return await _context.AttendanceLogs
                .Where(l => l.EmployeeId == employeeId && l.LogTime >= from && l.LogTime < to)
                .OrderBy(l => l.LogTime)
                .ToListAsync();
        }

        public async Task<ShiftAssignment?> GetActiveShiftAssignmentAsync(int employeeId, DateOnly date)
        {
            return await _context.ShiftAssignments
                .Include(sa => sa.Shift)
                .Where(sa => sa.EmployeeId == employeeId
                             && sa.Status == "Active"
                             && sa.StartDate <= date
                             && (sa.EndDate == null || sa.EndDate >= date))
                .OrderByDescending(sa => sa.StartDate)
                .FirstOrDefaultAsync();
        }

        public async Task<Shift?> GetShiftByIdAsync(int shiftId)
        {
            return await _context.Shifts.FirstOrDefaultAsync(s => s.ShiftId == shiftId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
