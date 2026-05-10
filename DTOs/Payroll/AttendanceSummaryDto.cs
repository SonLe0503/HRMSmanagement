using System;
using System.Collections.Generic;

namespace HRManagement.DTOs.Payroll
{
    public class AttendanceSummaryDto
    {
        public List<AttendanceItemDto> Records { get; set; } = new();
        public List<LeaveItemDto> ApprovedLeaves { get; set; } = new();
        public List<OvertimeItemDto> ApprovedOvertime { get; set; } = new();
        public AttendanceTotalsDto Totals { get; set; } = new();
    }

    public class AttendanceItemDto
    {
        public string Date { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal WorkingHours { get; set; }
        public bool IsExplanationApproved { get; set; }
    }

    public class LeaveItemDto
    {
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public string LeaveTypeName { get; set; } = "";
        public bool IsPaid { get; set; }
        public decimal Days { get; set; }
    }

    public class OvertimeItemDto
    {
        public string Date { get; set; } = "";
        public decimal Hours { get; set; }
    }

    public class AttendanceTotalsDto
    {
        public int PresentDays { get; set; }
        public int LateDays { get; set; }
        public int AbsentDays { get; set; }
        public int ExplanationApprovedDays { get; set; }
        public decimal PaidLeaveDays { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal TotalActualDays { get; set; }
    }
}
