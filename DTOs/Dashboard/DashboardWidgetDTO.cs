namespace HRManagement.DTOs.Dashboard
{
    public class DashboardWidgetDTO
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string WidgetType { get; set; } = "number"; // number, chart, table, list, calendar
        public string TimePeriod { get; set; } = "Current";
        public DateTime LastUpdated { get; set; }
        public bool CanRefresh { get; set; } = true;
        public bool CanRemove { get; set; } = false;
        public bool CanResize { get; set; } = false;
        public bool HasError { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public string? ViewDetailsUrl { get; set; }
        public object? Data { get; set; }
    }
}
