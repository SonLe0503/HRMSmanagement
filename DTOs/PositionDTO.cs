using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class CreatePositionDto
    {
        [Required]
        [MaxLength(20)]
        public string PositionCode { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string PositionName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(1, 10)]
        public int Level { get; set; }
        public bool IsTopLevel { get; set; }
    }
    public class UpdatePositionDto
    {
        [Required]
        [MaxLength(20)]
        public string PositionCode { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string PositionName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(1, 10)]
        public int Level { get; set; }
        public bool IsTopLevel { get; set; }
    }
    public class PositionResponseDto
    {
        public int PositionId { get; set; }
        public string PositionCode { get; set; } = null!;
        public string PositionName { get; set; } = null!;
        public string? Description { get; set; }
        public int Level { get; set; }
        public bool IsTopLevel { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
    }

    public class PositionListDto
    {
        public int PositionId { get; set; }
        public string PositionCode { get; set; } = null!;
        public string PositionName { get; set; } = null!;
        public int Level { get; set; }
        public bool IsTopLevel { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
    }
}
