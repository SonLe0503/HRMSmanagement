using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.OvertimeRequest
{
    public class CancelOvertimeRequestDTO
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
