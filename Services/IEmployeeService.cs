using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseListDto>> GetAllEmployeesAsync();
        Task<EmployeeResponseDetailDto?> GetEmployeeByIdAsync(int employeeId);
        Task<Employee> AddEmployeeAsync(CreateEmployeeDto dto);
        Task<Employee> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto dto);
        Task<bool> DisableEmployeeAsync(int id, int? disabledBy = null);
        Task<bool> EnableEmployeeAsync(int id, int? enabledBy = null);
    }
}
