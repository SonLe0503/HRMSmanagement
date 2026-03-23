using HRManagement.DataAcess;
using HRManagement.Models;

namespace HRManagement.Services
{
    public class ShiftAssignmentService : IShiftAssignmentService
    {
        private readonly IShiftAssignmentRepository _shiftAssignmentRepository;
        public ShiftAssignmentService(IShiftAssignmentRepository shiftAssignmentRepository)
        {
            _shiftAssignmentRepository = shiftAssignmentRepository;
        }

        public async Task<List<ShiftAssignment>> GetShiftAssignmentsAsync(DateOnly? date, int? employeeId, string? status)
        {
            return await _shiftAssignmentRepository.GetShiftAssignmentsAsync(date, employeeId, status);
        }

        public async Task<ShiftAssignment?> GetShiftAssignmentByIdAsync(int assignmentId)
        {
            return await _shiftAssignmentRepository.GetShiftAssignmentByIdAsync(assignmentId);
        }

        public async Task<List<ShiftAssignment>> GetMyShiftAssignmentsAsync(int employeeId, DateOnly? fromDate, DateOnly? toDate)
        {
            return await _shiftAssignmentRepository.GetMyShiftAssignmentsAsync(employeeId, fromDate, toDate);
        }
    }
}
