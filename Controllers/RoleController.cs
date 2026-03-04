using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private readonly HrmsDbContext _context;
        public RoleController(HrmsDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
       .Select(r => new RoleResponseDTO
       {
           RoleId = r.RoleId,
           RoleName = r.RoleName,
           Description = r.Description,

           UserCount = r.UserRoles.Count,

           IsActive = r.IsActive,
           LastModifiedDate = r.ModifiedDate ?? r.CreatedDate
       })
       .ToListAsync();

            return Ok(roles);
        }
        [Authorize(Roles = "ADMIN")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _context.Roles
                .Where(r => r.RoleId == id)
                .Select(r => new RoleResponseDTO
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    IsActive = r.IsActive
                })
                .FirstOrDefaultAsync();

            if (role == null)
                return NotFound();

            return Ok(role);
        }
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDTO dto)
        {
            if (await _context.Roles.AnyAsync(r => r.RoleName == dto.RoleName))
                return BadRequest("Role already exists");

            var role = new Role
            {
                RoleName = dto.RoleName,
                Description = dto.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok("Role created successfully");
        }
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeRoleStatus(int id, [FromQuery] bool isActive)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            if (!isActive)
            {
                var hasUsers = await _context.UserRoles
                    .AnyAsync(ur => ur.RoleId == id);

                if (hasUsers)
                {
                    return BadRequest(new
                    {
                        message = "Không thể vô hiệu hóa role đang được gán cho người dùng"
                    });
                }
            }
            role.IsActive = isActive;
            role.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Role status updated");
        }

    }
}
