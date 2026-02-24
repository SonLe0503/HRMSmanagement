using HRManagement.DTOs;
using HRManagement.Models;
using HRManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowController : Controller
    {
        private readonly HrmsDbContext _context;

        public WorkflowController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var workflows = await _context.Workflows
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return Ok(workflows);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var workflow = await _context.Workflows
                .Include(x => x.WorkflowStages)
                .FirstOrDefaultAsync(x => x.WorkflowId == id);

            if (workflow == null)
                return NotFound("Workflow not found");

            return Ok(workflow);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkflowDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data");

            // Validate user
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
                return Unauthorized("Invalid token");

            var userId = int.Parse(userClaim.Value);

            // Trim dữ liệu
            dto.WorkflowName = dto.WorkflowName?.Trim() ?? string.Empty;
            dto.WorkflowType = dto.WorkflowType?.Trim() ?? string.Empty;

            // Validate WorkflowName
            if (string.IsNullOrWhiteSpace(dto.WorkflowName))
                return BadRequest("Workflow name is required");

            // Validate WorkflowType
            var validTypes = new[]
            {
        "Leave",
        "Attendance",
        "Overtime",
        "Payroll",
        "Performance"
    };

            if (!validTypes.Contains(dto.WorkflowType))
                return BadRequest("Invalid Workflow Type");

            // Validate EffectiveDate (không cho ngày quá khứ nếu cần)
            if (dto.EffectiveDate.HasValue &&
                dto.EffectiveDate.Value < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            {
                return BadRequest("Effective date cannot be in the past");
            }

            // Check duplicate name
            if (await _context.Workflows
                .AnyAsync(x => x.WorkflowName == dto.WorkflowName))
            {
                return BadRequest("Workflow name already exists");
            }

            var workflow = new Workflow
            {
                WorkflowName = dto.WorkflowName,
                WorkflowType = dto.WorkflowType,
                Description = dto.Description?.Trim(),
                EffectiveDate = dto.EffectiveDate,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Workflows.Add(workflow);
            await _context.SaveChangesAsync();

            return Ok(workflow);
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkflowDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data");

            var workflow = await _context.Workflows.FindAsync(id);
            if (workflow == null)
                return NotFound("Workflow not found");

            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null)
                return Unauthorized("Invalid token");

            var userId = int.Parse(userClaim.Value);

            dto.WorkflowName = dto.WorkflowName?.Trim() ?? string.Empty;
            dto.WorkflowType = dto.WorkflowType?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(dto.WorkflowName))
                return BadRequest("Workflow name is required");

            var validTypes = new[]
            {
        "Leave",
        "Attendance",
        "Overtime",
        "Payroll",
        "Performance"
    };

            if (!validTypes.Contains(dto.WorkflowType))
                return BadRequest("Invalid Workflow Type");

            if (await _context.Workflows
                .AnyAsync(x => x.WorkflowName == dto.WorkflowName && x.WorkflowId != id))
            {
                return BadRequest("Workflow name already exists");
            }

            workflow.WorkflowName = dto.WorkflowName;
            workflow.WorkflowType = dto.WorkflowType;
            workflow.Description = dto.Description?.Trim();
            workflow.EffectiveDate = dto.EffectiveDate;
            workflow.IsActive = dto.IsActive;
            workflow.ModifiedDate = DateTime.UtcNow;
            workflow.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            return Ok(workflow);
        }


        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var workflow = await _context.Workflows.FindAsync(id);

            if (workflow == null)
                return NotFound("Workflow not found");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            workflow.IsActive = false;
            workflow.ModifiedDate = DateTime.UtcNow;
            workflow.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            return Ok("Workflow deactivated successfully");
        }
    }
}
