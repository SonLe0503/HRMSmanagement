using HRManagement.DataAcess;
using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Services.Evaluations
{
    public class EvaluationTemplateService : IEvaluationTemplateService
    {
        private readonly IEvaluationTemplateRepository _evaluationTemplateRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmployeeRepository _employeeRepository;

        public EvaluationTemplateService(IEvaluationTemplateRepository evaluationTemplateRepository, ICurrentUserService currentUserService, IEmployeeRepository employeeRepository)
        {
            _evaluationTemplateRepository = evaluationTemplateRepository; 
            _currentUserService = currentUserService;
            _employeeRepository = employeeRepository;
        }

        public async Task<EvaluationTemplateResponseDto> CreateAsync(CreateEvaluationTemplateDto dto)
        {
            if (await _evaluationTemplateRepository.NameExistsAsync(dto.TemplateName))
            {
                throw new InvalidOperationException("Template name already exists.");
            }

            var template = new EvaluationTemplate
            {
                TemplateName = dto.TemplateName,
                Description = dto.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = _currentUserService.GetCurrentUserId()
            };

            await _evaluationTemplateRepository.AddAsync(template);

            var employee = await _employeeRepository.GetEmployeeByIdAsync(template.CreatedBy.Value);

            return new EvaluationTemplateResponseDto
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                Description = template.Description,
                IsActive = template.IsActive,
                CriteriaCount = template.EvaluationCriteria?.Count ?? 0,
                CreatedDate = template.CreatedDate,
                CreatedBy = template.CreatedBy,
                CreatedByName = employee?.FullName ?? "System"
            };
        }

        public async Task<IEnumerable<EvaluationTemplateListDto>> GetAllAsync()
        {
            var data = await _evaluationTemplateRepository.GetAllAsync();

            return data.Select(x => new EvaluationTemplateListDto
            {
                TemplateId = x.TemplateId,
                TemplateName = x.TemplateName,
                IsActive = x.IsActive,
                CriteriaCount = x.EvaluationCriteria?.Count ?? 0
            });
        }

        public async Task<IEnumerable<EvaluationTemplateListDto>> GetActiveAsync()
        {
            var data = await _evaluationTemplateRepository.GetActiveAsync();

            return data.Select(x => new EvaluationTemplateListDto
            {
                TemplateId = x.TemplateId,
                TemplateName = x.TemplateName,
                IsActive = x.IsActive,
                CriteriaCount = x.EvaluationCriteria?.Count ?? 0
            });
        }

        public async Task<EvaluationTemplateResponseDto?> GetByIdAsync(int templateId)
        {
            var template = await _evaluationTemplateRepository.GetByIdWithDetailsAsync(templateId);
            if (template == null) return null;

            var createdById = template.CreatedBy;
            if (!createdById.HasValue)
            {
                throw new InvalidOperationException("CreatedBy field is null.");
            }

            var employee = await _employeeRepository.GetEmployeeByIdAsync(createdById.Value);
            return new EvaluationTemplateResponseDto
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                Description = template.Description,
                IsActive = template.IsActive,
                CriteriaCount = template.EvaluationCriteria?.Count ?? 0,
                CreatedDate = template.CreatedDate,
                CreatedBy = template.CreatedBy,
                CreatedByName = employee?.FullName ?? "System"
            };
        }

        public async Task<EvaluationTemplateResponseDto> UpdateAsync(int templateId, UpdateEvaluationTemplateDto dto)
        {
            var template = await _evaluationTemplateRepository.GetByIdAsync(templateId);
            if (template == null)
                throw new KeyNotFoundException("Template not found");

            if (await _evaluationTemplateRepository.NameExistsAsync(dto.TemplateName, templateId))
            {
                throw new InvalidOperationException("Template name already exists.");
            }

            template.TemplateName = dto.TemplateName;
            template.Description = dto.Description;

            var createdById = template.CreatedBy;
            if (!createdById.HasValue)
            {
                throw new InvalidOperationException("CreatedBy field is null.");
            }

            var employee = await _employeeRepository.GetEmployeeByIdAsync(createdById.Value);
            await _evaluationTemplateRepository.UpdateAsync(template);

            return new EvaluationTemplateResponseDto
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                Description = template.Description,
                IsActive = template.IsActive,
                CriteriaCount = template.EvaluationCriteria?.Count ?? 0,
                CreatedDate = template.CreatedDate,
                CreatedBy = template.CreatedBy,
                CreatedByName = employee?.FullName ?? "System"
            };
        }

        public async Task<bool> DeactivateAsync(int templateId)
        {
            var template = await _evaluationTemplateRepository.GetByIdAsync(templateId);
            if (template == null) return false;

            if (!template.IsActive) return false;

            template.IsActive = false;

            await _evaluationTemplateRepository.UpdateAsync(template);

            return true;
        }

        public async Task<bool> ActivateAsync(int templateId)
        {
            var template = await _evaluationTemplateRepository.GetByIdAsync(templateId);
            if (template == null) return false;

            if (template.IsActive) return false;

            template.IsActive = true;

            await _evaluationTemplateRepository.UpdateAsync(template);

            return true;
        }
    }
}

