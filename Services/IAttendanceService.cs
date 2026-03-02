using HRManagement.DTOs;

namespace HRManagement.Services;

public interface IAttendanceService
{
    // Shift Management
    Task<string> AssignShiftAsync(CreateShiftAssignmentDTO dto);
    Task<string> UpdateAssignmentAsync(int id, UpdateShiftAssignmentDTO dto);
    Task<List<ShiftScheduleDTO>> GetWeeklyScheduleAsync(int employeeId);

    // Check-in / Check-out
    Task<AttendanceResponseDTO> CheckInAsync(CheckInRequestDTO dto);
    Task<AttendanceResponseDTO> CheckOutAsync(int employeeId);
    Task<List<AttendanceHistoryDTO>> GetHistoryAsync(int employeeId);

    Task<List<AdminAttendanceDTO>> GetAdminViewAsync(DateOnly? date, int? deptId, string? status);
}