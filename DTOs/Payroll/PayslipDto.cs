using System;

namespace HRManagement.DTOs.Payroll
{
    public class PayslipDto
    {
        public int PayslipId { get; set; }
        public string PayslipNumber { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string PositionName { get; set; }
        public int PeriodId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateTime GeneratedDate { get; set; }
        public bool IsViewed { get; set; }
        public DateTime? ViewedDate { get; set; }
        public string? PdfPath { get; set; }

        // Snapshot số liệu
        public decimal GrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
    }
}
