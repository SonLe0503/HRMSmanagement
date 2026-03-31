using HRManagement.DataAcess.Interfaces;
using HRManagement.DTOs;
using HRManagement.Models;
using System.Security.Claims;

namespace HRManagement.Services.Departments
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DepartmentService(IDepartmentRepository departmentRepository, IHttpContextAccessor httpContextAccessor)
        {
            _departmentRepository = departmentRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> ActivateDepartmentAsync(int departmentId)
        {
            var department = await _departmentRepository.GetByIdAsync(departmentId);
            if (department == null)
                return false;

            if (department.IsActive)
                return false; 

            department.IsActive = true;
            department.ModifiedDate = DateTime.UtcNow;
            department.ModifiedBy = GetCurrentUserId();

            await _departmentRepository.UpdateAsync(department);

            return true;
        }

        public async Task<bool> DeactivateDepartmentAsync(int departmentId)
        {
            var department = await _departmentRepository.GetByIdAsync(departmentId);
            if (department == null)
                return false;

            if (!department.IsActive)
                return false; 

            if (await _departmentRepository.HasEmployeesAsync(departmentId))
            {
                throw new InvalidOperationException("Cannot deactivate department with active employees.");
            }
            if (await _departmentRepository.HasSubDepartmentsAsync(departmentId))
            {
                throw new InvalidOperationException("Cannot deactivate department with active sub-departments.");
            }

            department.IsActive = false;
            department.ModifiedDate = DateTime.UtcNow;
            department.ModifiedBy = GetCurrentUserId();

            await _departmentRepository.UpdateAsync(department);
            return true;
        }

        public async Task<DepartmentResponseDto> CreateDepartmentAsync(CreateDepartmentDto createDto)
        {
            if (await _departmentRepository.DepartmentCodeExistsAsync(createDto.DepartmentCode))
            {
                throw new InvalidOperationException($"Department code '{createDto.DepartmentCode}' already exists.");
            }

            if (createDto.ParentDepartmentId.HasValue)
            {
                if (!await _departmentRepository.ParentDepartmentExistsAsync(createDto.ParentDepartmentId.Value))
                {
                    throw new KeyNotFoundException("Parent department not found or inactive.");
                }
            }

            if (createDto.ManagerId.HasValue)
            {
                if (!await _departmentRepository.EmployeeExistsAsync(createDto.ManagerId.Value))
                {
                    throw new KeyNotFoundException("Manager employee not found.");
                }
            }

            var department = new Department
            {
                DepartmentCode = createDto.DepartmentCode,
                DepartmentName = createDto.DepartmentName,
                Description = createDto.Description,
                ParentDepartmentId = createDto.ParentDepartmentId,
                ManagerId = createDto.ManagerId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId()
            };

            await _departmentRepository.AddAsync(department);
            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                ParentDepartmentId = department.ParentDepartmentId,
                ParentDepartmentName = department.ParentDepartment?.DepartmentName,
                ManagerId = department.ManagerId,
                ManagerName = department.Manager?.FirstName + " " + department.Manager?.LastName,
                IsActive = department.IsActive,
                EmployeeCount = department.Employees?.Count ?? 0,
                SubDepartmentCount = department.InverseParentDepartment?.Count ?? 0,
                CreatedDate = department.CreatedDate,
                CreatedBy = department.CreatedBy,
                CreatedByName = "System",
                ModifiedDate = department.ModifiedDate,
                ModifiedBy = department.ModifiedBy,
                ModifiedByName = department.ModifiedBy.HasValue ? "System" : null
            };
        }

        public async Task<IEnumerable<DepartmentListDto>> GetActiveDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetActiveAsync();
            return departments.Select(d => new DepartmentListDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                IsActive = d.IsActive,
                ParentDepartmentName = d.ParentDepartment != null ? d.ParentDepartment.DepartmentName : null,
                ManagerName = d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}" : null
            }).ToList();
        }

        public async Task<IEnumerable<DepartmentListDto>> GetAllDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return departments.Select(d => new DepartmentListDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                IsActive = d.IsActive,
                ParentDepartmentName = d.ParentDepartment != null ? d.ParentDepartment.DepartmentName : null,
                ManagerName = d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}" : null
            }).ToList();
        }

        public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int departmentId)
        {
            var department = await _departmentRepository.GetByIdWithDetailsAsync(departmentId);
            if (department == null)
                return null;
            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                ParentDepartmentId = department.ParentDepartmentId,
                ParentDepartmentName = department.ParentDepartment?.DepartmentName,
                ManagerId = department.ManagerId,
                ManagerName = department.Manager != null ? $"{department.Manager.FirstName} {department.Manager.LastName}" : null,
                IsActive = department.IsActive,
                EmployeeCount = department.Employees?.Count ?? 0,
                SubDepartmentCount = department.InverseParentDepartment?.Count ?? 0,
                CreatedDate = department.CreatedDate,
                CreatedBy = department.CreatedBy,
                CreatedByName = "System",
                ModifiedDate = department.ModifiedDate,
                ModifiedBy = department.ModifiedBy,
                ModifiedByName = department.ModifiedBy.HasValue ? "System" : null
            };
        }

        public async Task<DepartmentResponseDto> UpdateDepartmentAsync(int departmentId, UpdateDepartmentDto updateDto)
        {
            var department = await _departmentRepository.GetByIdAsync(departmentId);
            if (department == null)
            {
                throw new KeyNotFoundException("Department not found.");
            }

            if (await _departmentRepository.DepartmentCodeExistsAsync(updateDto.DepartmentCode, departmentId))
            {
                throw new InvalidOperationException($"Department code '{updateDto.DepartmentCode}' already exists.");
            }

            if (updateDto.ParentDepartmentId.HasValue)
            {
                if (updateDto.ParentDepartmentId.Value == departmentId)
                {
                    throw new InvalidOperationException("Department cannot be its own parent.");
                }

                if (!await _departmentRepository.ParentDepartmentExistsAsync(updateDto.ParentDepartmentId.Value))
                {
                    throw new KeyNotFoundException("Parent department not found or inactive.");
                }
            }

            if (updateDto.ManagerId.HasValue)
            {
                if (!await _departmentRepository.EmployeeExistsAsync(updateDto.ManagerId.Value))
                {
                    throw new KeyNotFoundException("Manager employee not found.");
                }
            }

            department.DepartmentCode = updateDto.DepartmentCode;
            department.DepartmentName = updateDto.DepartmentName;
            department.Description = updateDto.Description;
            department.ParentDepartmentId = updateDto.ParentDepartmentId;
            department.ManagerId = updateDto.ManagerId;
            department.ModifiedDate = DateTime.UtcNow;
            department.ModifiedBy = GetCurrentUserId();

            await _departmentRepository.UpdateAsync(department);

            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                ParentDepartmentId = department.ParentDepartmentId,
                ParentDepartmentName = department.ParentDepartment?.DepartmentName,
                ManagerId = department.ManagerId,
                ManagerName = department.Manager != null ? $"{department.Manager.FirstName} {department.Manager.LastName}" : null,
                IsActive = department.IsActive,
                EmployeeCount = department.Employees?.Count ?? 0,
                SubDepartmentCount = department.InverseParentDepartment?.Count ?? 0,
                CreatedDate = department.CreatedDate,
                CreatedBy = department.CreatedBy,
                CreatedByName = "System",
                ModifiedDate = department.ModifiedDate,
                ModifiedBy = department.ModifiedBy,
                ModifiedByName = department.ModifiedBy.HasValue ? "System" : null
            };
        }
        private int GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(claim, out int userId))
                return userId;

            return 0;
        }
    }
}
