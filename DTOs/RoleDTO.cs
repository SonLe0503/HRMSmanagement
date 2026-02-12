namespace HRManagement.DTOs
{
    public class CreateRoleDTO
    {
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
    }
    public class UpdateRoleDTO
    {
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
    public class RoleResponseDTO
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
        public int UserCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }

}
