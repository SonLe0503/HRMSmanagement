using HRManagement.DTOs.Attendances;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/face")]
    [Authorize]
    public class FaceController : ControllerBase
    {
        private readonly IFaceVerificationService _faceVerificationService;
        private readonly ICurrentUserService _currentUserService;

        public FaceController(
            IFaceVerificationService faceVerificationService,
            ICurrentUserService currentUserService)
        {
            _faceVerificationService = faceVerificationService;
            _currentUserService = currentUserService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterFace([FromBody] FaceRegisterRequestDto request)
        {
            var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();

            if (employeeId <= 0)
                return Unauthorized(new { message = "Không xác định được nhân viên hiện tại." });

            if (request == null || string.IsNullOrWhiteSpace(request.ReferenceImageBase64))
                return BadRequest(new { message = "Ảnh khuôn mặt là bắt buộc." });

            try
            {
                var imagePath = await _faceVerificationService.RegisterFaceAsync(
                    employeeId,
                    request.ReferenceImageBase64
                );

                return Ok(new
                {
                    message = "Đăng ký khuôn mặt thành công.",
                    referenceImagePath = imagePath
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyFace([FromBody] CheckInRequestDto request)
        {
            var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();

            if (employeeId <= 0)
                return Unauthorized(new { message = "Không xác định được nhân viên hiện tại." });

            if (request == null || string.IsNullOrWhiteSpace(request.FaceImageBase64))
                return BadRequest(new { message = "Ảnh xác thực là bắt buộc." });

            var result = await _faceVerificationService.VerifyAsync(
                employeeId,
                request.FaceImageBase64,
                "CheckIn",
                request.DeviceInfo,
                request.IpAddress,
                request.Location
            );

            if (!result.IsMatch)
            {
                return Unauthorized(new
                {
                    message = "Xác thực khuôn mặt thất bại.",
                    result.ConfidenceScore,
                    result.ThresholdUsed,
                    result.FailureReason
                });
            }

            return Ok(new
            {
                message = "Xác thực khuôn mặt thành công.",
                result.ConfidenceScore,
                result.ThresholdUsed,
                result.CapturedImagePath
            });
        }
    }
}