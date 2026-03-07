using HRManagement.DTOs;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveBalanceController : ControllerBase
    {
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveBalanceController(ILeaveBalanceService leaveBalanceService)
        {
            _leaveBalanceService = leaveBalanceService;
        }

        [HttpGet("my-balance")]
        public async Task<IActionResult> GetMyLeaveBalance()
        {
            var employeeIdClaim = User.FindFirst("employeeId")?.Value;

            if (employeeIdClaim == null)
                return Unauthorized();

            int employeeId = int.Parse(employeeIdClaim);

            var balances = await _leaveBalanceService.GetLeaveBalanceAsync(employeeId);

            return Ok(balances);
        }

        [HttpPost("adjust")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> AdjustLeaveBalance([FromBody] AdjustLeaveBalanceDTO dto)
        {
            var userIdClaim = User.FindFirst("userId")?.Value;

            if (userIdClaim == null)
                return Unauthorized();

            int hrUserId = int.Parse(userIdClaim);

            await _leaveBalanceService.AdjustLeaveBalanceAsync(dto, hrUserId);

            return Ok(new { message = "Leave balance adjusted successfully" });
        }

    }
}
