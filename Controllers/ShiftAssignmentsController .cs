using HRManagement.DataAcess;
using HRManagement.DTOs.Attendances;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftAssignmentsController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IShiftAssignmentService _shiftAssignmentService;
        public ShiftAssignmentsController(IAttendanceService attendanceService, ICurrentUserService currentUserService, IShiftAssignmentService shiftAssignmentService)
        {
            _attendanceService = attendanceService;
            _currentUserService = currentUserService;
            _shiftAssignmentService = shiftAssignmentService;
        }

        [HttpPost("assign")]
        [Authorize]
        public async Task<IActionResult> AssignShift([FromBody] AssignShiftDto dto)
        {
            try
            {
                int managerId = _currentUserService.GetCurrentUserId();

                await _attendanceService.AssignShiftAsync(managerId, dto);

                return Ok(new
                {
                    success = true,
                    message = "Phân ca thành công."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi hệ thống.",
                    detail = ex.Message // nếu production thì nên bỏ detail
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShiftAssignments(
           [FromQuery] DateOnly? date,
           [FromQuery] int? employeeId,
           [FromQuery] string? status)
        {
            var data = await _shiftAssignmentService.GetShiftAssignmentsAsync(date, employeeId, status);

            return Ok(data);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetShiftAssignmentById(int id)
        {
            var data = await _shiftAssignmentService.GetShiftAssignmentByIdAsync(id);

            if (data == null)
                return NotFound(new { message = "Không tìm thấy phân ca." });

            return Ok(data);
        }

        [HttpGet("my-schedule")]
        [Authorize]
        public async Task<IActionResult> GetMySchedule(
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate)
        {
            var employeeId = _currentUserService.GetCurrentUserId();

            var data = await _shiftAssignmentService.GetMyShiftAssignmentsAsync(employeeId, fromDate, toDate);

            return Ok(data);
        }
    }
}
