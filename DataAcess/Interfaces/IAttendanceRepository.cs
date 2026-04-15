using HRManagement.Models;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<AttendanceRecord?> GetAttendanceByEmployeeAndDateAsync(int employeeId, DateOnly attendanceDate);
        Task<AttendanceRecord?> GetAttendanceByIdAsync(int attendanceId);
        Task<List<AttendanceRecord>> GetAttendanceByDateAsync(DateOnly date);
        Task<List<AttendanceRecord>> SearchAttendanceAsync(DateOnly? fromDate, DateOnly? toDate, int? employeeId, string? status);

        System.Threading.Tasks.Task AddAttendanceAsync(AttendanceRecord attendance);
        System.Threading.Tasks.Task UpdateAttendanceAsync(AttendanceRecord attendance);

        System.Threading.Tasks.Task AddAttendanceLogAsync(AttendanceLog log);
        Task<List<AttendanceLog>> GetLogsByEmployeeAndDateAsync(int employeeId, DateOnly date);

        Task<ShiftAssignment?> GetActiveShiftAssignmentAsync(int employeeId, DateOnly date);
        Task<ShiftAssignment?> GetShiftAssignmentByEmployeeAndDateAsync(int employeeId, DateOnly date);
        System.Threading.Tasks.Task AddShiftAssignmentAsync(ShiftAssignment assignment);

        Task<Shift?> GetShiftByIdAsync(int shiftId);

        Task<FaceProfile?> GetActiveFaceProfileByEmployeeIdAsync(int employeeId);
        Task<List<(Employee Employee, FaceProfile? FaceProfile)>> GetAllEmployeesWithFaceProfileAsync();
        System.Threading.Tasks.Task AddFaceProfileAsync(FaceProfile faceProfile);
        System.Threading.Tasks.Task UpdateFaceProfileAsync(FaceProfile faceProfile);

        System.Threading.Tasks.Task AddFaceVerificationLogAsync(FaceVerificationLog log);

        Task<AttendanceRecord?> GetOpenAttendanceRecordAsync(int employeeId);

        System.Threading.Tasks.Task SaveChangesAsync();
    }
}
