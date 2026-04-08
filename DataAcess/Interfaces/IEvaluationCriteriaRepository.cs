using HRManagement.Models;

namespace HRManagement.DataAcess
{
    public interface IEvaluationCriteriaRepository
    {
        Task<IEnumerable<EvaluationCriterion>> GetByTemplateIdAsync(int templateId);
        Task<EvaluationCriterion?> GetByIdAsync(int criteriaId);
        Task<EvaluationCriterion> AddAsync(EvaluationCriterion criterion);
        Task<EvaluationCriterion> UpdateAsync(EvaluationCriterion criterion);
        Task<bool> DeleteAsync(int criteriaId);
        Task<bool> ExistsAsync(int criteriaId);
        Task<bool> CriteriaNameExistsInTemplateAsync(int templateId, string criteriaName, int? excludeCriteriaId = null);
        Task<int> GetTotalWeightageAsync(int templateId, int? excludeCriteriaId = null);
    }
}
