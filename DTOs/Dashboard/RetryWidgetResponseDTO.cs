namespace HRManagement.DTOs.Dashboard
{
    public class RetryWidgetResponseDTO
    {
        public string WidgetKey { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DashboardWidgetDTO? Widget { get; set; }
    }
}
