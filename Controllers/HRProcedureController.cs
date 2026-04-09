using HRManagement.DTOs;
using HRManagement.Services.HRProceduces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HRProcedureController : ControllerBase
    {
        private readonly IHRProcedureService _procedureService;

        public HRProcedureController(IHRProcedureService procedureService)
        {
            _procedureService = procedureService;
        }

        [HttpPost]
        public async Task<ActionResult<HRProcedureResponseDto>> SubmitProcedure([FromBody] CreateHRProcedureDto createDto)
        {
            try
            {
                var procedure = await _procedureService.SubmitProcedureAsync(createDto);
                return CreatedAtAction(
                    nameof(GetProcedureById),
                    new { id = procedure.ProcedureId },
                    procedure);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
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
        public async Task<ActionResult<IEnumerable<HRProcedureListDto>>> GetAllProcedures()
        {
            var procedures = await _procedureService.GetAllProceduresAsync();
            return Ok(procedures);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HRProcedureResponseDto>> GetProcedureById(int id)
        {
            var procedure = await _procedureService.GetProcedureByIdAsync(id);

            if (procedure == null)
                return NotFound(new { message = "HR procedure not found." });

            return Ok(procedure);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<HRProcedureListDto>>> GetPendingProcedures()
        {
            var procedures = await _procedureService.GetPendingProceduresAsync();
            return Ok(procedures);
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<HRProcedureListDto>>> GetProceduresByEmployee(int employeeId)
        {
            var procedures = await _procedureService.GetProceduresByEmployeeAsync(employeeId);
            return Ok(procedures);
        }

        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<HRProcedureListDto>>> GetProceduresByStatus(string status)
        {
            var procedures = await _procedureService.GetProceduresByStatusAsync(status);
            return Ok(procedures);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<HRProcedureResponseDto>> UpdateProcedure(
            int id,
            [FromBody] UpdateHRProcedureDto updateDto)
        {
            try
            {
                var procedure = await _procedureService.UpdateProcedureAsync(id, updateDto);
                return Ok(procedure);
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

        [HttpPost("{id}/approve")]
        public async Task<ActionResult<HRProcedureResponseDto>> ApproveProcedure(
            int id,
            [FromBody] ApproveHRProcedureDto approveDto)
        {
            try
            {
                var procedure = await _procedureService.ApproveProcedureAsync(id, approveDto);
                return Ok(procedure);
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

        [HttpPost("{id}/reject")]
        public async Task<ActionResult<HRProcedureResponseDto>> RejectProcedure(
            int id,
            [FromBody] RejectHRProcedureDto rejectDto)
        {
            try
            {
                var procedure = await _procedureService.RejectProcedureAsync(id, rejectDto);
                return Ok(procedure);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProcedure(int id)
        {
            var deleted = await _procedureService.DeleteProcedureAsync(id);

            if (!deleted)
                return NotFound(new { message = "HR procedure not found or cannot be deleted." });

            return NoContent();
        }
    }
}
