using HRManagement.DTOs.Shifts;
using HRManagement.Services.CurrentUsers;
using HRManagement.Services.Shifts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftsController : Controller
    {
        private readonly IShiftService _shiftService;
        private readonly ICurrentUserService _currentUserService;

        public ShiftsController(IShiftService shiftService, ICurrentUserService currentUserService)
        {
            _shiftService = shiftService;
            _currentUserService = currentUserService;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto dto)
        {
            var currentUserId = _currentUserService.GetCurrentUserId();

            var result = await _shiftService.CreateShiftAsync(currentUserId, dto);

            return Ok(new
            {
                message = "Tạo ca làm việc thành công.",
                data = result
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllShifts([FromQuery] bool? isActive)
        {
            var result = await _shiftService.GetAllShiftsAsync(isActive);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetShiftById(int id)
        {
            var result = await _shiftService.GetShiftByIdAsync(id);

            if (result == null)
                return NotFound(new { message = "Không tìm thấy ca làm việc." });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateShiftDto dto)
        {
            var result = await _shiftService.UpdateShiftAsync(id, dto);

            return Ok(new
            {
                message = "Cập nhật ca làm việc thành công.",
                data = result
            });
        }

        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleShiftActive(int id)
        {
            await _shiftService.ToggleShiftActiveAsync(id);

            return Ok(new
            {
                message = "Cập nhật trạng thái ca làm việc thành công."
            });
        }
    }
}
