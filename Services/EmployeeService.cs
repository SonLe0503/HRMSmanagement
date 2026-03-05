using HRManagement.DataAcess;
using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HRManagement.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public EmployeeService(IEmployeeRepository employeeRepository, IHttpContextAccessor httpContextAccessor)
        {
            _employeeRepository = employeeRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<EmployeeResponseDetailDto> AddEmployeeAsync(CreateEmployeeDto dto)
        {
            if (await _employeeRepository.EmployeeCodeExistsAsync(dto.EmployeeCode))
            {
                throw new InvalidOperationException($"Employee code '{dto.EmployeeCode}' already exists.");
            }
            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FullName = $"{dto.FirstName} {dto.LastName}",
                Email = dto.Email,
                Phone = dto.Phone,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Address = dto.Address,
                City = dto.City,
                Country = dto.Country,
                DepartmentId = dto.DepartmentId,
                PositionId = dto.PositionId,
                ManagerId = dto.ManagerId,
                JoinDate = dto.JoinDate,
                EmploymentStatus = dto.EmploymentStatus,
                EmploymentType = dto.EmploymentType,
                BaseSalary = dto.BaseSalary,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = dto.CreatedBy ?? int.Parse(_httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "0")
            };
            await _employeeRepository.AddEmployeeAsync(employee);
            return await GetEmployeeByIdAsync(employee.EmployeeId) ?? throw new InvalidOperationException("Failed to retrieve the newly created employee.");
        }

        public async Task<bool> UpdateStatusAsync(int id, string status, int? modifiedBy = null)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
                return false;

            if (status == "Resigned" || status == "Terminated"|| status == "Inactive")
            {
                employee.ResignationDate = DateOnly.FromDateTime(DateTime.Now);
            }
            else if (status == "Active")
            {
                employee.ResignationDate = null;
            }
            employee.EmploymentStatus = status;
            employee.ModifiedDate = DateTime.UtcNow;
            employee.ModifiedBy = modifiedBy;

            await _employeeRepository.UpdateEmployeeAsync(employee);
            return true;
        }

        public async Task<IEnumerable<EmployeeResponseListDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return employees.Select(e => new EmployeeResponseListDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName, 
                Email = e.Email,
                Phone = e.Phone,
                Gender = e.Gender,
                EmploymentStatus = e.EmploymentStatus,
                DepartmentName = e.Department?.DepartmentName ?? "N/A",
                PositionName = e.Position?.PositionName ?? "N/A"
            }).ToList();
        }

        public async Task<EmployeeResponseDetailDto?> GetEmployeeByIdAsync(int employeeId)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
            if (employee is null) return null;

            return new EmployeeResponseDetailDto
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Phone = employee.Phone,
                DateOfBirth = employee.DateOfBirth,
                Gender = employee.Gender,
                Address = employee.Address,
                City = employee.City,
                Country = employee.Country,
                DepartmentId = employee.DepartmentId ?? null,
                DepartmentName = employee.Department?.DepartmentName ?? "N/A",
                PositionId = employee.PositionId ?? null,
                PositionName = employee.Position?.PositionName ?? "N/A",
                ManagerId = employee.ManagerId ?? null,
                ManagerName = employee.Manager is null ? "N/A" : $"{employee.Manager.FirstName} {employee.Manager.LastName}",
                JoinDate = employee.JoinDate,
                ResignationDate = employee.ResignationDate,
                EmploymentStatus = employee.EmploymentStatus,
                EmploymentType = employee.EmploymentType,
                BaseSalary = employee.BaseSalary,
            };
        }

        public async Task<EmployeeResponseDetailDto> UpdateEmployeeAsync(int employeeId, UpdateEmployeeDto dto)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException($"Employee with ID {employeeId} not found."); 

            if (await _employeeRepository.EmployeeCodeExistsAsync(dto.EmployeeCode, employeeId))
            {
                throw new InvalidOperationException($"Employee code '{dto.EmployeeCode}' already exists.");
            }

            if (dto.ResignationDate.HasValue && dto.ResignationDate.Value < dto.JoinDate)
            {
                throw new InvalidOperationException("Resignation date cannot be before join date.");
            }

            employee.EmployeeCode = dto.EmployeeCode;
            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.FullName = $"{dto.FirstName} {dto.LastName}";
            employee.Email = dto.Email;
            employee.Phone = dto.Phone;
            employee.DateOfBirth = dto.DateOfBirth;
            employee.Gender = dto.Gender;
            employee.Address = dto.Address;
            employee.City = dto.City;
            employee.Country = dto.Country;
            employee.DepartmentId = dto.DepartmentId;
            employee.PositionId = dto.PositionId;
            employee.ManagerId = dto.ManagerId;
            employee.JoinDate = dto.JoinDate;
            employee.ResignationDate = dto.ResignationDate;
            employee.EmploymentStatus = dto.EmploymentStatus;
            employee.EmploymentType = dto.EmploymentType;
            employee.BaseSalary = dto.BaseSalary;
            employee.ModifiedDate = DateTime.Now;
            employee.ModifiedBy = dto.ModifiedBy;

            await _employeeRepository.UpdateEmployeeAsync(employee);

            return await GetEmployeeByIdAsync(employee.EmployeeId) ?? throw new InvalidOperationException("Failed to retrieve the update employee.");
        }
    }
}
