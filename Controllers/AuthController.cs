using HRManagement.DTOs;
using HRManagement.Models;
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

        public AuthController(HrmsDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            if (user.EmployeeId.HasValue)
            {
                claims.Add(new Claim("employeeId", user.EmployeeId.Value.ToString()));
            }

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
    }
}
