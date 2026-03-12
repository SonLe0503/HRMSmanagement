namespace HRManagement.DTOs.Dashboard
{
    public class DashboardResponseDTO
    {
        public string Role { get; set; } = string.Empty;
        public string HomeScreenCode { get; set; } = "SR-18";
        public DateTime LastRefreshed { get; set; }
        public int RefreshIntervalSeconds { get; set; } = 300;
        public bool CanCustomizeLayout { get; set; } = true;
        public List<DashboardWidgetDTO> Widgets { get; set; } = new();
    }
}
