using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class Payslip
{
    public int PayslipId { get; set; }

    public int PayrollRecordId { get; set; }

    public int EmployeeId { get; set; }

    public int PeriodId { get; set; }

    public string PayslipNumber { get; set; } = null!;

    public DateTime GeneratedDate { get; set; }

    public DateTime? ViewedDate { get; set; }

    public bool IsViewed { get; set; }

    public string? Pdfpath { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual PayrollRecord PayrollRecord { get; set; } = null!;

    public virtual PayrollPeriod Period { get; set; } = null!;
}
