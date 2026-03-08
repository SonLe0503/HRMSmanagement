using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : Controller
    {
        private HrmsDbContext _context;
        public PermissionController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _context.Permissions
                .Select(p => new DTOs.PermisstionDTO
                {
                    PermissionId = p.PermissionId,
                    PermissionCode = p.PermissionCode,
                    PermissionName = p.PermissionName,
                    Module = p.Module,
                    Description = p.Description
                })
                .ToListAsync();
            return Ok(permissions);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var permission = await _context.Permissions
                .Where(p => p.PermissionId == id)
                .Select(p => new DTOs.PermisstionDTO
                {
                    PermissionId = p.PermissionId,
                    PermissionCode = p.PermissionCode,
                    PermissionName = p.PermissionName,
                    Module = p.Module,
                    Description = p.Description
                })
                .FirstOrDefaultAsync();
            if (permission == null)
                return NotFound();
            return Ok(permission);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePermissionDTO dto)
        {
            var exists = await _context.Permissions
                .AnyAsync(x => x.PermissionCode == dto.PermissionCode);

            if (exists)
                return BadRequest("PermissionCode already exists");

            var permission = new Permission
            {
                PermissionCode = dto.PermissionCode,
                PermissionName = dto.PermissionName,
                Module = dto.Module,
                Description = dto.Description
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Permission created successfully" });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdatePermissionDTO dto)
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(x => x.PermissionId == id);

            if (permission == null)
                return NotFound("Permission not found");

            permission.PermissionName = dto.PermissionName;
            permission.Module = dto.Module;
            permission.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Permission updated successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await _context.Permissions
                .Include(p => p.RolePermissions)
                .FirstOrDefaultAsync(p => p.PermissionId == id);

            if (permission == null)
                return NotFound("Permission not found");

            if (permission.RolePermissions.Any())
                return BadRequest("Permission is assigned to roles, cannot delete");

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Permission deleted successfully" });
        }
    }
}
