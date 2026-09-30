namespace HRManagement.DTOs
{
    public class PermissionResponseDTO
    {
        public int PermissionId { get; set; }
        public string PermissionKey { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class UpdateRolePermissionsDTO
    {
        public List<string> PermissionKeys { get; set; } = new();
    }
}
