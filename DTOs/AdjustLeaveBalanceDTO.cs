using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class AdjustLeaveBalanceDTO
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public string AdjustmentType { get; set; } = string.Empty; // Add / Deduct

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Adjustment amount must be greater than 0.")]
        public decimal NumberOfDays { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public DateOnly EffectiveDate { get; set; }
    }
}
