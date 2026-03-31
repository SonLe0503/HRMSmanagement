using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.OvertimeRequest
{
    public class RejectOvertimeRequestDTO
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
