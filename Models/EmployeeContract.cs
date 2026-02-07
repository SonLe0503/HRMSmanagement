using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class EmployeeContract
{
    public int ContractId { get; set; }

    public int EmployeeId { get; set; }

    public string ContractNumber { get; set; } = null!;

    public string ContractType { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal SalaryAmount { get; set; }

    public string? Terms { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
