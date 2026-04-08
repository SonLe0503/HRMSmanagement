using HRManagement.DTOs;

namespace HRManagement.Services.Evaluations
{
    public interface IEvaluationTemplateService
    {
        Task<EvaluationTemplateResponseDto> CreateAsync(CreateEvaluationTemplateDto dto);

        Task<IEnumerable<EvaluationTemplateListDto>> GetAllAsync();
        Task<IEnumerable<EvaluationTemplateListDto>> GetActiveAsync();

        Task<EvaluationTemplateResponseDto?> GetByIdAsync(int templateId);

        Task<EvaluationTemplateResponseDto> UpdateAsync(int templateId, UpdateEvaluationTemplateDto dto);

        Task<bool> DeactivateAsync(int templateId);
        Task<bool> ActivateAsync(int templateId);
    }
}

