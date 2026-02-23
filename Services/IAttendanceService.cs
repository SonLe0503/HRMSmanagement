using HRManagement.DTOs;

namespace HRManagement.Services;

public interface IAttendanceService
{
    // Shift Management
    Task<string> AssignShiftAsync(CreateShiftAssignmentDTO dto);

    // Check-in / Check-out
    Task<AttendanceResponseDTO> CheckInAsync(CheckInRequestDTO dto);
    Task<AttendanceResponseDTO> CheckOutAsync(int employeeId);
    Task<List<AttendanceHistoryDTO>> GetHistoryAsync(int employeeId);
}