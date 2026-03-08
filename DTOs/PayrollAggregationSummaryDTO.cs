namespace HRManagement.DTOs
{
    public class PayrollAggregationSummaryDTO
    {
        public int TotalEmployeesProcessed { get; set; }
        public int RecordsCreated { get; set; }
        public int ExistingRecords { get; set; }
        public string Message { get; set; }
    }
}
