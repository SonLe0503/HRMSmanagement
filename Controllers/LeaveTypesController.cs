using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/leave-types")]
    [Authorize]
    public class LeaveTypesController : ControllerBase
    {
        private readonly ILeaveTypeService _leaveTypeService;

        public LeaveTypesController(ILeaveTypeService leaveTypeService)
        {
            _leaveTypeService = leaveTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveLeaveTypes()
        {
            var result = await _leaveTypeService.GetActiveLeaveTypesAsync();
            return Ok(result);
        }
    }
}