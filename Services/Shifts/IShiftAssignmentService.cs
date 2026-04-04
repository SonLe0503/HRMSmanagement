using HRManagement.DTOs.ShiftAssignments;
using HRManagement.Models;

namespace HRManagement.Services.Shifts
{
    public interface IShiftAssignmentService
    {
        System.Threading.Tasks.Task AssignShiftAsync(int managerId, AssignShiftDto dto);

        Task<List<ShiftAssignmentResponseDto>> GetShiftAssignmentsAsync(DateOnly? date, int? employeeId, string? status);

        Task<ShiftAssignmentResponseDto?> GetShiftAssignmentByIdAsync(int assignmentId);

        Task<List<ShiftAssignmentResponseDto>> GetMyShiftAssignmentsAsync(int employeeId, DateOnly? fromDate, DateOnly? toDate);

        Task<ShiftAssignmentResponseDto> UpdateShiftAssignmentAsync(int assignmentId, UpdateShiftAssignmentDto dto);

        System.Threading.Tasks.Task DeactivateShiftAssignmentAsync(int assignmentId);

        System.Threading.Tasks.Task ActivateShiftAssignmentAsync(int assignmentId);

        System.Threading.Tasks.Task DeleteShiftAssignmentAsync(int assignmentId);

    }
}
