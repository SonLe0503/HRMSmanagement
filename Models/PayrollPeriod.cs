using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class PayrollPeriod
{
    public int PeriodId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly AttendanceCutoffDate { get; set; }

    public int ReviewWindowDays { get; set; }

    public DateTime? CalculatedDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ReviewDeadline { get; set; }

    public string? RejectionReason { get; set; }

    public int? RejectedBy { get; set; }

    public DateTime? RejectedDate { get; set; }

    public virtual ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();

    public virtual ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}
