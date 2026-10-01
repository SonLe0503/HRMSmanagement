using HRManagement.Authorization;
using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Roles and their permissions are configured by the SuperAdmin for a chosen company; company admins
    // only read their roles (to assign them to accounts).
    [SuperAdminCompanyScope]
    public class RoleController : Controller
    {
        private readonly HrmsDbContext _context;
        private readonly IPermissionCache _permissionCache;
        public RoleController(HrmsDbContext context, IPermissionCache permissionCache)
        {
            _context = context;
            _permissionCache = permissionCache;
        }
        [Authorize(Roles = "ADMIN," + PlatformRoles.SuperAdmin)]
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
        [Authorize(Roles = "ADMIN," + PlatformRoles.SuperAdmin)]
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
        [Authorize(Roles = PlatformRoles.SuperAdmin)]
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
        [Authorize(Roles = PlatformRoles.SuperAdmin)]
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
            _permissionCache.Invalidate();
            return Ok("Role status updated");
        }

        [Authorize(Roles = PlatformRoles.SuperAdmin)]
        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissionCatalog()
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.PermissionId)
                .Select(p => new PermissionResponseDTO
                {
                    PermissionId = p.PermissionId,
                    PermissionKey = p.PermissionKey,
                    DisplayName = p.DisplayName,
                    Category = p.Category,
                    Description = p.Description,
                    DisplayOrder = p.DisplayOrder
                })
                .ToListAsync();

            return Ok(permissions);
        }

        [Authorize(Roles = PlatformRoles.SuperAdmin)]
        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetRolePermissions(int id)
        {
            if (!await _context.Roles.AnyAsync(r => r.RoleId == id))
                return NotFound();

            var keys = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Select(rp => rp.Permission.PermissionKey)
                .ToListAsync();

            return Ok(keys);
        }

        [Authorize(Roles = PlatformRoles.SuperAdmin)]
        [HttpPut("{id}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(int id, [FromBody] UpdateRolePermissionsDTO dto)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return NotFound();

            var permissionIds = await _context.Permissions
                .Where(p => dto.PermissionKeys.Contains(p.PermissionKey))
                .Select(p => p.PermissionId)
                .ToListAsync();

            var existing = _context.RolePermissions.Where(rp => rp.RoleId == id);
            _context.RolePermissions.RemoveRange(existing);

            foreach (var permissionId in permissionIds)
                _context.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = permissionId });

            await _context.SaveChangesAsync();
            _permissionCache.Invalidate();
            return Ok("Role permissions updated");
        }

    }
}
