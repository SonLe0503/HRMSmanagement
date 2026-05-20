using HRManagement.DTOs;
using HRManagement.DTOs.Auths;
using HRManagement.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var (success, error, token) = await _authService.LoginAsync(request);
            if (!success)
                return Unauthorized(new { message = error });

            return Ok(new { message = "Đăng nhập thành công", Token = token });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            if (dto is null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var (success, error, notFound) = await _authService.ChangePasswordAsync(userId, dto);
            if (notFound)
                return NotFound(new { message = error });
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO dto)
        {
            if (dto is null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            var (success, error) = await _authService.ForgotPasswordAsync(dto);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Nếu tài khoản tồn tại, mã OTP đặt lại mật khẩu đã được gửi qua email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            if (dto is null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            var (success, error) = await _authService.ResetPasswordAsync(dto);
            if (!success)
                return BadRequest(new { message = error });

            return Ok(new { message = "Đặt lại mật khẩu thành công." });
        }

        [Authorize]
        [HttpGet("ping")]
        public IActionResult Ping() => Ok(new { message = "Session is valid." });
    }
}
