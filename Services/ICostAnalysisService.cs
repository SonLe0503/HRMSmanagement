using HRManagement.DTOs.CostAnalysis;

namespace HRManagement.Services
{
    public interface ICostAnalysisService
    {
        Task<CostAnalysisResponseDTO> GenerateCostAnalysisAsync(CostAnalysisRequestDTO request);
        Task<CostScenarioResponseDTO> CreateScenarioAsync(CostScenarioDTO request);
        Task<string> SetCostAlertAsync(CostAlertDTO request);
    }
}