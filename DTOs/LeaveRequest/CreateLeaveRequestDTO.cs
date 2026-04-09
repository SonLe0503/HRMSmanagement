using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.LeaveRequest
{
    public class CreateLeaveRequestDTO
    {
        [Required]
        public int LeaveTypeID { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required]
        [Range(0.5, 365)]
        public decimal NumberOfDays { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        // Nếu muốn submit dù thiếu balance
        public bool SubmitAnyway { get; set; } = false;
    }
}
