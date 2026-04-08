namespace HRManagement.DTOs.WorkforceAnalytics
{
    public class SaveWorkforceViewDTO
    {
        public string ViewName { get; set; } = string.Empty;
        public WorkforceAnalyticsRequestDTO Filters { get; set; } = new();
    }
}
