using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class PayrollPolicy
{
    public int PolicyId { get; set; }

    public string PolicyName { get; set; } = null!;

    public string PolicyType { get; set; } = null!;

    public string? Description { get; set; }

    public string? CalculationFormula { get; set; }

    public string? ApplicableEmployeeGroup { get; set; }

    public DateOnly EffectiveStartDate { get; set; }

    public DateOnly? EffectiveEndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? ModifiedBy { get; set; }
}
