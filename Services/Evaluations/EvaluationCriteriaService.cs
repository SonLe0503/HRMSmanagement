using HRManagement.DataAcess;
using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Services.Evaluations
{
    public class EvaluationCriteriaService : IEvaluationCriteriaService
    {
        private readonly IEvaluationCriteriaRepository _criterionRepository;
        private readonly IEvaluationTemplateRepository _templateRepository;

        public EvaluationCriteriaService(
            IEvaluationCriteriaRepository criterionRepository,
            IEvaluationTemplateRepository templateRepository)
        {
            _criterionRepository = criterionRepository;
            _templateRepository = templateRepository;
        }

        public async Task<EvaluationCriterionResponseDto> CreateAsync(int templateId, CreateEvaluationCriterionDto dto)
        {

            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                throw new KeyNotFoundException("Evaluation template not found.");
            }

            if (!template.IsActive)
            {
                throw new InvalidOperationException("Cannot add criteria to an inactive template.");
            }

            if (await _criterionRepository.CriteriaNameExistsInTemplateAsync(templateId, dto.CriteriaName))
            {
                throw new InvalidOperationException("A criterion with this name already exists in this template.");
            }

            var currentTotalWeightage = await _criterionRepository.GetTotalWeightageAsync(templateId);
            if (currentTotalWeightage + dto.Weightage > 100)
            {
                throw new InvalidOperationException(
                    $"Total weightage would exceed 100%. Current total: {currentTotalWeightage}%, " +
                    $"attempting to add: {dto.Weightage}%.");
            }

            var criterion = new EvaluationCriterion
            {
                TemplateId = templateId,
                CriteriaName = dto.CriteriaName,
                CriteriaCategory = dto.CriteriaCategory,
                Description = dto.Description,
                Weightage = dto.Weightage,
                DisplayOrder = dto.DisplayOrder
            };

            await _criterionRepository.AddAsync(criterion);

            return MapToResponseDto(criterion);
        }

        public async Task<IEnumerable<EvaluationCriterionResponseDto>> CreateBulkAsync(int templateId, BulkCreateCriteriaDto dto)
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                throw new KeyNotFoundException("Evaluation template not found.");
            }

            if (!template.IsActive)
            {
                throw new InvalidOperationException("Cannot add criteria to an inactive template.");
            }

            var totalWeightage = dto.Criteria.Sum(c => c.Weightage);
            if (totalWeightage > 100)
            {
                throw new InvalidOperationException($"Total weightage exceeds 100%. Total: {totalWeightage}%");
            }

            var currentWeightage = await _criterionRepository.GetTotalWeightageAsync(templateId);
            if (currentWeightage + totalWeightage > 100)
            {
                throw new InvalidOperationException(
                    $"Total weightage would exceed 100%. Current: {currentWeightage}%, adding: {totalWeightage}%");
            }

            var criteriaNames = dto.Criteria.Select(c => c.CriteriaName).ToList();
            if (criteriaNames.Count != criteriaNames.Distinct().Count())
            {
                throw new InvalidOperationException("Duplicate criteria names found in the request.");
            }

            var results = new List<EvaluationCriterionResponseDto>();

            foreach (var criterionDto in dto.Criteria)
            {
                if (await _criterionRepository.CriteriaNameExistsInTemplateAsync(templateId, criterionDto.CriteriaName))
                {
                    throw new InvalidOperationException($"Criterion '{criterionDto.CriteriaName}' already exists in this template.");
                }

                var criterion = new EvaluationCriterion
                {
                    TemplateId = templateId,
                    CriteriaName = criterionDto.CriteriaName,
                    CriteriaCategory = criterionDto.CriteriaCategory,
                    Description = criterionDto.Description,
                    Weightage = criterionDto.Weightage,
                    DisplayOrder = criterionDto.DisplayOrder
                };

                await _criterionRepository.AddAsync(criterion);
                results.Add(MapToResponseDto(criterion));
            }

            return results;
        }

        public async Task<IEnumerable<EvaluationCriterionListDto>> GetByTemplateIdAsync(int templateId)
        {
            var criteria = await _criterionRepository.GetByTemplateIdAsync(templateId);
            return criteria.Select(MapToListDto).ToList();
        }

        public async Task<EvaluationCriterionResponseDto?> GetByIdAsync(int criteriaId)
        {
            var criterion = await _criterionRepository.GetByIdAsync(criteriaId);
            if (criterion == null)
                return null;

            return MapToResponseDto(criterion);
        }

        public async Task<EvaluationCriterionResponseDto> UpdateAsync(int criteriaId, UpdateEvaluationCriterionDto dto)
        {
            var criterion = await _criterionRepository.GetByIdAsync(criteriaId);
            if (criterion == null)
            {
                throw new KeyNotFoundException("Evaluation criterion not found.");
            }

            var template = await _templateRepository.GetByIdAsync(criterion.TemplateId);
            if (template != null && !template.IsActive)
            {
                throw new InvalidOperationException("Cannot update criteria in an inactive template.");
            }

            if (await _criterionRepository.CriteriaNameExistsInTemplateAsync(
                criterion.TemplateId,
                dto.CriteriaName,
                criteriaId))
            {
                throw new InvalidOperationException("A criterion with this name already exists in this template.");
            }

            var currentTotalWeightage = await _criterionRepository.GetTotalWeightageAsync(
                criterion.TemplateId,
                criteriaId);

            if (currentTotalWeightage + dto.Weightage > 100)
            {
                throw new InvalidOperationException(
                    $"Total weightage would exceed 100%. Current total (excluding this): {currentTotalWeightage}%, " +
                    $"attempting to set: {dto.Weightage}%.");
            }

            criterion.CriteriaName = dto.CriteriaName;
            criterion.CriteriaCategory = dto.CriteriaCategory;
            criterion.Description = dto.Description;
            criterion.Weightage = dto.Weightage;
            criterion.DisplayOrder = dto.DisplayOrder;

            await _criterionRepository.UpdateAsync(criterion);

            return MapToResponseDto(criterion);
        }

        public async Task<bool> DeleteAsync(int criteriaId)
        {
            var criterion = await _criterionRepository.GetByIdAsync(criteriaId);
            if (criterion == null)
                return false;

            var success = await _criterionRepository.DeleteAsync(criteriaId);

            return success;
        }

        private EvaluationCriterionResponseDto MapToResponseDto(EvaluationCriterion criterion)
        {
            return new EvaluationCriterionResponseDto
            {
                CriteriaId = criterion.CriteriaId,
                TemplateId = criterion.TemplateId,
                CriteriaName = criterion.CriteriaName,
                CriteriaCategory = criterion.CriteriaCategory,
                Description = criterion.Description,
                Weightage = criterion.Weightage,
                DisplayOrder = criterion.DisplayOrder
            };
        }

        private EvaluationCriterionListDto MapToListDto(EvaluationCriterion criterion)
        {
            return new EvaluationCriterionListDto
            {
                CriteriaId = criterion.CriteriaId,
                CriteriaName = criterion.CriteriaName,
                CriteriaCategory = criterion.CriteriaCategory,
                Weightage = criterion.Weightage,
                DisplayOrder = criterion.DisplayOrder
            };
        }
    }
}

