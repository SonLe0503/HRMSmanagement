using HRManagement.Models;

namespace HRManagement.Services.Shifts
{
    public interface IShiftAssignmentService
    {
        Task<List<ShiftAssignment>> GetShiftAssignmentsAsync(DateOnly? date, int? employeeId, string? status);
        Task<ShiftAssignment?> GetShiftAssignmentByIdAsync(int assignmentId);
        Task<List<ShiftAssignment>> GetMyShiftAssignmentsAsync(int employeeId, DateOnly? fromDate, DateOnly? toDate);
    }
}
