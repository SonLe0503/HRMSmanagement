using HRManagement.DTOs;
using HRManagement.Services;
using HRManagement.Services.Evaluations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationResultController : ControllerBase
    {
        private readonly IViewEvaluationResultService _viewResultService;

        public EvaluationResultController(IViewEvaluationResultService viewResultService)
        {
            _viewResultService = viewResultService;
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<EvaluationResultListDto>>> GetAvailableResults(int employeeId)
        {

            var results = await _viewResultService.GetAvailableResultsForEmployeeAsync(employeeId);
            return Ok(results);

        }

        [HttpGet("{evaluationId}")]
        public async Task<ActionResult<EvaluationResultDto>> GetEvaluationResult(int evaluationId)
        {
            try
            {
                var result = await _viewResultService.GetEvaluationResultAsync(evaluationId);
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

        [HttpGet("{evaluationId}/chart")]
        public async Task<ActionResult<EvaluationChartDataDto>> GetChartData(int evaluationId)
        {
            try
            {
                var chartData = await _viewResultService.GetEvaluationChartDataAsync(evaluationId);
                return Ok(chartData);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        [HttpGet("employee/{employeeId}/summary")]
        public async Task<ActionResult<PerformanceSummaryDto>> GetPerformanceSummary(int employeeId)
        {

            var summary = await _viewResultService.GetPerformanceSummaryAsync(employeeId);
            return Ok(summary);

        }

        [HttpPost("acknowledge")]
        public async Task<ActionResult<EvaluationResultDto>> AcknowledgeEvaluation([FromBody] AcknowledgeEvaluationDto dto)
        {
            try
            {
                var result = await _viewResultService.AcknowledgeEvaluationAsync(dto);
                return Ok(new
                {
                    message = "Evaluation acknowledged successfully. Manager and HR have been notified.",
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

        [HttpPost("appeal")]
        public async Task<ActionResult> RequestReview([FromBody] RequestReviewDto dto)
        {
            try
            {
                var success = await _viewResultService.RequestReviewAsync(dto);
                if (!success)
                    return BadRequest(new { message = "Failed to submit appeal request." });

                return Ok(new
                {
                    message = "Appeal request submitted successfully. HR has been notified and will review your concerns."
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
    }
}
