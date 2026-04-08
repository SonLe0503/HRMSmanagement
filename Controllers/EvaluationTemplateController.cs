using HRManagement.DTOs;
using HRManagement.Services.Evaluations;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationTemplateController : ControllerBase
    {
        private readonly IEvaluationTemplateService _service;

        public EvaluationTemplateController(IEvaluationTemplateService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<EvaluationTemplateResponseDto>> Create([FromBody] CreateEvaluationTemplateDto dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.TemplateId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EvaluationTemplateListDto>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<EvaluationTemplateListDto>>> GetActive()
        {
            var result = await _service.GetActiveAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationTemplateResponseDto>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound(new { message = "Evaluation template not found." });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EvaluationTemplateResponseDto>> Update(
            int id,
            [FromBody] UpdateEvaluationTemplateDto dto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, dto);
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

        [HttpPatch("{id}/deactivate")]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var success = await _service.DeactivateAsync(id);

                if (!success)
                    return NotFound(new { message = "Template not found or already inactive." });

                return Ok(new { message = "Template deactivated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/activate")]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var success = await _service.ActivateAsync(id);

                if (!success)
                    return NotFound(new { message = "Template not found or already active." });

                return Ok(new { message = "Template activated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
