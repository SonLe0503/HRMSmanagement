namespace HRManagement.DTOs.WorkforceAnalytics
{
    public class ScheduleWorkforceReportDTO
    {
        public string Frequency { get; set; } = string.Empty; // daily, weekly, monthly
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeOnly? Time { get; set; }
        public List<string> Recipients { get; set; } = new();
        public string Format { get; set; } = "csv";
        public WorkforceAnalyticsRequestDTO Filters { get; set; } = new();
    }
}
