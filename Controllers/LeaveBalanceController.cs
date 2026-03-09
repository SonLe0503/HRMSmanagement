using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/leave-balances")]
    [Authorize(Roles = "Employee")]
    public class LeaveBalanceController : ControllerBase
    {
        private readonly ILeaveBalanceService _leaveBalanceService;

        public LeaveBalanceController(ILeaveBalanceService leaveBalanceService)
        {
            _leaveBalanceService = leaveBalanceService;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyLeaveBalance()
        {
            var result = await _leaveBalanceService.GetMyLeaveBalanceAsync(User);

            if (!result.Success)
            {
                return result.MessageCode switch
                {
                    "AUTH-01" => Unauthorized(result),
                    "AUTH-02" => NotFound(result),
                    "MSG-46" => NotFound(result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }
    }
}