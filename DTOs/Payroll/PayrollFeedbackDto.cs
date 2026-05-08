using System;
using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.Payroll
{
    public class PayrollFeedbackDto
    {
        public int FeedbackId { get; set; }
        public int PayrollRecordId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string DepartmentName { get; set; } = "";
        public string? Content { get; set; }
        public bool IsAgreed { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string? HrResponse { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedByName { get; set; }
        public decimal NetPay { get; set; }
        public string PeriodLabel { get; set; } = "";
    }

    public class CreatePayrollFeedbackDto
    {
        public bool IsAgreed { get; set; }

        [StringLength(500)]
        public string? Content { get; set; }
    }

    public class ResolveFeedbackDto
    {
        [Required]
        public string Status { get; set; } = ""; // "Resolved" | "Dismissed"

        [Required(ErrorMessage = "Phản hồi của HR không được để trống.")]
        [StringLength(500)]
        public string HrResponse { get; set; } = "";
    }

    public class PublishForReviewDto
    {
        [Range(1, 30, ErrorMessage = "Số ngày xem xét phải từ 1 đến 30.")]
        public int ReviewDays { get; set; } = 3;
    }
}
