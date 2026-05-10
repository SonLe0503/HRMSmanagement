using System;
using System.Collections.Generic;

namespace HRManagement.DTOs.Payroll
{
    public class PayrollRecordDto
    {
        public int PayrollRecordId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public int PeriodId { get; set; }
        public string PeriodStatus { get; set; }

        // Ngày công
        public decimal WorkingDays { get; set; }
        public decimal ActualWorkingDays { get; set; }

        // Thu nhập
        public decimal BaseSalary { get; set; }
        public decimal SalariedAmount { get; set; }     // Lương theo ngày công thực tế
        public decimal TotalAllowances { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal GrossPay { get; set; }

        // Khấu trừ
        public decimal InsuranceAmount { get; set; }    // BHXH + BHYT + BHTN
        public decimal TaxAmount { get; set; }          // Thuế TNCN
        public decimal TotalDeductions { get; set; }

        // Thực lĩnh
        public decimal NetPay { get; set; }

        public string Status { get; set; }
        public DateTime? CalculatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public List<PayrollAllowanceDto> Allowances { get; set; } = new();
        public List<PayrollDeductionDto> Deductions { get; set; } = new();
    }
}
