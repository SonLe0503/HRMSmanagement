using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class OvertimeRequest
{
    public int OvertimeRequestId { get; set; }

    public string RequestNumber { get; set; } = null!;

    public int EmployeeId { get; set; }

    public DateOnly OvertimeDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public decimal TotalHours { get; set; }

    public string? Reason { get; set; }

    public string? TaskDescription { get; set; }

    public string Status { get; set; } = null!;

    public string? RejectionReason { get; set; }

    public DateTime SubmittedDate { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public int? ApprovedBy { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual User? ReviewedByNavigation { get; set; }
}
