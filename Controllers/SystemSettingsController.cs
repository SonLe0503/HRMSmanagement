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

        [HttpGet("payroll")]
        [Authorize]
        public async Task<IActionResult> GetPayrollSettings()
        {
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == "Payroll.CutOffDay");

            var dto = new PayrollSettingsDto
            {
                PayrollCutOffDay = setting != null && int.TryParse(setting.SettingValue, out var day) ? day : 1
            };

            return Ok(dto);
        }

        [HttpPut("payroll")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> UpdatePayrollSettings([FromBody] PayrollSettingsDto dto)
        {
            if (dto.PayrollCutOffDay < 1 || dto.PayrollCutOffDay > 28)
                return BadRequest(new { message = "Ngày chốt lương phải từ 1 đến 28." });

            await UpdateOrInsertSetting("Payroll.CutOffDay", dto.PayrollCutOffDay.ToString(), "Payroll");
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật cấu hình kỳ lương thành công." });
        }

        [HttpGet("company")]
        [Authorize]
        public async Task<IActionResult> GetCompanySettings()
        {
            var keys = new[] { "Company.Name", "Company.Address", "Company.Phone", "Company.Email" };
            var settings = await _context.SystemSettings
                .Where(s => keys.Contains(s.SettingKey))
                .ToListAsync();

            var dto = new CompanySettingsDto
            {
                CompanyName = "CÔNG TY CỔ PHẦN HR SYSTEM",
                Address = "",
                Phone = "",
                Email = ""
            };
            foreach (var s in settings)
            {
                if (s.SettingKey == "Company.Name")    dto.CompanyName = s.SettingValue;
                if (s.SettingKey == "Company.Address") dto.Address     = s.SettingValue;
                if (s.SettingKey == "Company.Phone")   dto.Phone       = s.SettingValue;
                if (s.SettingKey == "Company.Email")   dto.Email       = s.SettingValue;
            }
            return Ok(dto);
        }

        [HttpPut("company")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateCompanySettings([FromBody] CompanySettingsDto dto)
        {
            await UpdateOrInsertSetting("Company.Name",    dto.CompanyName ?? "", "Company");
            await UpdateOrInsertSetting("Company.Address", dto.Address     ?? "", "Company");
            await UpdateOrInsertSetting("Company.Phone",   dto.Phone       ?? "", "Company");
            await UpdateOrInsertSetting("Company.Email",   dto.Email       ?? "", "Company");
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thông tin công ty thành công." });
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
