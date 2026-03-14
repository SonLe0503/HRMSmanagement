using HRManagement.Models;

namespace HRManagement.DataAcess
{
    public interface IAttendanceRepository
    {
        Task<AttendanceRecord?> GetAttendanceByEmployeeAndDateAsync(int employeeId, DateOnly attendanceDate);
        Task<AttendanceRecord?> GetAttendanceByIdAsync(int attendanceId);
        Task<List<AttendanceRecord>> GetAttendanceByDateAsync(DateOnly date);
        Task<List<AttendanceRecord>> SearchAttendanceAsync(DateOnly? fromDate, DateOnly? toDate, int? employeeId, string? status);

        Task AddAttendanceAsync(AttendanceRecord attendance);
        Task UpdateAttendanceAsync(AttendanceRecord attendance);

        Task AddAttendanceLogAsync(AttendanceLog log);
        Task<List<AttendanceLog>> GetLogsByEmployeeAndDateAsync(int employeeId, DateOnly date);

        Task<ShiftAssignment?> GetActiveShiftAssignmentAsync(int employeeId, DateOnly date);
        Task<Shift?> GetShiftByIdAsync(int shiftId);

        Task SaveChangesAsync();
    }
}
