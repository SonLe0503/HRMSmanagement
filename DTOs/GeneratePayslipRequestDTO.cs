namespace HRManagement.DTOs
{
    public class GeneratePayslipRequestDTO
    {
        public int PeriodId { get; set; }

        public string DeliveryMethod { get; set; } // System, Email, Both
    }
    public class PayslipGenerationSummaryDTO
    {
        public int TotalEmployees { get; set; }

        public int GeneratedPayslips { get; set; }

        public int FailedPayslips { get; set; }

        public string Message { get; set; }
    }
}
