using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Hrprocedure
{
    public int ProcedureId { get; set; }

    public string ProcedureNumber { get; set; } = null!;

    public int EmployeeId { get; set; }

    public string ProcedureType { get; set; } = null!;

    public DateOnly EffectiveDate { get; set; }

    public int? NewDepartmentId { get; set; }

    public int? NewPositionId { get; set; }

    public decimal? NewSalary { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = null!;

    public string? RejectionReason { get; set; }

    public DateTime SubmittedDate { get; set; }

    public int SubmittedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public int? ApprovedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Department? NewDepartment { get; set; }

    public virtual Position? NewPosition { get; set; }
}
