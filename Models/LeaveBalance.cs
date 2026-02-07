using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class LeaveBalance
{
    public int BalanceId { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public int Year { get; set; }

    public decimal TotalEntitlement { get; set; }

    public decimal UsedDays { get; set; }

    public decimal? RemainingDays { get; set; }

    public decimal CarriedForward { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual LeaveType LeaveType { get; set; } = null!;
}
