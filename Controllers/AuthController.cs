using HRManagement.DTOs;
using HRManagement.DTOs.Auths;
using HRManagement.Models;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly HrmsDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(HrmsDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Username không tồn tại" });
            }
            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Tài khoản đã bị vô hiệu hóa" });
            }
            bool verifyPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!verifyPassword)
            {
                return Unauthorized(new { message = "Mật khẩu không đúng" });
            }
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();


            var claims = new List<Claim>
            {
                  new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                  new Claim(ClaimTypes.Name, user.Username),

            };

            foreach (var userRole in user.UserRoles)
            {
                claims.Add(new Claim(
                    ClaimTypes.Role,
                    userRole.Role.RoleName
                ));
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            return Ok(new { message = "Đăng nhập thành công", Token = jwtToken });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                return BadRequest(new { message = "Mật khẩu hiện tại là bắt buộc." });

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "Mật khẩu mới là bắt buộc." });

            if (string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
                return BadRequest(new { message = "Xác nhận mật khẩu mới là bắt buộc." });

            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(new { message = "Mật khẩu mới và xác nhận mật khẩu không khớp." });

            if (dto.NewPassword.Length < 6)
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại." });

            if (!user.IsActive)
                return Unauthorized(new { message = "Tài khoản đã bị vô hiệu hóa." });

            bool verifyCurrentPassword = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!verifyCurrentPassword)
                return BadRequest(new { message = "Mật khẩu hiện tại không đúng." });

            bool isSameAsOldPassword = BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash);
            if (isSameAsOldPassword)
                return BadRequest(new { message = "Mật khẩu mới không được trùng với mật khẩu hiện tại." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.EmailOrUsername))
                return BadRequest(new { message = "Email hoặc username là bắt buộc." });

            var input = dto.EmailOrUsername.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == input || u.Username.ToLower() == input);

            // Vì lý do bảo mật: không nói rõ user có tồn tại hay không
            if (user == null || !user.IsActive)
            {
                return Ok(new
                {
                    message = "Nếu tài khoản tồn tại, mã OTP đặt lại mật khẩu đã được gửi qua email."
                });
            }

            var otp = GenerateOtp();

            user.PasswordResetOtp = otp;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(5);

            await _context.SaveChangesAsync();

            var body = $@"
        <h3>Đặt lại mật khẩu HR System</h3>
        <p>Xin chào {user.Username},</p>
        <p>Mã OTP để đặt lại mật khẩu của bạn là:</p>
        <h2>{otp}</h2>
        <p>Mã này có hiệu lực trong <b>5 phút</b>.</p>
        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
    ";

            await _emailService.SendAsync(
                user.Email,
                "Mã OTP đặt lại mật khẩu HR System",
                body
            );

            return Ok(new
            {
                message = "Nếu tài khoản tồn tại, mã OTP đặt lại mật khẩu đã được gửi qua email."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });

            if (string.IsNullOrWhiteSpace(dto.EmailOrUsername))
                return BadRequest(new { message = "Email hoặc username là bắt buộc." });

            if (string.IsNullOrWhiteSpace(dto.Otp))
                return BadRequest(new { message = "OTP là bắt buộc." });

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "Mật khẩu mới là bắt buộc." });

            if (string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
                return BadRequest(new { message = "Xác nhận mật khẩu mới là bắt buộc." });

            if (dto.NewPassword != dto.ConfirmNewPassword)
                return BadRequest(new { message = "Mật khẩu mới và xác nhận mật khẩu không khớp." });

            if (dto.NewPassword.Length < 6)
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });

            var input = dto.EmailOrUsername.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == input || u.Username.ToLower() == input);

            if (user == null)
                return BadRequest(new { message = "Thông tin đặt lại mật khẩu không hợp lệ." });

            if (!user.IsActive)
                return BadRequest(new { message = "Tài khoản đã bị vô hiệu hóa." });

            if (string.IsNullOrWhiteSpace(user.PasswordResetOtp) || user.PasswordResetOtpExpiry == null)
                return BadRequest(new { message = "OTP không hợp lệ hoặc chưa được tạo." });

            if (user.PasswordResetOtp != dto.Otp.Trim())
                return BadRequest(new { message = "OTP không đúng." });

            if (user.PasswordResetOtpExpiry < DateTime.UtcNow)
                return BadRequest(new { message = "OTP đã hết hạn." });

            bool isSameAsOldPassword = BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash);
            if (isSameAsOldPassword)
                return BadRequest(new { message = "Mật khẩu mới không được trùng với mật khẩu hiện tại." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            // Xóa OTP sau khi dùng
            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lại mật khẩu thành công." });
        }

        private static string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
