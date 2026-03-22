using HRManagement.DTOs.LeaveBalance;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveBalancesController : ControllerBase
    {
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveBalancesController(ILeaveBalanceService leaveBalanceService)
        {
            _leaveBalanceService = leaveBalanceService;
        }

        [HttpGet("my-balance")]
        public async Task<IActionResult> GetMyBalance()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveBalanceService.GetMyLeaveBalanceAsync(userId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    MessageCode = result.MessageCode,
                    Message = result.Message
                });
            }

            return Ok(result.Data);
        }
        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustBalance([FromBody] AdjustLeaveBalanceDTO dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _leaveBalanceService.AdjustLeaveBalanceAsync(userId, dto);

            if (!result.Success)
                return BadRequest(new { result.MessageCode, result.Message });

            return Ok(new { result.MessageCode, result.Message });
        }
    }
}