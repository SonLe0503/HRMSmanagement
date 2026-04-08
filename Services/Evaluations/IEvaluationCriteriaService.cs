using HRManagement.DTOs;

namespace HRManagement.Services.Evaluations
{
    public interface IEvaluationCriteriaService
    {
        Task<EvaluationCriterionResponseDto> CreateAsync(int templateId, CreateEvaluationCriterionDto dto);
        Task<IEnumerable<EvaluationCriterionResponseDto>> CreateBulkAsync(int templateId, BulkCreateCriteriaDto dto);
        Task<IEnumerable<EvaluationCriterionListDto>> GetByTemplateIdAsync(int templateId);
        Task<EvaluationCriterionResponseDto?> GetByIdAsync(int criteriaId);
        Task<EvaluationCriterionResponseDto> UpdateAsync(int criteriaId, UpdateEvaluationCriterionDto dto);
        Task<bool> DeleteAsync(int criteriaId);
    }
}

