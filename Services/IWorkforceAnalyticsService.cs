using HRManagement.DTOs.WorkforceAnalytics;

namespace HRManagement.Services
{
    public interface IWorkforceAnalyticsService
    {
        Task<WorkforceAnalyticsResponseDTO> GenerateAnalyticsAsync(WorkforceAnalyticsRequestDTO request);
        Task<bool> SaveViewAsync(SaveWorkforceViewDTO request);
        Task<string> ScheduleReportAsync(ScheduleWorkforceReportDTO request);
        Task<AIInsightsResponseDTO> GetAIInsightsAsync(WorkforceAnalyticsRequestDTO request);
    }
}
