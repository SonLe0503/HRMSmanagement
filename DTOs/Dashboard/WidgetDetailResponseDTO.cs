namespace HRManagement.DTOs.Dashboard
{
    public class WidgetDetailResponseDTO
    {
        public string WidgetKey { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public Dictionary<string, string> Filters { get; set; } = new();
    }
}
