namespace HRManagement.DTOs
{
    public class PayrollCalculationSummaryDTO
    {
        public int TotalEmployees { get; set; }
        public int CalculatedEmployees { get; set; }
        public int ErrorEmployees { get; set; }
        public decimal TotalPayrollAmount { get; set; }
        public string Message { get; set; }
    }
}
