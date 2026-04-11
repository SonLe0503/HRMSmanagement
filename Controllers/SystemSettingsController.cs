using HRManagement.DTOs.SystemSettings;
using HRManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemSettingsController : ControllerBase
    {
        private readonly HrmsDbContext _context;

        public SystemSettingsController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet("location")]
        [Authorize]
        public async Task<IActionResult> GetLocationSettings()
        {
            var settings = await _context.SystemSettings
                .Where(s => s.SettingKey == "OfficeLatitude" || s.SettingKey == "OfficeLongitude" || s.SettingKey == "AttendanceAllowedRadius")
                .ToListAsync();

            var dto = new LocationSettingsDto();
            foreach (var s in settings)
            {
                if (s.SettingKey == "OfficeLatitude" && double.TryParse(s.SettingValue, out var lat)) dto.OfficeLatitude = lat;
                if (s.SettingKey == "OfficeLongitude" && double.TryParse(s.SettingValue, out var lng)) dto.OfficeLongitude = lng;
                if (s.SettingKey == "AttendanceAllowedRadius" && double.TryParse(s.SettingValue, out var rad)) dto.AttendanceAllowedRadius = rad;
            }

            return Ok(dto);
        }

        [HttpPut("location")]
        [Authorize(Roles = "ADMIN,MANAGE")]
        public async Task<IActionResult> UpdateLocationSettings([FromBody] LocationSettingsDto dto)
        {
            await UpdateOrInsertSetting("OfficeLatitude", dto.OfficeLatitude.ToString(), "Attendance");
            await UpdateOrInsertSetting("OfficeLongitude", dto.OfficeLongitude.ToString(), "Attendance");
            await UpdateOrInsertSetting("AttendanceAllowedRadius", dto.AttendanceAllowedRadius.ToString(), "Attendance");

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật cấu hình vị trí điểm danh thành công." });
        }

        [HttpGet("approval")]
        [Authorize]
        public async Task<IActionResult> GetApprovalSettings()
        {
            var settings = await _context.SystemSettings
                .Where(s => s.SettingKey == "Approval.TopLevelFallbackUserId" || s.SettingKey == "Approval.DefaultFallbackUserId")
                .ToListAsync();

            var dto = new ApprovalSettingsDto();
            foreach (var s in settings)
            {
                if (s.SettingKey == "Approval.TopLevelFallbackUserId" && int.TryParse(s.SettingValue, out var topId))
                    dto.TopLevelFallbackUserId = topId;
                if (s.SettingKey == "Approval.DefaultFallbackUserId" && int.TryParse(s.SettingValue, out var defId))
                    dto.DefaultFallbackUserId = defId;
            }

            return Ok(dto);
        }

        [HttpPut("approval")]
        [Authorize(Roles = "ADMIN,MANAGE")]
        public async Task<IActionResult> UpdateApprovalSettings([FromBody] ApprovalSettingsDto dto)
        {
            await UpdateOrInsertSetting("Approval.TopLevelFallbackUserId", dto.TopLevelFallbackUserId?.ToString() ?? "", "Workflow");
            await UpdateOrInsertSetting("Approval.DefaultFallbackUserId", dto.DefaultFallbackUserId?.ToString() ?? "", "Workflow");
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật cấu hình phê duyệt thành công." });
        }

        private async Task UpdateOrInsertSetting(string key, string value, string category)
        {
            var exists = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            int? modifiedBy = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idStr = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idStr, out var id)) modifiedBy = id;
            }

            if (exists == null)
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    SettingCategory = category,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = modifiedBy
                });
            }
            else
            {
                exists.SettingValue = value;
                exists.ModifiedDate = DateTime.Now;
                exists.ModifiedBy = modifiedBy;
            }
        }
    }
}
