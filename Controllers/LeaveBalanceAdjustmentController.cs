using HRManagement.DTOs;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/leave-balances")]
    [Authorize(Roles = "HR Staff")]
    public class LeaveBalanceAdjustmentController : ControllerBase
    {
        private readonly ILeaveBalanceAdjustmentService _service;

        public LeaveBalanceAdjustmentController(ILeaveBalanceAdjustmentService service)
        {
            _service = service;
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> Adjust([FromBody] AdjustLeaveBalanceDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    messageCode = "MSG-48",
                    message = "Please fill in all required fields (leave type, adjustment amount, reason).",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            var result = await _service.AdjustAsync(dto, User);

            if (!result.Success)
            {
                return result.MessageCode switch
                {
                    "MSG-48" => BadRequest(result),
                    "MSG-49" => BadRequest(result),
                    "MSG-46" => NotFound(result),
                    "EMP-404" => NotFound(result),
                    "LEAVE_TYPE-404" => NotFound(result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }
    }
}