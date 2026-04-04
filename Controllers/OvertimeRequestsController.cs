using HRManagement.DTOs.LeaveRequest;
using HRManagement.DTOs.OvertimeRequest;
using HRManagement.Models;
using HRManagement.Services.Overtimes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OvertimeRequestsController : ControllerBase
    {
        private readonly IOvertimeRequestService _overtimeRequestService;

        public OvertimeRequestsController(IOvertimeRequestService overtimeRequestService)
        {
            _overtimeRequestService = overtimeRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOvertimeRequest([FromBody] CreateOvertimeRequestDTO dto)
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

            var result = await _overtimeRequestService.CreateOvertimeRequestAsync(userId, dto);

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
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveOvertimeRequestDTO dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _overtimeRequestService.ApproveOvertimeRequestAsync(userId, id, dto);

            if (!result.Success)
                return BadRequest(new { result.MessageCode, result.Message });

            return Ok(new { result.MessageCode, result.Message });
        }
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectOvertimeRequestDTO dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _overtimeRequestService.RejectOvertimeRequestAsync(userId, id, dto);

            if (!result.Success)
                return BadRequest(new { result.MessageCode, result.Message });

            return Ok(new { result.MessageCode, result.Message });
        }
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelOvertimeRequestDTO dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _overtimeRequestService.CancelOvertimeRequestAsync(userId, id, dto);

            if (!result.Success)
                return BadRequest(new { result.MessageCode, result.Message });

            return Ok(new { result.MessageCode, result.Message });
        }
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _overtimeRequestService.GetMyOvertimeRequestsAsync(userId);

            if (!result.Success)
                return BadRequest(new { result.MessageCode, result.Message });

            return Ok(result.Data);
        }
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingOvertimeRequests()
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

            var result = await _overtimeRequestService.GetPendingOvertimeRequestsAsync(userId);

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