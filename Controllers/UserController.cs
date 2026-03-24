using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly HrmsDbContext _context;
        private readonly IEmailService _emailService;
        public UserController(HrmsDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult>GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(u => new UserResponseDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    EmployeeId = u.EmployeeId,
                    IsActive = u.IsActive,
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                })
                .ToListAsync();
            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserId == id)
                .Select(u => new UserResponseDTO
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    EmployeeId = u.EmployeeId,
                    IsActive = u.IsActive,
                    Roles = u.UserRoles.Select(r => r.Role.RoleName).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            return Ok(user);
        }
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO dto)
        {
            if (dto.EmployeeId <= 0)
                return BadRequest("EmployeeId is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId);

            if (employee == null)
                return BadRequest("Employee does not exist");

            var employeeAlreadyHasUser = await _context.Users
                .AnyAsync(u => u.EmployeeId == dto.EmployeeId);

            if (employeeAlreadyHasUser)
                return BadRequest("This employee already has an account");


            var email = dto.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(x => x.Email == email))
                return BadRequest("Email already exists");

            // Sinh username từ employee name
            var baseUsername = GenerateBaseUsername(employee.FirstName, employee.LastName);

            // fallback nếu tên trống bất thường
            if (string.IsNullOrWhiteSpace(baseUsername))
                baseUsername = $"user{employee.EmployeeId}";

            var username = baseUsername;
            int suffix = 1;

            while (await _context.Users.AnyAsync(x => x.Username == username))
            {
                username = $"{baseUsername}{suffix}";
                suffix++;
            }

            var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 8);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                EmployeeId = dto.EmployeeId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Validate roleIds nếu cần
            if (dto.RoleIds != null && dto.RoleIds.Any())
            {
                var validRoleIds = await _context.Roles
                    .Where(r => dto.RoleIds.Contains(r.RoleId))
                    .Select(r => r.RoleId)
                    .ToListAsync();

                foreach (var roleId in validRoleIds.Distinct())
                {
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = roleId
                    });
                }

                await _context.SaveChangesAsync();
            }

            var body = $@"
        <h3>Tài khoản HR System</h3>
        <p>Xin chào {employee.LastName} {employee.FirstName},</p>
        <p>Tài khoản của bạn đã được tạo thành công:</p>
        <ul>
             <li><b>Username:</b> {username}</li>
             <li><b>Password tạm:</b> {tempPassword}</li>
        </ul>
        <p>Vui lòng đăng nhập và đổi mật khẩu ngay.</p>
    ";

            await _emailService.SendAsync(
                email,
                "Thông tin tài khoản HR System",
                body
            );

            return Ok(new
            {
                message = "User created successfully",
                username = username
            });
        }
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (dto.EmployeeId == null)
                return BadRequest("EmployeeId is required");

            var employeeExists = await _context.Employees
                .AnyAsync(e => e.EmployeeId == dto.EmployeeId);

            if (!employeeExists)
                return BadRequest("Employee does not exist");

            var employeeAlreadyUsed = await _context.Users
                .AnyAsync(u => u.EmployeeId == dto.EmployeeId && u.UserId != id);

            if (employeeAlreadyUsed)
                return BadRequest("This employee is already linked to another account");

            if (user == null)
                return NotFound();

            var oldEmail = user.Email;
            var oldIsActive = user.IsActive;
            var oldRoles = user.UserRoles.Select(x => x.RoleId).ToList();

            // ===== UPDATE USER =====
            user.Email = dto.Email;
            user.IsActive = dto.IsActive;
            user.EmployeeId = dto.EmployeeId;
            user.ModifiedDate = DateTime.UtcNow;

            // ===== UPDATE ROLE =====
            _context.UserRoles.RemoveRange(user.UserRoles);

            foreach (var roleId in dto.RoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = id,
                    RoleId = roleId
                });
            }

            await _context.SaveChangesAsync();

            // ===== KIỂM TRA THAY ĐỔI QUAN TRỌNG =====
            bool emailChanged = oldEmail != dto.Email;
            bool statusChanged = oldIsActive != dto.IsActive;
            bool roleChanged = !oldRoles.OrderBy(x => x)
                                        .SequenceEqual(dto.RoleIds.OrderBy(x => x));

            if (emailChanged || statusChanged || roleChanged)
            {
                var body = $@"
            <h3>Cập nhật tài khoản HR System</h3>
            <p>Xin chào,</p>
            <p>Thông tin tài khoản của bạn vừa được quản trị viên cập nhật:</p>
            <ul>
                {(emailChanged ? $"<li><b>Email mới:</b> {dto.Email}</li>" : "")}
                {(statusChanged ? $"<li><b>Trạng thái:</b> {(dto.IsActive ? "Kích hoạt" : "Ngưng hoạt động")}</li>" : "")}
                {(roleChanged ? "<li><b>Vai trò:</b> Đã được cập nhật</li>" : "")}
            </ul>
            <p>Nếu bạn không nhận ra thay đổi này, vui lòng liên hệ quản trị hệ thống.</p>
            ";

                await _emailService.SendAsync(
                    dto.Email,
                    "Cập nhật thông tin tài khoản HR System",
                    body
                );
            }
            return Ok("User updated successfully");
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            user.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var body = $@"
            <h3>Thông báo trạng thái tài khoản HR System</h3>
            <p>Xin chào,</p>
            <p>Tài khoản của bạn đã bị <b>ngưng hoạt động</b> bởi quản trị viên hệ thống.</p>
            <ul>
            <li><b>Thời gian:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
            </ul>
            <p>Nếu bạn cho rằng đây là nhầm lẫn, vui lòng liên hệ bộ phận quản trị.</p>
            ";

            await _emailService.SendAsync(
                user.Email,
                "Tài khoản HR System đã bị ngưng hoạt động",
                body
            );

            return Ok("User deactivated");
        }
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            user.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var body = $@"
             <h3>Thông báo trạng thái tài khoản HR System</h3>
             <p>Xin chào,</p>
             <p>Tài khoản của bạn đã được <b>kích hoạt lại</b> và có thể đăng nhập hệ thống.</p>
             <ul>
             <li><b>Thời gian:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
             </ul>
             <p>Vui lòng đăng nhập để tiếp tục sử dụng hệ thống.</p>
            ";

            await _emailService.SendAsync(
                user.Email,
                "Tài khoản HR System đã được kích hoạt",
                body
            );

            return Ok("User activated");
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in text)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace('đ', 'd')
                     .Replace('Đ', 'D');
        }

        private static string GenerateBaseUsername(string firstName, string lastName)
        {
            // Ví dụ: LastName + FirstName => "Lê" + "Văn Sơn" => "levanson"
            var raw = $"{lastName}{firstName}";
            raw = RemoveDiacritics(raw);
            raw = raw.Replace(" ", "").ToLowerInvariant();

            return raw;
        }
    }
}
