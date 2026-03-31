using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagement.Models
{
    public partial class FaceVerificationLog
    {
        [Key]
        public int VerificationId { get; set; }

        public int EmployeeId { get; set; }

        public int? AttendanceLogId { get; set; }

        [Required]
        [MaxLength(20)]
        public string VerificationType { get; set; } = null!; // CheckIn / CheckOut

        [MaxLength(500)]
        public string? CapturedImagePath { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ConfidenceScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ThresholdUsed { get; set; }

        public bool IsMatch { get; set; }

        public bool? LivenessPassed { get; set; }

        [MaxLength(255)]
        public string? FailureReason { get; set; }

        public DateTime VerifiedAt { get; set; }

        [MaxLength(255)]
        public string? DeviceInfo { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public int? CreatedBy { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public virtual Employee? Employee { get; set; }

        [ForeignKey(nameof(AttendanceLogId))]
        public virtual AttendanceLog? AttendanceLog { get; set; }
    }
}
