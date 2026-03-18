using HRManagement.DTOs.Attendances;
using HRManagement.Models;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly HrmsDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AttendanceController(IAttendanceService attendanceService, HrmsDbContext context, ICurrentUserService currentUserService)
        {
            _attendanceService = attendanceService;
            _context = context;
            _currentUserService = currentUserService;
        }




        [HttpPost("check-in")]
        [Authorize]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto dto)
        {
            try
            {
                var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();
                var result = await _attendanceService.CheckInAsync(employeeId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpPost("check-out")]
        [Authorize]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutRequestDto dto)
        {
            try
            {
                var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();
                var result = await _attendanceService.CheckOutAsync(employeeId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpGet("my-today")]
        [Authorize]
        public async Task<IActionResult> GetMyToday()
        {
            try
            {
                var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();
                var result = await _attendanceService.GetMyTodayAsync(employeeId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpGet("my-history")]
        [Authorize]
        public async Task<IActionResult> GetMyHistory(
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate)
        {
            try
            {
                if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                    return BadRequest(new { message = "Từ ngày không được lớn hơn đến ngày." });

                var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();
                var result = await _attendanceService.GetMyHistoryAsync(employeeId, fromDate, toDate);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        // =========================
        // MANAGEMENT ATTENDANCE
        // =========================

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetByDate([FromQuery] DateOnly date)
        {
            try
            {
                var result = await _attendanceService.GetAttendanceByDateAsync(date);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> Search(
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate,
            [FromQuery] int? employeeId,
            [FromQuery] string? status)
        {
            try
            {
                if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
                    return BadRequest(new { message = "Từ ngày không được lớn hơn đến ngày." });

                if (employeeId.HasValue && employeeId.Value <= 0)
                    return BadRequest(new { message = "EmployeeId không hợp lệ." });

                if (status != null && string.IsNullOrWhiteSpace(status))
                    return BadRequest(new { message = "Status không hợp lệ." });

                var result = await _attendanceService.SearchAttendanceAsync(fromDate, toDate, employeeId, status);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpGet("{employeeId:int}/{date}")]
        [Authorize]
        public async Task<IActionResult> GetDetail(int employeeId, DateOnly date)
        {
            try
            {
                if (employeeId <= 0)
                    return BadRequest(new { message = "EmployeeId không hợp lệ." });

                var result = await _attendanceService.GetAttendanceDetailAsync(employeeId, date);

                if (result == null)
                    return NotFound(new { message = "Không tìm thấy dữ liệu chấm công." });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpPut("manual-adjust/{attendanceId:int}")]
        [Authorize]
        public async Task<IActionResult> ManualAdjust(int attendanceId, [FromBody] ManualAdjustAttendanceDto dto)
        {
            try
            {
                if (attendanceId <= 0)
                    return BadRequest(new { message = "AttendanceId không hợp lệ." });

                var approverId = _currentUserService.GetCurrentUserId();
                var result = await _attendanceService.ManualAdjustAttendanceAsync(attendanceId, approverId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpPost("manual-create")]
        [Authorize]
        public async Task<IActionResult> ManualCreate([FromBody] ManualCreateAttendanceDto dto)
        {
            try
            {
                var approverId = _currentUserService.GetCurrentUserId();
                var result = await _attendanceService.ManualCreateAttendanceAsync(approverId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpPut("{attendanceId:int}/lock")]
        [Authorize]
        public async Task<IActionResult> Lock(int attendanceId)
        {
            try
            {
                if (attendanceId <= 0)
                    return BadRequest(new { message = "AttendanceId không hợp lệ." });

                var userId = _currentUserService.GetCurrentUserId();
                await _attendanceService.LockAttendanceAsync(attendanceId, userId);

                return Ok(new { message = "Khóa chấm công thành công." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpPut("{attendanceId:int}/unlock")]
        [Authorize]
        public async Task<IActionResult> Unlock(int attendanceId)
        {
            try
            {
                if (attendanceId <= 0)
                    return BadRequest(new { message = "AttendanceId không hợp lệ." });

                var userId = _currentUserService.GetCurrentUserId();
                await _attendanceService.UnlockAttendanceAsync(attendanceId, userId);

                return Ok(new { message = "Mở khóa chấm công thành công." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpGet("logs")]
        [Authorize]
        public async Task<IActionResult> GetLogs([FromQuery] int employeeId, [FromQuery] DateOnly date)
        {
            try
            {
                if (employeeId <= 0)
                    return BadRequest(new { message = "EmployeeId không hợp lệ." });

                var result = await _attendanceService.GetLogsAsync(employeeId, date);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }
    }
}
