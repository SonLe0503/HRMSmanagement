using System;
using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs.Payroll
{
    public class PayrollPeriodDto
    {
        public int PeriodId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } = "";  // Open | Aggregated | Calculated | UnderReview | Approved | Rejected
        public int TotalEmployees { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal TotalInsurance { get; set; }
        public decimal TotalTax { get; set; }
        public DateTime? AggregatedDate { get; set; }
        public DateTime? CalculatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedByName { get; set; }
        // UnderReview fields
        public DateTime? ReviewDeadline { get; set; }
        public bool ReviewDeadlineExpired { get; set; }
        public bool AllAgreed { get; set; }
        public int AgreedCount { get; set; }
        // Rejection fields
        public string? RejectionReason { get; set; }
        public string? RejectedByName { get; set; }
        public DateTime? RejectedDate { get; set; }
    }

    public class RejectPayrollPeriodDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "Lý do từ chối không được để trống.")]
        public string Reason { get; set; } = "";
    }

    public class CreatePayrollPeriodDto
    {
        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
