using HRManagement.DTOs.ResignationRequest;
using HRManagement.Services.Resignations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResignationRequestsController : ControllerBase
    {
        private readonly IResignationRequestService _service;

        public ResignationRequestsController(IResignationRequestService service)
        {
            _service = service;
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateResignationRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { messageCode = "RR-00", message = "Dữ liệu không hợp lệ." });

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _service.CreateAsync(userId.Value, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _service.GetMyRequestsAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _service.CancelAsync(userId.Value, id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _service.GetPendingForManagerAsync(userId.Value);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveResignationRequestDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _service.ApproveAsync(userId.Value, id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectResignationRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { messageCode = "RR-00", message = "Vui lòng nhập lý do từ chối." });

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _service.RejectAsync(userId.Value, id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
