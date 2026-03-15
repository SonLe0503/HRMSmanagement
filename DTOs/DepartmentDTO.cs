using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class CreateDepartmentDto
    {
        [Required]
        [MaxLength(20)]
        public string DepartmentCode { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }

        public int? ManagerId { get; set; }
    }
    public class UpdateDepartmentDto
    {
        [Required]
        [MaxLength(20)]
        public string DepartmentCode { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string DepartmentName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ParentDepartmentId { get; set; }

        public int? ManagerId { get; set; }
    }
    public class DepartmentResponseDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentCode { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentDepartmentId { get; set; }
        public string? ParentDepartmentName { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeCount { get; set; }
        public int SubDepartmentCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
    }
    public class DepartmentListDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentCode { get; set; } = null!;
        public string DepartmentName { get; set; } = null!;
        public string? ParentDepartmentName { get; set; }
        public string? ManagerName { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
    }
}
