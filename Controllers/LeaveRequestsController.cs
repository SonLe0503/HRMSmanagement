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

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveRequestService.CreateLeaveRequestAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    MessageCode = result.MessageCode,
                    Message = result.Message,
                    Data = result.Data
                });
            }

            return Ok(new
            {
                MessageCode = result.MessageCode,
                Message = result.Message,
                Data = result.Data
            });
        }
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveLeaveRequest(int id, [FromBody] ApproveLeaveRequestDTO dto)
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

            var result = await _leaveRequestService.ApproveLeaveRequestAsync(userId, id, dto);

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

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectLeaveRequest(int id, [FromBody] RejectLeaveRequestDTO dto)
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

            var result = await _leaveRequestService.RejectLeaveRequestAsync(userId, id, dto);

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
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelLeaveRequest(int id, [FromBody] CancelLeaveRequestDTO dto)
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

            var result = await _leaveRequestService.CancelLeaveRequestAsync(userId, id, dto);

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
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyLeaveRequests()
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

            var result = await _leaveRequestService.GetMyLeaveRequestsAsync(userId);

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
                Data = result.Data
            });
        }
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingLeaveRequests()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new
                {
                    MessageCode = "MSG-106",
                    Message = "Access Denied."
                });
            }

            var result = await _leaveRequestService.GetPendingLeaveRequestsAsync(userId);

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
                Data = result.Data
            });
        }
    }
}