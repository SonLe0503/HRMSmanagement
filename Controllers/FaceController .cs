using HRManagement.DTOs.Attendances;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaceController : Controller
    {
        private readonly IFaceVerificationService _faceVerificationService;
        private readonly ICurrentUserService _currentUserService;

        public FaceController(IFaceVerificationService faceVerificationService, ICurrentUserService currentUserService)
        {
            _faceVerificationService = faceVerificationService;
            _currentUserService = currentUserService;
        }

        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> RegisterFace([FromBody] FaceRegisterRequestDto dto)
        {
            try
            {
                var employeeId = await _currentUserService.GetCurrentEmployeeIdAsync();

                if (string.IsNullOrWhiteSpace(dto.ReferenceImageBase64))
                    return BadRequest(new { message = "Ảnh khuôn mặt không được để trống." });

                var imagePath = await _faceVerificationService.RegisterFaceAsync(employeeId, dto.ReferenceImageBase64);

                return Ok(new
                {
                    message = "Đăng ký khuôn mặt thành công.",
                    imagePath
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }
    }
}
