using HRManagement.DTOs.Attendances;
using HRManagement.Models;
using HRManagement.Services.Attendances;
using HRManagement.Services.CurrentUsers;
using HRManagement.Services.FaceVerifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

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

            // Verify Location if configured in SystemSettings
            var officeLatSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "OfficeLatitude");
            var officeLngSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "OfficeLongitude");
            var radiusSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "AttendanceAllowedRadius");

            if (officeLatSetting != null && officeLngSetting != null && radiusSetting != null && 
                !string.IsNullOrEmpty(officeLatSetting.SettingValue) && !string.IsNullOrEmpty(officeLngSetting.SettingValue))
            {
                if (double.TryParse(officeLatSetting.SettingValue, out var officeLat) && 
                    double.TryParse(officeLngSetting.SettingValue, out var officeLng) && 
                    double.TryParse(radiusSetting.SettingValue, out var radius))
                {
                    if (request.Latitude.HasValue && request.Longitude.HasValue)
                    {
                        var distance = CalculateDistance(request.Latitude.Value, request.Longitude.Value, officeLat, officeLng);
                        if (distance > radius)
                        {
                            return BadRequest(new { 
                                message = $"Vĩ trí check-in của bạn ({distance:F1}m) nằm ngoài phạm vi văn phòng cho phép ({radius}m). Chi tiết: Tọa độ của bạn: {request.Latitude},{request.Longitude}. Tọa độ văn phòng: {officeLat},{officeLng}" 
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = "Hệ thống yêu cầu quyền truy cập vị trí GPS để thực hiện check-in." });
                    }
                }
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var nowTimeSpan = DateTime.Now.TimeOfDay;

            var shiftAssignment = await _context.ShiftAssignments
                .Include(sa => sa.Shift)
                .FirstOrDefaultAsync(sa => sa.EmployeeId == employeeId && sa.AssignmentDate == today && sa.Status == "Active");

            if (shiftAssignment != null && shiftAssignment.Shift != null)
            {
                var shift = shiftAssignment.Shift;
                var startTimeSpan = shift.StartTime.ToTimeSpan();
                
                var allowedCheckInStart = startTimeSpan.Add(TimeSpan.FromMinutes(-(shift.EarlyCheckInMinutes ?? 0)));
                var allowedCheckInEnd = startTimeSpan.Add(TimeSpan.FromMinutes(shift.LatestCheckInMinutes ?? 0));

                if (nowTimeSpan < allowedCheckInStart)
                {
                    return BadRequest(new { message = $"Chưa đến giờ check-in. Giờ check-in sớm nhất là {allowedCheckInStart:hh\\:mm}." });
                }
                if (nowTimeSpan > allowedCheckInEnd)
                {
                    return BadRequest(new { message = $"Đã quá giờ check-in cho phép ({allowedCheckInEnd:hh\\:mm})." });
                }
            }

            var attendance = await _context.AttendanceRecords
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.AttendanceDate == today);

            if (attendance != null && attendance.CheckInTime != null)
            {
                return Ok(new
                {
                    message = "Bạn đã check-in trước đó. Hệ thống giữ nguyên thời gian đầu tiên.",
                    attendance.AttendanceId,
                    attendance.EmployeeId,
                    attendance.AttendanceDate,
                    attendance.CheckInTime,
                    verifyResult.ConfidenceScore,
                    verifyResult.ThresholdUsed
                });
            }

            int lateMinutes = 0;
            string status = "Present";
            if (shiftAssignment != null && shiftAssignment.Shift != null)
            {
                var shift = shiftAssignment.Shift;
                var startTimeSpan = shift.StartTime.ToTimeSpan();
                var lateTimeSpan = nowTimeSpan - startTimeSpan;
                
                if (lateTimeSpan.TotalMinutes > (shift.LateGraceMinutes ?? 0))
                {
                    lateMinutes = (int)Math.Floor(lateTimeSpan.TotalMinutes);
                    status = "Late";
                }
            }

            if (attendance == null)
            {
                attendance = new AttendanceRecord
                {
                    EmployeeId = employeeId,
                    AttendanceDate = today,
                    CheckInTime = DateTime.Now,
                    Status = status,
                    LateMinutes = lateMinutes,
                    ShiftId = shiftAssignment?.ShiftId,
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
                attendance.Status = status;
                attendance.LateMinutes = lateMinutes;
                attendance.ShiftId = shiftAssignment?.ShiftId;
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

            // Verify Location if configured in SystemSettings
            var officeLatSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "OfficeLatitude");
            var officeLngSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "OfficeLongitude");
            var radiusSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == "AttendanceAllowedRadius");

            if (officeLatSetting != null && officeLngSetting != null && radiusSetting != null && 
                !string.IsNullOrEmpty(officeLatSetting.SettingValue) && !string.IsNullOrEmpty(officeLngSetting.SettingValue))
            {
                if (double.TryParse(officeLatSetting.SettingValue, out var officeLat) && 
                    double.TryParse(officeLngSetting.SettingValue, out var officeLng) && 
                    double.TryParse(radiusSetting.SettingValue, out var radius))
                {
                    if (request.Latitude.HasValue && request.Longitude.HasValue)
                    {
                        var distance = CalculateDistance(request.Latitude.Value, request.Longitude.Value, officeLat, officeLng);
                        if (distance > radius)
                        {
                            return BadRequest(new { 
                                message = $"Vị trí check-out của bạn ({distance:F1}m) nằm ngoài phạm vi văn phòng cho phép ({radius}m). Chi tiết: Tọa độ của bạn: {request.Latitude},{request.Longitude}. Tọa độ văn phòng: {officeLat},{officeLng}" 
                            });
                        }
                    }
                    else
                    {
                        return BadRequest(new { message = "Hệ thống yêu cầu quyền truy cập vị trí GPS để thực hiện check-out." });
                    }
                }
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var nowTimeSpan = DateTime.Now.TimeOfDay;

            var attendance = await _context.AttendanceRecords
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.AttendanceDate == today);

            if (attendance == null || attendance.CheckInTime == null)
            {
                return BadRequest(new
                {
                    message = "Bạn chưa check-in hôm nay."
                });
            }

            var shiftAssignment = await _context.ShiftAssignments
                .Include(sa => sa.Shift)
                .FirstOrDefaultAsync(sa => sa.EmployeeId == employeeId && sa.AssignmentDate == today && sa.Status == "Active");

            int earlyLeaveMinutes = 0;
            if (shiftAssignment != null && shiftAssignment.Shift != null)
            {
                var shift = shiftAssignment.Shift;
                var endTimeSpan = shift.EndTime.ToTimeSpan();
                
                var allowedCheckOutStart = endTimeSpan.Add(TimeSpan.FromMinutes(-(shift.EarliestCheckOutMinutes ?? 0)));
                var allowedCheckOutEnd = endTimeSpan.Add(TimeSpan.FromMinutes(shift.LatestCheckOutMinutes ?? 0));

                if (nowTimeSpan < allowedCheckOutStart)
                {
                    return BadRequest(new { message = $"Chưa tới giờ được phép check-out. Sớm nhất là {allowedCheckOutStart:hh\\:mm}." });
                }
                if (nowTimeSpan > allowedCheckOutEnd)
                {
                    return BadRequest(new { message = $"Đã quá giờ check-out ({allowedCheckOutEnd:hh\\:mm})." });
                }
                
                var earlyTimeSpan = endTimeSpan - nowTimeSpan;
                if (earlyTimeSpan.TotalMinutes > 0)
                {
                    earlyLeaveMinutes = (int)Math.Floor(earlyTimeSpan.TotalMinutes);
                }
            }

            attendance.CheckOutTime = DateTime.Now;
            attendance.EarlyLeaveMinutes = earlyLeaveMinutes;
            attendance.CheckOutVerificationMethod = "FACE_AI";
            attendance.CheckOutVerified = true;
            attendance.Location = request.Location;
            attendance.Remarks = string.IsNullOrWhiteSpace(request.Remarks)
                ? (string.IsNullOrWhiteSpace(attendance.Remarks) ? "Check-out bằng nhận diện khuôn mặt" : attendance.Remarks)
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

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // metres
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // in metres
        }
    }
}