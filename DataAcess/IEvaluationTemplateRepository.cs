using HRManagement.Models;

namespace HRManagement.DataAcess
{
    public interface IEvaluationTemplateRepository
    {
        Task<IEnumerable<EvaluationTemplate>> GetAllAsync();
        Task<IEnumerable<EvaluationTemplate>> GetActiveAsync();
        Task<EvaluationTemplate?> GetByIdAsync(int id);
        Task<EvaluationTemplate?> GetByIdWithDetailsAsync(int id);
        Task<EvaluationTemplate> AddAsync(EvaluationTemplate template);
        Task<EvaluationTemplate> UpdateAsync(EvaluationTemplate template);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
