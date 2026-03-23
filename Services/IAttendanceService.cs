using HRManagement.DTOs.Attendances;

namespace HRManagement.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceResponseDto> CheckInAsync(int employeeId, CheckInRequestDto dto);
        Task<AttendanceResponseDto> CheckOutAsync(int employeeId, CheckOutRequestDto dto);

        Task<AttendanceDetailResponseDto?> GetMyTodayAsync(int employeeId);
        Task<List<AttendanceResponseDto>> GetMyHistoryAsync(int employeeId, DateOnly? fromDate, DateOnly? toDate);

        Task<List<AttendanceResponseDto>> GetAttendanceByDateAsync(DateOnly date);
        Task<List<AttendanceResponseDto>> SearchAttendanceAsync(DateOnly? fromDate, DateOnly? toDate, int? employeeId, string? status);
        Task<AttendanceDetailResponseDto?> GetAttendanceDetailAsync(int employeeId, DateOnly date);

        Task<AttendanceResponseDto> ManualAdjustAttendanceAsync(int attendanceId, int approverId, ManualAdjustAttendanceDto dto);
        Task<AttendanceResponseDto> ManualCreateAttendanceAsync(int approverId, ManualCreateAttendanceDto dto);

        Task LockAttendanceAsync(int attendanceId, int userId);
        Task UnlockAttendanceAsync(int attendanceId, int userId);

        Task<List<AttendanceLogResponseDto>> GetLogsAsync(int employeeId, DateOnly date);
        Task AssignShiftAsync(int managerId, AssignShiftDto dto);
    }
}
