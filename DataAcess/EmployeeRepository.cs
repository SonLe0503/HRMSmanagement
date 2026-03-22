using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly HrmsDbContext _context;
        public EmployeeRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            employee.CreatedDate = DateTime.Now;
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return employee;
        }
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                 .Include(d => d.Department)
                 .Include(p => p.Position)
                 .Include(m => m.Manager)
                 .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Manager)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            employee.ModifiedDate = DateTime.Now;
            _context.Employees.Update(employee); 
            await _context.SaveChangesAsync(); 
            return employee;
        }
        public async Task<bool> EmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeCode == employeeCode && (excludeEmployeeId == null || e.EmployeeId != excludeEmployeeId.Value));
        }
        public async Task<bool> DepartmentExistsAsync(int id)
        {
            return await _context.Departments.AnyAsync(d => d.DepartmentId == id);
        }
        public async Task<bool> PositionExistsAsync(int id)
        {
            return await _context.Positions.AnyAsync(p => p.PositionId == id);
        }
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Employees.AnyAsync(e => e.Email == email);
        }
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> EmployeeExistsAsync(int employeeId)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeId == employeeId);
        }
    }
}
