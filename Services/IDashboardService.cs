using HRManagement.DTOs.Dashboard;

namespace HRManagement.Services
{
    public interface IDashboardService
    {
        Task<DashboardResponseDTO> GetDashboardAsync();
        Task<DashboardResponseDTO> RefreshDashboardAsync(RefreshDashboardDTO request);
        Task<bool> SaveLayoutAsync(DashboardLayoutUpdateDTO request);
        Task<RetryWidgetResponseDTO> RetryWidgetAsync(string widgetKey);
        Task<WidgetDetailResponseDTO> GetWidgetDetailsAsync(string widgetKey);
    }
}
