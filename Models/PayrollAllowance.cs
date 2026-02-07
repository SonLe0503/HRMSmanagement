using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class PayrollAllowance
{
    public int AllowanceId { get; set; }

    public int PayrollRecordId { get; set; }

    public string AllowanceType { get; set; } = null!;

    public string AllowanceName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public virtual PayrollRecord PayrollRecord { get; set; } = null!;
}
