using HRManagement.Models;

namespace HRManagement.DataAcess
{
    public interface IEvaluationCycleRepository
    {
        Task<IEnumerable<EvaluationCycle>> GetAllAsync();
        Task<IEnumerable<EvaluationCycle>> GetActiveAsync();
        Task<IEnumerable<EvaluationCycle>> GetCompletedAsync();
        Task<EvaluationCycle?> GetByIdWithDetailsAsync(int cycleId);
        Task<EvaluationCycle?> GetByIdAsync(int cycleId);
        Task<EvaluationCycle> AddAsync(EvaluationCycle cycle);
        Task<EvaluationCycle> UpdateAsync(EvaluationCycle cycle);
        Task<bool> ExistsAsync(int cycleId);
        Task<bool> HasOverlappingCycleAsync(DateOnly periodStart, DateOnly periodEnd, int? excludeCycleId = null);
    }
}
