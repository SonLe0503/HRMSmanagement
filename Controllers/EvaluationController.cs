using HRManagement.DTOs;
using HRManagement.Services.Evaluations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationsController : ControllerBase
    {
        private readonly IEvaluationService _evaluationService;

        public EvaluationsController(IEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        [HttpPost("assign")]
        public async Task<ActionResult<AssignmentResultDto>> AssignEvaluators([FromBody] AssignEvaluatorsDto dto)
        {
            try
            {
                var result = await _evaluationService.AssignEvaluatorsAsync(dto);
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

        [HttpPost("auto-assign")]
        public async Task<ActionResult<AssignmentResultDto>> AutoAssignEvaluators([FromBody] AutoAssignEvaluatorsDto dto)
        {
            try
            {
                var result = await _evaluationService.AutoAssignEvaluatorsAsync(dto);
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

        [HttpPost("bulk-assign-department")]
        public async Task<ActionResult<AssignmentResultDto>> BulkAssignByDepartment([FromBody] BulkAssignByDepartmentDto dto)
        {
            try
            {
                var result = await _evaluationService.BulkAssignByDepartmentAsync(dto);
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

        [HttpGet("cycle/{cycleId}/preview")]
        public async Task<ActionResult<List<AssignmentPreviewDto>>> GetAssignmentPreview(int cycleId)
        {
            try
            {
                var preview = await _evaluationService.GetAssignmentPreviewAsync(cycleId);
                return Ok(preview);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

        }

        [HttpGet("cycle/{cycleId}")]
        public async Task<ActionResult<IEnumerable<EvaluationListDto>>> GetEvaluationsByCycle(int cycleId)
        {

                var evaluations = await _evaluationService.GetEvaluationsByCycleAsync(cycleId);
                return Ok(evaluations);

        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<EvaluationListDto>>> GetEvaluationsByEmployee(int employeeId)
        {

                var evaluations = await _evaluationService.GetEvaluationsByEmployeeAsync(employeeId);
                return Ok(evaluations);

        }

        [HttpGet("evaluator/{evaluatorId}")]
        public async Task<ActionResult<IEnumerable<EvaluationListDto>>> GetEvaluationsByEvaluator(int evaluatorId)
        {

                var evaluations = await _evaluationService.GetEvaluationsByEvaluatorAsync(evaluatorId);
                return Ok(evaluations);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EvaluationResponseDto>> GetEvaluationById(int id)
        {

                var evaluation = await _evaluationService.GetEvaluationByIdAsync(id);
                if (evaluation == null)
                    return NotFound(new { message = "Evaluation not found." });

                return Ok(evaluation);
        }

        [HttpPatch("{id}/evaluator")]
        public async Task<ActionResult<EvaluationResponseDto>> ChangeEvaluator(
            int id,
            [FromBody] ChangeEvaluatorDto dto)
        {
            try
            {
                var evaluation = await _evaluationService.ChangeEvaluatorAsync(id, dto);
                return Ok(evaluation);
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
