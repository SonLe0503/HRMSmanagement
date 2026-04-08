using HRManagement.DTOs;
using HRManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/evaluationtemplate/{templateId}/criteria")]
    public class EvaluationCriteriaController : ControllerBase
    {
        private readonly IEvaluationCriteriaService _criterionService;

        public EvaluationCriteriaController(IEvaluationCriteriaService criterionService)
        {
            _criterionService = criterionService;
        }

        [HttpPost]
        public async Task<ActionResult<EvaluationCriterionResponseDto>> CreateCriterion(
            int templateId,
            [FromBody] CreateEvaluationCriterionDto dto)
        {
            try
            {
                var result = await _criterionService.CreateAsync(templateId, dto);
                return CreatedAtAction(
                    nameof(GetCriterionById),
                    new { templateId, id = result.CriteriaId },
                    result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<EvaluationCriterionResponseDto>>> CreateCriteriaBulk(
            int templateId,
            [FromBody] BulkCreateCriteriaDto dto)
        {
            try
            {
                var results = await _criterionService.CreateBulkAsync(templateId, dto);
                return Ok(results);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EvaluationCriterionListDto>>> GetCriteriaByTemplate(int templateId)
        {
                var criteria = await _criterionService.GetByTemplateIdAsync(templateId);
                return Ok(criteria);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationCriterionResponseDto>> GetCriterionById(int templateId, int id)
        {
                var criterion = await _criterionService.GetByIdAsync(id);
                if (criterion == null)
                    return NotFound(new { message = "Evaluation criterion not found." });

                if (criterion.TemplateId != templateId)
                    return NotFound(new { message = "Criterion does not belong to this template." });

                return Ok(criterion);

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EvaluationCriterionResponseDto>> UpdateCriterion(
            int templateId,
            int id,
            [FromBody] UpdateEvaluationCriterionDto dto)
        {
            try
            {
                var result = await _criterionService.UpdateAsync(id, dto);

                if (result.TemplateId != templateId)
                    return NotFound(new { message = "Criterion does not belong to this template." });

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCriterion(int templateId, int id)
        {

                var success = await _criterionService.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Evaluation criterion not found." });

                return Ok(new { message = "Criterion deleted successfully." });
        }
    }
}
