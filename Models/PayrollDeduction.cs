using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class PayrollDeduction
{
    public int DeductionId { get; set; }

    public int PayrollRecordId { get; set; }

    public string DeductionType { get; set; } = null!;

    public string DeductionName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public virtual PayrollRecord PayrollRecord { get; set; } = null!;
}
