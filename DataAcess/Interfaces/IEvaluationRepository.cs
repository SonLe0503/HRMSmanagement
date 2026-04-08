using HRManagement.Models;

namespace HRManagement.DataAcess
{
    public interface IEvaluationRepository
    {
        Task<IEnumerable<Evaluation>> GetAllAsync();
        Task<IEnumerable<Evaluation>> GetByCycleIdAsync(int cycleId);
        Task<IEnumerable<Evaluation>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<Evaluation>> GetByEvaluatorIdAsync(int evaluatorId);
        Task<Evaluation?> GetByIdWithDetailsAsync(int evaluationId);
        Task<Evaluation?> GetByIdAsync(int evaluationId);
        Task<Evaluation> AddAsync(Evaluation evaluation);
        Task<Evaluation> UpdateAsync(Evaluation evaluation);
        Task<bool> ExistsAsync(int evaluationId);
        Task<bool> EmployeeHasEvaluationInCycleAsync(int cycleId, int employeeId);
        Task<int> GetEvaluationCountByCycleAsync(int cycleId);
        Task<int> GetAssignedEvaluatorCountAsync(int cycleId);
        Task<IEnumerable<Evaluation>> GetPendingEvaluationsByEvaluatorAsync(int evaluatorId);
    }
}
