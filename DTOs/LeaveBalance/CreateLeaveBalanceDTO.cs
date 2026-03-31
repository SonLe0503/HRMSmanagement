using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.LeaveBalance
{
    public class CreateLeaveBalanceDTO
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [Range(0, 365)]
        public decimal TotalEntitlement { get; set; }

        [Range(0, 365)]
        public decimal UsedDays { get; set; } = 0;

        [Range(0, 365)]
        public decimal CarriedForward { get; set; } = 0;

    }
}
