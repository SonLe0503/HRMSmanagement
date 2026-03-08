namespace HRManagement.DTOs
{
    public class PayrollSummaryDTO
    {
        public int TotalEmployees { get; set; }

        public decimal TotalGrossPay { get; set; }

        public decimal TotalDeductions { get; set; }

        public decimal TotalNetPay { get; set; }

        public decimal AverageNetPay { get; set; }

        public decimal MaxNetPay { get; set; }

        public decimal MinNetPay { get; set; }
    }
}
