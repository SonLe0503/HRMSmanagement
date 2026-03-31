using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess.Implementations
{
    public class ShiftAssignmentRepository : IShiftAssignmentRepository
    {
        private readonly HrmsDbContext _context;
        public ShiftAssignmentRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShiftAssignment>> GetShiftAssignmentsAsync(DateOnly? date, int? employeeId, string? status)
        {
            var query = _context.ShiftAssignments
                .Include(x => x.Employee)
                .Include(x => x.Shift)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(x => x.AssignmentDate == date.Value);
            }

            if (employeeId.HasValue)
                query = query.Where(x => x.EmployeeId == employeeId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.Status == status);

            return await query
                .OrderByDescending(x => x.StartDate)
                .ThenBy(x => x.Employee.FullName)
                .ToListAsync();
        }

        public async Task<ShiftAssignment?> GetShiftAssignmentByIdAsync(int assignmentId)
        {
            return await _context.ShiftAssignments
                .Include(x => x.Employee)
                .Include(x => x.Shift)
                .FirstOrDefaultAsync(x => x.AssignmentId == assignmentId);
        }

        public async Task<List<ShiftAssignment>> GetMyShiftAssignmentsAsync(int employeeId, DateOnly? fromDate, DateOnly? toDate)
        {
            var query = _context.ShiftAssignments
                .Include(x => x.Employee)
                .Include(x => x.Shift)
                .Where(x => x.EmployeeId == employeeId)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(x => x.EndDate == null || x.EndDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.StartDate <= toDate.Value);

            return await query
                .OrderBy(x => x.StartDate)
                .ToListAsync();
        }
    }
}
