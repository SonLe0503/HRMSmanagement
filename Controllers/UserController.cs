using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services.Emails;
using HRManagement.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly HrmsDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IUserAccountValidationService _userAccountValidationService;

        public UserController(
            HrmsDbContext context,
            IEmailService emailService,
            IUserAccountValidationService userAccountValidationService)
        {
            _context = context;
            _emailService = emailService;
            _userAccountValidationService = userAccountValidationService;
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
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

            var roleValidation = await _userAccountValidationService.ValidateRoleSelectionAsync(dto.RoleIds);
            if (!roleValidation.IsValid)
                return BadRequest(roleValidation.ErrorMessage);

            var managerValidationError = await _userAccountValidationService.ValidateApprovalRouteAsync(employee, roleValidation.RoleNames);
            if (managerValidationError != null)
                return BadRequest(managerValidationError);

            var employeeAlreadyHasUser = await _context.Users
                .AnyAsync(u => u.EmployeeId == dto.EmployeeId);

            if (employeeAlreadyHasUser)
                return BadRequest("This employee already has an account");

            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(x => x.Email == email))
                return BadRequest("Email already exists");

            var baseUsername = _userAccountValidationService.GenerateBaseUsername(employee.FirstName, employee.LastName);
            if (string.IsNullOrWhiteSpace(baseUsername))
                baseUsername = $"user{employee.EmployeeId}";

            var username = baseUsername;
            var suffix = 1;

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

            foreach (var role in roleValidation.ValidRoles)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = role.RoleId
                });
            }

            if (roleValidation.ValidRoles.Any())
                await _context.SaveChangesAsync();

            var body = $@"
        <h3>Tai khoan HR System</h3>
        <p>Xin chao {employee.LastName} {employee.FirstName},</p>
        <p>Tai khoan cua ban da duoc tao thanh cong:</p>
        <ul>
             <li><b>Username:</b> {username}</li>
             <li><b>Password tam:</b> {tempPassword}</li>
        </ul>
        <p>Vui long dang nhap va doi mat khau ngay.</p>
    ";

            await _emailService.SendAsync(
                email,
                "Thong tin tai khoan HR System",
                body
            );

            return Ok(new
            {
                message = "User created successfully",
                username
            });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO dto)
        {
            if (dto.EmployeeId <= 0)
                return BadRequest("EmployeeId is required");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId);

            if (employee == null)
                return BadRequest("Employee does not exist");

            var employeeAlreadyUsed = await _context.Users
                .AnyAsync(u => u.EmployeeId == dto.EmployeeId && u.UserId != id);

            if (employeeAlreadyUsed)
                return BadRequest("This employee is already linked to another account");

            var roleValidation = await _userAccountValidationService.ValidateRoleSelectionAsync(dto.RoleIds);
            if (!roleValidation.IsValid)
                return BadRequest(roleValidation.ErrorMessage);

            var approvalRouteError = await _userAccountValidationService.ValidateApprovalRouteAsync(employee, roleValidation.RoleNames);
            if (approvalRouteError != null)
                return BadRequest(approvalRouteError);

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var emailAlreadyUsed = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail && u.UserId != id);

            if (emailAlreadyUsed)
                return BadRequest("Email already exists");

            var oldEmail = user.Email;
            var oldIsActive = user.IsActive;
            var oldRoles = user.UserRoles.Select(x => x.RoleId).OrderBy(x => x).ToList();

            user.Email = normalizedEmail;
            user.IsActive = dto.IsActive;
            user.EmployeeId = dto.EmployeeId;
            user.ModifiedDate = DateTime.UtcNow;

            _context.UserRoles.RemoveRange(user.UserRoles);

            foreach (var role in roleValidation.ValidRoles)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = id,
                    RoleId = role.RoleId
                });
            }

            await _context.SaveChangesAsync();

            var newRoles = roleValidation.ValidRoles
                .Select(r => r.RoleId)
                .OrderBy(x => x)
                .ToList();

            var emailChanged = !string.Equals(oldEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase);
            var statusChanged = oldIsActive != dto.IsActive;
            var roleChanged = !oldRoles.SequenceEqual(newRoles);

            if (emailChanged || statusChanged || roleChanged)
            {
                var body = $@"
            <h3>Cap nhat tai khoan HR System</h3>
            <p>Xin chao,</p>
            <p>Thong tin tai khoan cua ban vua duoc quan tri vien cap nhat:</p>
            <ul>
                {(emailChanged ? $"<li><b>Email moi:</b> {normalizedEmail}</li>" : "")}
                {(statusChanged ? $"<li><b>Trang thai:</b> {(dto.IsActive ? "Kich hoat" : "Ngung hoat dong")}</li>" : "")}
                {(roleChanged ? "<li><b>Vai tro:</b> Da duoc cap nhat</li>" : "")}
            </ul>
            <p>Neu ban khong nhan ra thay doi nay, vui long lien he quan tri he thong.</p>
            ";

                await _emailService.SendAsync(
                    normalizedEmail,
                    "Cap nhat thong tin tai khoan HR System",
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
            <h3>Thong bao trang thai tai khoan HR System</h3>
            <p>Xin chao,</p>
            <p>Tai khoan cua ban da bi <b>ngung hoat dong</b> boi quan tri vien he thong.</p>
            <ul>
            <li><b>Thoi gian:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
            </ul>
            <p>Neu ban cho rang day la nham lan, vui long lien he bo phan quan tri.</p>
            ";

            await _emailService.SendAsync(
                user.Email,
                "Tai khoan HR System da bi ngung hoat dong",
                body
            );

            return Ok("User deactivated");
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            if (user.EmployeeId == null)
                return BadRequest("This account is not linked to an employee.");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == user.EmployeeId.Value);

            if (employee == null)
                return BadRequest("Employee does not exist");

            var managerValidationError = await _userAccountValidationService.ValidateApprovalRouteAsync(
                employee,
                user.UserRoles.Select(ur => ur.Role.RoleName));

            if (managerValidationError != null)
                return BadRequest(managerValidationError);

            user.IsActive = true;
            user.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var body = $@"
             <h3>Thong bao trang thai tai khoan HR System</h3>
             <p>Xin chao,</p>
             <p>Tai khoan cua ban da duoc <b>kich hoat lai</b> va co the dang nhap he thong.</p>
             <ul>
             <li><b>Thoi gian:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm}</li>
             </ul>
             <p>Vui long dang nhap de tiep tuc su dung he thong.</p>
            ";

            await _emailService.SendAsync(
                user.Email,
                "Tai khoan HR System da duoc kich hoat",
                body
            );

            return Ok("User activated");
        }
    }
}
