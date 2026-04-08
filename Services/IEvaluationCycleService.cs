using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface IEvaluationCycleService
    {
        Task<EvaluationCycleResponseDto> CreateCycleAsync(CreateEvaluationCycleDto createDto);
        Task<EvaluationCycleSummaryDto> GetCycleSummaryAsync(int cycleId);
        Task<EvaluationCycleResponseDto> ActivateCycleAsync(int cycleId);
        Task<IEnumerable<EvaluationCycleListDto>> GetAllCyclesAsync();
        Task<IEnumerable<EvaluationCycleListDto>> GetActiveCyclesAsync();
        Task<EvaluationCycleResponseDto?> GetCycleByIdAsync(int cycleId);
        Task<EvaluationCycleResponseDto> UpdateCycleAsync(int cycleId, UpdateEvaluationCycleDto updateDto);
        Task<bool> CloseCycleAsync(int cycleId, CloseCycleDto closeDto);
    }
}
