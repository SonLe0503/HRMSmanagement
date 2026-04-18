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

            var department = new Department
            {
                DepartmentCode = createDto.DepartmentCode,
                DepartmentName = createDto.DepartmentName,
                Description = createDto.Description,
                ManagerId = createDto.ManagerId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId()
            };

            await _departmentRepository.AddAsync(department);
            
            // Reload to get employees if needed, but for new dept it's empty anyway
            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentCode = department.DepartmentCode,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                ManagerId = FindDepartmentManagerId(department) ?? department.ManagerId,
                ManagerName = FindDepartmentManagerName(department),
                IsActive = department.IsActive,
                EmployeeCount = 0,
                Employees = new List<DepartmentEmployeeDto>(),
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
                EmployeeCount = d.Employees?.Count ?? 0,
                ManagerName = FindDepartmentManagerName(d)
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
                EmployeeCount = d.Employees?.Count ?? 0,
                ManagerName = FindDepartmentManagerName(d)
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
                ManagerId = FindDepartmentManagerId(department) ?? department.ManagerId,
                ManagerName = FindDepartmentManagerName(department),
                IsActive = department.IsActive,
                EmployeeCount = department.Employees?.Count ?? 0,
                Employees = MapDepartmentEmployees(department.Employees),
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
            var department = await _departmentRepository.GetByIdWithDetailsAsync(departmentId);
            if (department == null)
            {
                throw new KeyNotFoundException("Department not found.");
            }

            if (await _departmentRepository.DepartmentCodeExistsAsync(updateDto.DepartmentCode, departmentId))
            {
                throw new InvalidOperationException($"Department code '{updateDto.DepartmentCode}' already exists.");
            }

            department.DepartmentCode = updateDto.DepartmentCode;
            department.DepartmentName = updateDto.DepartmentName;
            department.Description = updateDto.Description;
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
                ManagerId = FindDepartmentManagerId(department) ?? department.ManagerId,
                ManagerName = FindDepartmentManagerName(department),
                IsActive = department.IsActive,
                EmployeeCount = department.Employees?.Count ?? 0,
                Employees = MapDepartmentEmployees(department.Employees),
                CreatedDate = department.CreatedDate,
                CreatedBy = department.CreatedBy,
                CreatedByName = "System",
                ModifiedDate = department.ModifiedDate,
                ModifiedBy = department.ModifiedBy,
                ModifiedByName = department.ModifiedBy.HasValue ? "System" : null
            };
        }

        private int? FindDepartmentManagerId(Department d)
        {
            if (d.Employees == null || !d.Employees.Any())
                return null;

            var manager = d.Employees
                .Where(e => e.Users.Any(u => u.UserRoles.Any(ur => ur.Role.RoleName == "MANAGE")))
                .OrderByDescending(e => e.Position?.Level ?? 0)
                .FirstOrDefault();

            return manager?.EmployeeId;
        }

        private string? FindDepartmentManagerName(Department d)
        {
            if (d.Employees == null || !d.Employees.Any())
                return null;

            var manager = d.Employees
                .Where(e => e.Users.Any(u => u.UserRoles.Any(ur => ur.Role.RoleName == "MANAGE")))
                .OrderByDescending(e => e.Position?.Level ?? 0)
                .FirstOrDefault();

            return manager?.FullName;
        }

        private IEnumerable<DepartmentEmployeeDto> MapDepartmentEmployees(ICollection<Employee>? employees)
        {
            if (employees == null) return new List<DepartmentEmployeeDto>();
            return employees.Select(e => new DepartmentEmployeeDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                PositionName = e.Position?.PositionName,
                Email = e.Email,
                Phone = e.Phone,
                EmploymentStatus = e.EmploymentStatus,
                Gender = e.Gender
            });
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
