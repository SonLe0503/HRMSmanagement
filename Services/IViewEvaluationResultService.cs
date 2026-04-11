using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface IViewEvaluationResultService
    {
        Task<IEnumerable<EvaluationResultListDto>> GetAvailableResultsForEmployeeAsync(int employeeId);
        Task<EvaluationResultDto> GetEvaluationResultAsync(int evaluationId);
        Task<EvaluationChartDataDto> GetEvaluationChartDataAsync(int evaluationId);
        Task<PerformanceSummaryDto> GetPerformanceSummaryAsync(int employeeId);
        Task<EvaluationResultDto> AcknowledgeEvaluationAsync(AcknowledgeEvaluationDto dto);
        Task<bool> RequestReviewAsync(RequestReviewDto dto);
    }
}
