using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.LeaveTypes
{
    public class CreateLeaveTypeDTO
    {
        [MaxLength(50)]
        public string? LeaveTypeCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string LeaveTypeName { get; set; } = null!;

        [Range(0, 365)]
        public int AnnualEntitlement { get; set; }

        public bool IsPaid { get; set; }
        public bool RequiresApproval { get; set; }
        public bool IsCarryForward { get; set; }

        [Range(0, 365)]
        public int? MaxCarryForwardDays { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
