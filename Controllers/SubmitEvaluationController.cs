using HRManagement.DTOs;
using HRManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubmitEvaluationController : ControllerBase
    {
        private readonly ISubmitEvaluationService _submitEvaluationService;

        public SubmitEvaluationController(ISubmitEvaluationService submitEvaluationService)
        {
            _submitEvaluationService = submitEvaluationService;
        }
        [HttpPost("self")]
        public async Task<ActionResult<EvaluationDetailDto>> SubmitSelfEvaluation([FromBody] SubmitSelfEvaluationDto dto)
        {
            try
            {
                var result = await _submitEvaluationService.SubmitSelfEvaluationAsync(dto);
                return Ok(new
                {
                    message = "Self-evaluation submitted successfully.",
                    data = result
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


        [HttpPost("manager")]
        public async Task<ActionResult<EvaluationDetailDto>> SubmitManagerEvaluation([FromBody] SubmitManagerEvaluationDto dto)
        {
            try
            {
                var result = await _submitEvaluationService.SubmitManagerEvaluationAsync(dto);
                return Ok(new
                {
                    message = "Manager evaluation submitted successfully. Employee has been notified.",
                    data = result
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


        [HttpPost("draft")]
        public async Task<ActionResult<EvaluationDetailDto>> SaveDraft([FromBody] SaveEvaluationDraftDto dto)
        {
            try
            {
                var result = await _submitEvaluationService.SaveEvaluationDraftAsync(dto);
                return Ok(new
                {
                    message = "Evaluation saved as draft.",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("pending/{evaluatorId}")]
        public async Task<ActionResult<IEnumerable<PendingEvaluationDto>>> GetPendingEvaluations(int evaluatorId)
        {
            var evaluations = await _submitEvaluationService.GetPendingEvaluationsForManagerAsync(evaluatorId);
            return Ok(evaluations);
        }

        [HttpGet("{evaluationId}")]
        public async Task<ActionResult<EvaluationDetailDto>> GetEvaluationDetail(int evaluationId)
        {
            var evaluation = await _submitEvaluationService.GetEvaluationDetailAsync(evaluationId);
            if (evaluation == null)
                return NotFound(new { message = "Evaluation not found." });

            return Ok(evaluation);
        }
    }
}
