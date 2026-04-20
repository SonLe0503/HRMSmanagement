using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.ResignationRequest
{
    public class RejectResignationRequestDto
    {
        [Required]
        public string RejectionReason { get; set; } = null!;
    }
}
