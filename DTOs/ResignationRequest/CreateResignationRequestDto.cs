using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.ResignationRequest
{
    public class CreateResignationRequestDto
    {
        [Required]
        public DateOnly ExpectedLastWorkingDate { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }

        [MaxLength(2000)]
        public string? HandoverNote { get; set; }

        public int? HandoverToEmployeeId { get; set; }
    }
}
