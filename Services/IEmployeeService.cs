using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseListDto>> GetAllEmployeesAsync();
        Task<EmployeeResponseDetailDto?> GetEmployeeByIdAsync(int employeeId);
        Task<EmployeeResponseDetailDto> AddEmployeeAsync(CreateEmployeeDto dto);
        Task<EmployeeResponseDetailDto?> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto dto);
        Task<bool> UpdateStatusAsync(int id, string status, int? modifiedBy = null);
    }
}
