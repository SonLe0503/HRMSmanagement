using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class EmployeeResponseDetailDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public DateOnly JoinDate { get; set; }
        public DateOnly? ResignationDate { get; set; }
        public string EmploymentStatus { get; set; } = null!;
        public string EmploymentType { get; set; } = null!;
        public decimal? BaseSalary { get; set; }
    }
    public class EmployeeResponseListDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }     
        public string? Gender { get; set; }      
        public string EmploymentStatus { get; set; } = null!;
        public DateOnly JoinDate { get; set; }
        public string DepartmentName { get; set; } = null!;
        public string PositionName { get; set; } = null!;
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public int? DepartmentId { get; set; }
        public string? RoleName { get; set; }
    }
    public class CreateEmployeeDto
    {
        //public int EmployeeId { get; set; }
        //[Required]
        //[MaxLength(20)]
        //public string EmployeeCode { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [MaxLength(20)]
        public string? Phone { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? Country { get; set; }

        public int? DepartmentId { get; set; }
        public int? PositionId { get; set; }
        public int? ManagerId { get; set; }

        [Required]
        public DateOnly JoinDate { get; set; }

        [Required]
        public string EmploymentStatus { get; set; } = null!;

        [Required]
        public string EmploymentType { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal? BaseSalary { get; set; }

        public int? CreatedBy { get; set; }
    }
    public class UpdateEmployeeDto
    {
        //[Required]
        //[MaxLength(20)]
        //public string EmployeeCode { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [MaxLength(20)]
        public string? Phone { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? Country { get; set; }

        public int? DepartmentId { get; set; }

        public int? PositionId { get; set; }

        public int? ManagerId { get; set; }

        [Required]
        public DateOnly JoinDate { get; set; }

        public DateOnly? ResignationDate { get; set; }

        [Required]
        public string EmploymentStatus { get; set; } = null!;

        [Required]
        public string EmploymentType { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal? BaseSalary { get; set; }

        public int? ModifiedBy { get; set; }
    }
}
