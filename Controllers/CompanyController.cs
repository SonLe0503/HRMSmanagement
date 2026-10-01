using HRManagement.Authorization;
using HRManagement.DTOs;
using HRManagement.Services.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Controllers
{
    /// <summary>Quản lý các công ty trên hệ thống, chỉ dành cho SuperAdmin.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = PlatformRoles.SuperAdmin)]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _companyService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            return company is null ? NotFound(new { message = "Không tìm thấy công ty." }) : Ok(company);
        }

        /// <summary>Tạo công ty mới kèm role mẫu, dữ liệu nền và tài khoản ADMIN đầu tiên.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDTO dto)
        {
            var (result, error) = await _companyService.CreateAsync(dto);
            if (error is not null)
                return BadRequest(new { message = error });

            return CreatedAtAction(nameof(GetById), new { id = result!.CompanyId }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyWriteDTO dto)
        {
            var (found, error) = await _companyService.UpdateAsync(id, dto);
            if (!found)
                return NotFound(new { message = "Không tìm thấy công ty." });
            if (error is not null)
                return BadRequest(new { message = error });

            return Ok(new { message = "Cập nhật công ty thành công." });
        }

        /// <summary>Khóa / mở khóa công ty. Khi khóa, nhân viên công ty không đăng nhập được và phiên hiện tại bị từ chối.</summary>
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] UpdateCompanyStatusDTO dto)
        {
            var (found, _) = await _companyService.SetStatusAsync(id, dto.IsActive);
            if (!found)
                return NotFound(new { message = "Không tìm thấy công ty." });

            return Ok(new { message = dto.IsActive ? "Đã mở khóa công ty." : "Đã khóa công ty." });
        }

        /// <summary>Cấp mật khẩu tạm mới cho một admin của công ty (gửi email + trả về một lần) và đăng xuất các phiên cũ.</summary>
        [HttpPost("{id:int}/admins/{userId:int}/reset-password")]
        public async Task<IActionResult> ResetAdminPassword(int id, int userId)
        {
            var (result, error) = await _companyService.ResetAdminPasswordAsync(id, userId);
            if (error is not null)
                return BadRequest(new { message = error });
            if (result is null)
                return NotFound(new { message = "Không tìm thấy tài khoản admin này trong công ty." });

            return Ok(result);
        }
    }
}
