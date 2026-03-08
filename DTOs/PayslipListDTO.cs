namespace HRManagement.DTOs
{
    public class PayslipListDTO
    {
        public int PayslipId { get; set; }

        public int PeriodId { get; set; }

        public DateTime GeneratedDate { get; set; }

        public decimal NetPay { get; set; }

        public string Status { get; set; }
    }
    public class PayslipDetailDTO
    {
        public int PayslipId { get; set; }

        public int EmployeeId { get; set; }

        public decimal BaseSalary { get; set; }

        public decimal TotalAllowances { get; set; }

        public decimal OvertimePay { get; set; }

        public decimal BonusAmount { get; set; }

        public decimal TotalDeductions { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal InsuranceAmount { get; set; }

        public decimal NetPay { get; set; }
    }
}
