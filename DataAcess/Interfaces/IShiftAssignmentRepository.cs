using HRManagement.Models;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IShiftAssignmentRepository
    {
        Task<List<ShiftAssignment>> GetShiftAssignmentsAsync(DateOnly? date, int? employeeId, string? status);
        Task<ShiftAssignment?> GetShiftAssignmentByIdAsync(int assignmentId);
        Task<List<ShiftAssignment>> GetMyShiftAssignmentsAsync(int employeeId, DateOnly? fromDate, DateOnly? toDate);

        Task<ShiftAssignment?> GetShiftAssignmentByEmployeeAndDateAsync(int employeeId, DateOnly date);

        System.Threading.Tasks.Task AddShiftAssignmentAsync(ShiftAssignment assignment);
        System.Threading.Tasks.Task UpdateShiftAssignmentAsync(ShiftAssignment assignment);
        System.Threading.Tasks.Task DeleteShiftAssignmentAsync(ShiftAssignment assignment);

        System.Threading.Tasks.Task SaveChangesAsync();

    }
}
