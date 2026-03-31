using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.LeaveRequest
{
    public class RejectLeaveRequestDTO
    {
        [Required]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
