using HRManagement.DTOs.Attendances;
using HRManagement.Models;
using HRManagement.Services.Attendances;
using HRManagement.Services.CurrentUsers;
using HRManagement.Services.FaceVerifications;
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
        private readonly IFaceVerificationService _faceVerificationService;

        public AttendanceController(IAttendanceService attendanceService, HrmsDbContext context, ICurrentUserService currentUserService, IFaceVerificationService faceVerificationService)
        {
            _attendanceService = attendanceService;
            _context = context;
            _currentUserService = currentUserService;
            _faceVerificationService = faceVerificationService;
        }




        [HttpPost("checkin")]
        public async Task<IActionResult> FaceCheckIn([FromBody] CheckInRequestDto request)
        {
            var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();

            if (employeeId <= 0)
                return Unauthorized(new { message = "Không xác định được nhân viên hiện tại." });

            if (request == null || string.IsNullOrWhiteSpace(request.FaceImageBase64))
                return BadRequest(new { message = "Ảnh check-in là bắt buộc." });

            var verifyResult = await _faceVerificationService.VerifyAsync(
                employeeId,
                request.FaceImageBase64,
                "CheckIn",
                request.DeviceInfo,
                request.IpAddress,
                request.Location
            );

            if (!verifyResult.IsMatch)
            {
                return Unauthorized(new
                {
                    message = "Check-in thất bại do khuôn mặt không khớp.",
                    verifyResult.ConfidenceScore,
                    verifyResult.ThresholdUsed,
                    verifyResult.FailureReason
                });
            }

            var today = DateOnly.FromDateTime(DateTime.Now);

            var attendance = await _context.AttendanceRecords
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.AttendanceDate == today);

            if (attendance != null && attendance.CheckInTime != null)
            {
                return BadRequest(new
                {
                    message = "Bạn đã check-in hôm nay rồi."
                });
            }

            if (attendance == null)
            {
                attendance = new AttendanceRecord
                {
                    EmployeeId = employeeId,
                    AttendanceDate = today,
                    CheckInTime = DateTime.Now,
                    Status = "Present",
                    CheckInVerificationMethod = "FACE_AI",
                    CheckInVerified = true,
                    Location = request.Location,
                    Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                        ? "Check-in bằng nhận diện khuôn mặt"
                        : request.Remarks,
                    CreatedDate = DateTime.Now
                };

                _context.AttendanceRecords.Add(attendance);
            }
            else
            {
                attendance.CheckInTime = DateTime.Now;
                attendance.Status = "Present";
                attendance.CheckInVerificationMethod = "FACE_AI";
                attendance.CheckInVerified = true;
                attendance.Location = request.Location;
                attendance.Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                    ? "Check-in bằng nhận diện khuôn mặt"
                    : request.Remarks;
                attendance.ModifiedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Check-in thành công.",
                attendance.AttendanceId,
                attendance.EmployeeId,
                attendance.AttendanceDate,
                attendance.CheckInTime,
                verifyResult.ConfidenceScore,
                verifyResult.ThresholdUsed
            });
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> FaceCheckOut([FromBody] CheckInRequestDto request)
        {
            var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();

            if (employeeId <= 0)
                return Unauthorized(new { message = "Không xác định được nhân viên hiện tại." });

            if (request == null || string.IsNullOrWhiteSpace(request.FaceImageBase64))
                return BadRequest(new { message = "Ảnh check-out là bắt buộc." });

            var verifyResult = await _faceVerificationService.VerifyAsync(
                employeeId,
                request.FaceImageBase64,
                "CheckOut",
                request.DeviceInfo,
                request.IpAddress,
                request.Location
            );

            if (!verifyResult.IsMatch)
            {
                return Unauthorized(new
                {
                    message = "Check-out thất bại do khuôn mặt không khớp.",
                    verifyResult.ConfidenceScore,
                    verifyResult.ThresholdUsed,
                    verifyResult.FailureReason
                });
            }

            var today = DateOnly.FromDateTime(DateTime.Now);

            var attendance = await _context.AttendanceRecords
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.AttendanceDate == today);

            if (attendance == null || attendance.CheckInTime == null)
            {
                return BadRequest(new
                {
                    message = "Bạn chưa check-in hôm nay."
                });
            }

            if (attendance.CheckOutTime != null)
            {
                return BadRequest(new
                {
                    message = "Bạn đã check-out hôm nay rồi."
                });
            }

            attendance.CheckOutTime = DateTime.Now;
            attendance.CheckOutVerificationMethod = "FACE_AI";
            attendance.CheckOutVerified = true;
            attendance.Location = request.Location;
            attendance.Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                ? attendance.Remarks
                : request.Remarks;
            attendance.ModifiedDate = DateTime.Now;

            if (attendance.CheckInTime.HasValue)
            {
                attendance.WorkingHours = (decimal)(attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Check-out thành công.",
                attendance.AttendanceId,
                attendance.EmployeeId,
                attendance.AttendanceDate,
                attendance.CheckInTime,
                attendance.CheckOutTime,
                attendance.WorkingHours,
                verifyResult.ConfidenceScore,
                verifyResult.ThresholdUsed
            });
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