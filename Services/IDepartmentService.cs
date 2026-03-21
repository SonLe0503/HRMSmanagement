using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface IDepartmentService
    {
        Task<DepartmentResponseDto> CreateDepartmentAsync(CreateDepartmentDto createDto);
        Task<IEnumerable<DepartmentListDto>> GetAllDepartmentsAsync();
        Task<IEnumerable<DepartmentListDto>> GetActiveDepartmentsAsync();
        Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int departmentId);
        Task<DepartmentResponseDto> UpdateDepartmentAsync(int departmentId, UpdateDepartmentDto updateDto);
        Task<bool> DeactivateDepartmentAsync(int departmentId);
        Task<bool> ActivateDepartmentAsync(int departmentId);

    }
}
