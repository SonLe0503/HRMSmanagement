using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class LeaveType
{
    public int LeaveTypeId { get; set; }

    public string LeaveTypeCode { get; set; } = null!;

    public string LeaveTypeName { get; set; } = null!;

    public int AnnualEntitlement { get; set; }

    public bool IsPaid { get; set; }

    public bool RequiresApproval { get; set; }

    public bool IsCarryForward { get; set; }

    public int? MaxCarryForwardDays { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
