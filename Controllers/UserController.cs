using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if (await _context.Users.AnyAsync(x =>
                x.Username == dto.Username || x.Email == dto.Email))
                return BadRequest("Username or Email already exists");

            var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 8);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                EmployeeId = dto.EmployeeId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            foreach (var roleId in dto.RoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = roleId
                });
            }

            await _context.SaveChangesAsync();
            var body = $@"
                <h3>Tài khoản HR System</h3>
                <p>Xin chào,</p>
                <p>Tài khoản của bạn đã được tạo thành công:</p>
                <ul>
                     <li><b>Username:</b> {dto.Username}</li>
                     <li><b>Password tạm:</b> {tempPassword}</li>
                </ul>
                <p>Vui lòng đăng nhập và đổi mật khẩu ngay.</p>
            ";

            await _emailService.SendAsync(
                dto.Email,
                "Thông tin tài khoản HR System",
                body
            );

            return Ok("User created successfully");
        }
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == id);

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
    }
}
