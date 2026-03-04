using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [Authorize(Roles = "Manager")]
    [ApiController]
    [Route("api/[controller]")]
    public class ApprovalsController : ControllerBase
    {
        private readonly IApprovalService _service;

        public ApprovalsController(IApprovalService service)
        {
            _service = service;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var employeeId = int.Parse(User.FindFirst("EmployeeId")!.Value);

            var result = await _service.GetPendingRequestsAsync(employeeId);

            if (!result.Any())
                return Ok(new { message = "No pending approvals" });

            return Ok(result);
        }
    }
}
