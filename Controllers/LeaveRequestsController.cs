using System.Security.Claims;
using HRManagement.DTOs.LeaveRequest;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveRequestsController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return null;

            if (!int.TryParse(userIdClaim, out int userId))
                return null;

            return userId;
        }

        [HttpGet("my-balance")]
        public async Task<IActionResult> GetMyLeaveBalance()
        {
            var result = await _leaveRequestService.GetMyLeaveBalanceAsync();

            if (!result.Any())
            {
                return NotFound(new
                {
                    MessageCode = "MSG-104",
                    Message = "Leave balance not found."
                });
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    MessageCode = "MSG-23",
                    Message = "Please fill in all required fields."
                });
            }

            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveRequestService.CreateLeaveRequestAsync(userId.Value, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveLeaveRequest(int id, [FromBody] ApproveLeaveRequestDTO dto)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveRequestService.ApproveLeaveRequestAsync(userId.Value, id, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectLeaveRequest(int id, [FromBody] RejectLeaveRequestDTO dto)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveRequestService.RejectLeaveRequestAsync(userId.Value, id, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelLeaveRequest(int id, [FromBody] CancelLeaveRequestDTO dto)
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveRequestService.CancelLeaveRequestAsync(userId.Value, id, dto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyLeaveRequests()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveRequestService.GetMyLeaveRequestsAsync(userId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("team-calendar")]
        public async Task<IActionResult> GetTeamLeaveCalendar()
        {
            var userId = GetUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveRequestService.GetTeamLeaveCalendarAsync(userId.Value);

            if (!result.Success)
            {
                if (result.MessageCode == "MSG-47")
                {
                    return Ok(new
                    {
                        MessageCode = result.MessageCode,
                        Message = result.Message,
                        Data = new List<TeamLeaveCalendarDTO>()
                    });
                }

                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}