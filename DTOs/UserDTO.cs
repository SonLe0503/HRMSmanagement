namespace HRManagement.DTOs
{
    public class CreateUserDTO
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int EmployeeId { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }
    public class UpdateUserDTO
    {
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; }
        public int EmployeeId { get; set; }
        public List<int> RoleIds { get; set; } = new();
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

}
