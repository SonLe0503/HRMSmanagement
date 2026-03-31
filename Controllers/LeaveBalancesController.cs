using HRManagement.DTOs.LeaveBalance;
using HRManagement.Services.Leaves;
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

        [HttpGet]
        public async Task<IActionResult> GetAllLeaveBalances()
        {
            var result = await _leaveBalanceService.GetAllLeaveBalancesAsync();

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

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var result = await _leaveBalanceService.GetLeaveBalancesByEmployeeAsync(employeeId);

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

        [HttpPost]
        public async Task<IActionResult> CreateLeaveBalance([FromBody] CreateLeaveBalanceDTO dto)
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

            var result = await _leaveBalanceService.CreateLeaveBalanceAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    MessageCode = result.MessageCode,
                    Message = result.Message
                });
            }

            return Ok(new
            {
                MessageCode = result.MessageCode,
                Message = result.Message
            });
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustBalance([FromBody] AdjustLeaveBalanceDTO dto)
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

            var result = await _leaveBalanceService.AdjustLeaveBalanceAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    MessageCode = result.MessageCode,
                    Message = result.Message
                });
            }

            return Ok(new
            {
                MessageCode = result.MessageCode,
                Message = result.Message
            });
        }
    }
}