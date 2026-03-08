using HRManagement.DTOs;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OvertimeRequestController : Controller
    {
        private readonly IOvertimeRequestService _service;

        public OvertimeRequestController(IOvertimeRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOvertimeRequestDTO dto)
        {
            var employeeId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.CreateAsync(dto,employeeId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var employeeIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (employeeIdClaim == null)
                return Unauthorized();

            int employeeId = int.Parse(employeeIdClaim);

            var result = await _service.CancelAsync(id, employeeId);

            return StatusCode(result.StatusCode, result);
        }
    }
}

