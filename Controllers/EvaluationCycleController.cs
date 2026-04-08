using HRManagement.DTOs;
using HRManagement.Services.Evaluations;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationCycleController : ControllerBase
    {
        private readonly IEvaluationCycleService _evaluationCycleService;

        public EvaluationCycleController(IEvaluationCycleService evaluationCycleService)
        {
            _evaluationCycleService = evaluationCycleService;
        }

        [HttpPost]
        public async Task<ActionResult<EvaluationCycleResponseDto>> CreateCycle([FromBody] CreateEvaluationCycleDto createDto)
        {
            try
            {
                var cycle = await _evaluationCycleService.CreateCycleAsync(createDto);
                return CreatedAtAction(
                    nameof(GetCycleById),
                    new { id = cycle.CycleId },
                    cycle);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/summary")]
        public async Task<ActionResult<EvaluationCycleSummaryDto>> GetCycleSummary(int id)
        {
            try
            {
                var summary = await _evaluationCycleService.GetCycleSummaryAsync(id);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/activate")]
        public async Task<ActionResult<EvaluationCycleResponseDto>> ActivateCycle(int id)
        {
            try
            {
                var cycle = await _evaluationCycleService.ActivateCycleAsync(id);
                return Ok(new
                {
                    message = "Evaluation cycle activated successfully. Notifications have been sent to managers and employees.",
                    data = cycle
                });
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
        public async Task<ActionResult<IEnumerable<EvaluationCycleListDto>>> GetAllCycles()
        {
            
            var cycles = await _evaluationCycleService.GetAllCyclesAsync();
            return Ok(cycles);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<EvaluationCycleListDto>>> GetActiveCycles()
        {         
            var cycles = await _evaluationCycleService.GetActiveCyclesAsync();
            return Ok(cycles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationCycleResponseDto>> GetCycleById(int id)
        {           
            var cycle = await _evaluationCycleService.GetCycleByIdAsync(id);
            if (cycle == null)
                return NotFound(new { message = "Evaluation cycle not found." });
            return Ok(cycle);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EvaluationCycleResponseDto>> UpdateCycle(
            int id,
            [FromBody] UpdateEvaluationCycleDto updateDto)
        {
            try
            {
                var cycle = await _evaluationCycleService.UpdateCycleAsync(id, updateDto);
                return Ok(cycle);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/close")]
        public async Task<ActionResult> CloseCycle(int id, [FromBody] CloseCycleDto closeDto)
        {
                var success = await _evaluationCycleService.CloseCycleAsync(id, closeDto);

                if (!success)
                    return NotFound(new { message = "Evaluation cycle not found or already closed." });

                return Ok(new { message = "Evaluation cycle closed successfully." });

        }
    }
}
