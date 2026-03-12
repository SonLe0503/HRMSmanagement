namespace HRManagement.DTOs.Dashboard
{
    public class DashboardLayoutUpdateDTO
    {
        public List<DashboardWidgetLayoutItemDTO> Widgets { get; set; } = new();
    }

    public class DashboardWidgetLayoutItemDTO
    {
        public string WidgetKey { get; set; } = string.Empty;
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
