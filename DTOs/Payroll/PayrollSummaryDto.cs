using System.Collections.Generic;

namespace HRManagement.DTOs.Payroll
{
    public class PayrollSummaryDto
    {
        public int PeriodId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalOvertimePay { get; set; }
        public decimal TotalBonuses { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalInsurance { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetPay { get; set; }

        // Phân theo phòng ban
        public List<DepartmentPayrollSummary> ByDepartment { get; set; } = new();
    }

    public class DepartmentPayrollSummary
    {
        public string DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
        public decimal TotalNetPay { get; set; }
    }
}
