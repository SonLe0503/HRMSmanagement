using System;
using System.Collections.Generic;

namespace HRManagement.Models;

public partial class PayrollRecord
{
    public int PayrollRecordId { get; set; }

    public int EmployeeId { get; set; }

    public int PeriodId { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal WorkingDays { get; set; }

    public decimal ActualWorkingDays { get; set; }

    public decimal TotalAllowances { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal OvertimePay { get; set; }

    public decimal BonusAmount { get; set; }

    public decimal? GrossPay { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal InsuranceAmount { get; set; }

    public decimal? NetPay { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CalculatedDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<PayrollAllowance> PayrollAllowances { get; set; } = new List<PayrollAllowance>();

    public virtual ICollection<PayrollDeduction> PayrollDeductions { get; set; } = new List<PayrollDeduction>();

    public virtual ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();

    public virtual PayrollPeriod Period { get; set; } = null!;
}
