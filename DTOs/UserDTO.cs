using HRManagement.Models;

namespace HRManagement.DTOs
{
    public class CreateUserDTO
    {
        public string Email { get; set; } = null!;
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateUserDTO
    {
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; }
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }
    }

    public class UserResponseDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int? EmployeeId { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class RoleValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public List<Role> ValidRoles { get; set; } = new();
        public List<string> RoleNames { get; set; } = new();
    }
}
