using HRManagement.Models;
namespace HRManagement.DataAcess
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);
        Task<Employee> AddEmployeeAsync(Employee employee);
        Task<Employee> UpdateEmployeeAsync(Employee employee);
        Task<bool> EmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null);
        Task<bool> DepartmentExistsAsync(int id);
        Task<bool> PositionExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
    }
}
