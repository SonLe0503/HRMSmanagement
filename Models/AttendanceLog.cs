using System.ComponentModel.DataAnnotations;

namespace HRManagement.Models
{
    public partial class AttendanceLog
    {
        public int LogId { get; set; }

        public int EmployeeId { get; set; }

        public int? ShiftId { get; set; }

        public DateTime LogTime { get; set; }

        public string LogType { get; set; } = null!; // CheckIn / CheckOut

        public string Source { get; set; } = null!; // Web / Machine / Import / Manual

        public string? DeviceInfo { get; set; }

        public string? IpAddress { get; set; }

        public string? Location { get; set; }

        public string? Remarks { get; set; }

        public bool IsValid { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? CreatedBy { get; set; }

        public virtual Employee Employee { get; set; } = null!;

        public virtual Shift? Shift { get; set; }

        [MaxLength(20)]
        public string? VerificationMethod { get; set; }

        [MaxLength(20)]
        public string? VerificationStatus { get; set; }

        public int? FaceVerificationId { get; set; }
    }
}
