namespace HRManagement.DTOs.Attendances
{
    public class EmployeeFaceStatusDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public bool IsRegistered { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
