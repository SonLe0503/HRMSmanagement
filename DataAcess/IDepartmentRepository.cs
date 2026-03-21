using HRManagement.Models;

namespace HRManagement.DataAcess
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync();
        Task<IEnumerable<Department>> GetActiveAsync();
        Task<Department?> GetByIdWithDetailsAsync(int departmentId);
        Task<Department?> GetByIdAsync(int departmentId);
        Task<IEnumerable<Department>> GetByParentIdAsync(int parentDepartmentId);
        Task<Department> AddAsync(Department department);
        Task<Department> UpdateAsync(Department department);
        Task<bool> ExistsAsync(int departmentId);
        Task<bool> DepartmentCodeExistsAsync(string departmentCode, int? excludeDepartmentId = null);
        Task<bool> ParentDepartmentExistsAsync(int parentDepartmentId);
        Task<bool> EmployeeExistsAsync(int employeeId);
        Task<bool> HasEmployeesAsync(int departmentId);
        Task<bool> HasSubDepartmentsAsync(int departmentId);
    }
}
