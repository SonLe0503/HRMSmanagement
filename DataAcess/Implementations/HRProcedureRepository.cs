using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess.Implementations
{
    public class HRProcedureRepository : IHRProcedureRepository
    {
        private readonly HrmsDbContext _context;
        public HRProcedureRepository(HrmsDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Hrprocedure>> GetAllAsync()
        {
            return await _context.Hrprocedures
                .Include(p => p.Employee)
                .Include(p => p.NewDepartment)
                .Include(p => p.NewPosition)
                .OrderByDescending(p => p.SubmittedDate)
                .ToListAsync();
        }

        public async Task<Hrprocedure?> GetByIdWithDetailsAsync(int procedureId)
        {
            return await _context.Hrprocedures
                .Include(p => p.Employee)
                .Include(p => p.NewDepartment)
                .Include(p => p.NewPosition)
                .FirstOrDefaultAsync(p => p.ProcedureId == procedureId);
        }

        public async Task<Hrprocedure?> GetByIdAsync(int procedureId)
        {
            return await _context.Hrprocedures.FindAsync(procedureId);
        }

        public async Task<IEnumerable<Hrprocedure>> GetPendingProceduresAsync()
        {
            return await _context.Hrprocedures
                .Include(p => p.Employee)
                .Include(p => p.NewDepartment)
                .Include(p => p.NewPosition)
                .Where(p => p.Status == "Pending")
                .OrderBy(p => p.SubmittedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Hrprocedure>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.Hrprocedures
                .Include(p => p.Employee)
                .Include(p => p.NewDepartment)
                .Include(p => p.NewPosition)
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.SubmittedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Hrprocedure>> GetByStatusAsync(string status)
        {
            return await _context.Hrprocedures
                .Include(p => p.Employee)
                .Include(p => p.NewDepartment)
                .Include(p => p.NewPosition)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.SubmittedDate)
                .ToListAsync();
        }

        public async Task<bool> HasActiveProcedureAsync(int employeeId, string procedureType)
        {
            return await _context.Hrprocedures
                .AnyAsync(p => p.EmployeeId == employeeId &&
                              p.ProcedureType == procedureType &&
                              p.Status == "Pending");
        }

        public async Task<Hrprocedure> AddAsync(Hrprocedure procedure)
        {
            await _context.Hrprocedures.AddAsync(procedure);
            await _context.SaveChangesAsync();
            return procedure;
        }

        public async Task<Hrprocedure> UpdateAsync(Hrprocedure procedure)
        {
            _context.Hrprocedures.Update(procedure);
            await _context.SaveChangesAsync();
            return procedure;
        }

        public async Task<bool> DeleteAsync(int procedureId)
        {
            var procedure = await _context.Hrprocedures.FindAsync(procedureId);
            if (procedure == null)
                return false;

            _context.Hrprocedures.Remove(procedure);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsAsync(int procedureId)
        {
            return await _context.Hrprocedures.AnyAsync(p => p.ProcedureId == procedureId);
        }

        public async Task<bool> EmployeeExistsAsync(int employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> DepartmentExistsAsync(int departmentId)
        {
            return await _context.Departments.AnyAsync(d => d.DepartmentId == departmentId);
        }

        public async Task<bool> PositionExistsAsync(int positionId)
        {
            return await _context.Positions.AnyAsync(p => p.PositionId == positionId);
        }
    }
}
