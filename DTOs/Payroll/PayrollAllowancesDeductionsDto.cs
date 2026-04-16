using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.Payroll
{
    public class PayrollAllowanceDto
    {
        public int AllowanceId { get; set; }
        public int PayrollRecordId { get; set; }
        public string AllowanceType { get; set; }   // "Meal" | "Transport" | "Phone" | "Responsibility" | ...
        public string AllowanceName { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class CreatePayrollAllowanceDto
    {
        [Required]
        public string AllowanceType { get; set; }
        [Required]
        public string AllowanceName { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class PayrollDeductionDto
    {
        public int DeductionId { get; set; }
        public int PayrollRecordId { get; set; }
        public string DeductionType { get; set; }   // "Insurance" | "Tax" | "Advance" | "Penalty" | "Other"
        public string DeductionName { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class CreatePayrollDeductionDto
    {
        [Required]
        public string DeductionType { get; set; }
        [Required]
        public string DeductionName { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
