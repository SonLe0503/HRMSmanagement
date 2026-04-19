using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class LeaveRequest
{
    public int LeaveRequestId { get; set; }

    public string RequestNumber { get; set; } = null!;

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal NumberOfDays { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public string? RejectionReason { get; set; }

    public DateTime SubmittedDate { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public int? ReviewedBy { get; set; }

    public string? ReviewerComments { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public int? ApprovedBy { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }
    
    public int? TargetApproverId { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual LeaveType LeaveType { get; set; } = null!;

    public virtual User? ReviewedByNavigation { get; set; }
    
    public virtual User? TargetApprover { get; set; }
}
