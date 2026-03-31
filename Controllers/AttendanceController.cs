using HRManagement.DTOs.Attendances;
using HRManagement.Models;
using HRManagement.Services.Attendances;
using HRManagement.Services.CurrentUsers;
using HRManagement.Services.FaceVerifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    [Authorize]
    public class AttendanceFaceController : ControllerBase
    {
        private readonly IFaceVerificationService _faceVerificationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly HrmsDbContext _context;

        public AttendanceFaceController(
            IFaceVerificationService faceVerificationService,
            ICurrentUserService currentUserService,
            HrmsDbContext context)
        {
            _faceVerificationService = faceVerificationService;
            _currentUserService = currentUserService;
            _context = context;
        }

        [HttpPost("face-checkin")]
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

        [HttpPost("face-checkout")]
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
    }
}