using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.OvertimeRequest
{
    public class CreateOvertimeRequestDTO
    {
        [Required]
        public DateOnly OvertimeDate { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        public decimal TotalHours { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public string? TaskDescription { get; set; }
    }
}
