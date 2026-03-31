using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess.Implementations
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly HrmsDbContext _context;
        public DepartmentRepository(HrmsDbContext context)
        {
            _context = context;
        }
        public async Task<Department> AddAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            _context.SaveChanges();
            return department;
        }

        public async Task<bool> DepartmentCodeExistsAsync(string departmentCode, int? excludeDepartmentId = null)
        {
            var department = _context.Departments.Where(d => d.DepartmentCode == departmentCode);
            if (excludeDepartmentId.HasValue)
            {
                department = department.Where(d => d.DepartmentId != excludeDepartmentId.Value);
            }
            return await department.AnyAsync();
        }

        public async Task<bool> EmployeeExistsAsync(int employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<bool> ExistsAsync(int departmentId)
        {
            return await _context.Departments.AnyAsync(d => d.DepartmentId == departmentId);
        }

        public async Task<IEnumerable<Department>> GetActiveAsync()
        {
            return await _context.Departments.Include(e => e.Employees).Include(e => e.ParentDepartment).Include(e => e.Manager).Where(e => e.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments.Include(d => d.ParentDepartment).Include(d => d.Employees).Include(d => d.Manager).ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int departmentId)
        {
            return await _context.Departments.FindAsync(departmentId);
        }

        public async Task<Department?> GetByIdWithDetailsAsync(int departmentId)
        {
            return await _context.Departments
                .Include(d => d.ParentDepartment)
                .Include(d => d.Employees)
                .Include(d => d.InverseParentDepartment)
                .Include(d => d.Manager)
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
        }

        public async Task<IEnumerable<Department>> GetByParentIdAsync(int parentDepartmentId)
        {
            return await _context.Departments.Where(d => d.ParentDepartmentId == parentDepartmentId).ToListAsync();
        }

        public async Task<bool> HasEmployeesAsync(int departmentId)
        {
            return await _context.Employees.AnyAsync(e => e.DepartmentId == departmentId);
        }

        public Task<bool> HasSubDepartmentsAsync(int departmentId)
        {
            return _context.Departments.AnyAsync(d => d.ParentDepartmentId == departmentId);
        }

        public Task<bool> ParentDepartmentExistsAsync(int parentDepartmentId)
        {
            return _context.Departments.AnyAsync(d => d.DepartmentId == parentDepartmentId && d.IsActive);
        }

        public async Task<Department> UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
            return department;
        }
    }
}
