using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class CreateHRProcedureDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProcedureType { get; set; } = null!;

        [Required]
        public DateOnly EffectiveDate { get; set; }

        public int? NewDepartmentId { get; set; }

        public int? NewPositionId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? NewSalary { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public int? SubmittedBy { get; set; }
    }
    public class UpdateHRProcedureDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProcedureType { get; set; } = null!;

        [Required]
        public DateOnly EffectiveDate { get; set; }

        public int? NewDepartmentId { get; set; }

        public int? NewPositionId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? NewSalary { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class ApproveHRProcedureDto
    {
        public int? ApprovedBy { get; set; }
    }
    public class RejectHRProcedureDto
    {
        [Required]
        [MaxLength(500)]
        public string RejectionReason { get; set; } = null!;

        public int? ReviewedBy { get; set; }
    }
    public class HRProcedureResponseDto
    {
        public int ProcedureId { get; set; }
        public string ProcedureNumber { get; set; } = null!;
        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string ProcedureType { get; set; } = null!;
        public DateOnly EffectiveDate { get; set; }
        public int? NewDepartmentId { get; set; }
        public string? NewDepartmentName { get; set; }
        public int? NewPositionId { get; set; }
        public string? NewPositionName { get; set; }
        public decimal? NewSalary { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = null!;
        public string? RejectionReason { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int SubmittedBy { get; set; }
        public string SubmittedByName { get; set; } = null!;
        public DateTime? ReviewedDate { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
    }
    public class HRProcedureListDto
    {
        public int ProcedureId { get; set; }
        public string ProcedureNumber { get; set; } = null!;
        public string EmployeeFullName { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public string ProcedureType { get; set; } = null!;
        public DateOnly EffectiveDate { get; set; }
        public string Status { get; set; } = null!;
        public DateTime SubmittedDate { get; set; }
        public string SubmittedByName { get; set; } = null!;
    }
}
