using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class CreateOvertimeRequestDTO
    {
        [Required]
        public DateTime OvertimeDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = null!;
    }
}
