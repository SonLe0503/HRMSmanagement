using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowStageApproveController : Controller
    {
        private readonly HrmsDbContext _context;
        public WorkflowStageApproveController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet("stage/{stageId}")]
        public async Task<IActionResult> GetByStage(int stageId)
        {
            var approvers = await _context.WorkflowStageApprovers
                .Where(x => x.StageId == stageId)
                .Include(x => x.Role)
                .Include(x => x.User)
                .ToListAsync();

            return Ok(approvers);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateWorkflowStageApproverDTO dto)
        {
            if (dto == null)
                return BadRequest("Invalid data");

            if (!Enum.IsDefined(typeof(ApproverTypeEnum), dto.ApproverType))
                return BadRequest("Invalid ApproverType");

            var stageExists = await _context.WorkflowStages
                .AnyAsync(x => x.StageId == dto.StageId);

            if (!stageExists)
                return BadRequest("Stage does not exist");

            // Reset tất cả trước khi validate
            dto.RoleId = dto.RoleId == 0 ? null : dto.RoleId;
            dto.UserId = dto.UserId == 0 ? null : dto.UserId;
            dto.DynamicRule = dto.DynamicRule?.Trim();

            switch (dto.ApproverType)
            {
                case (int)ApproverTypeEnum.Role:
                    if (dto.RoleId == null || dto.UserId != null)
                        return BadRequest("Role approver must have RoleId and no UserId");
                    break;

                case (int)ApproverTypeEnum.User:
                    if (dto.UserId == null || dto.RoleId != null)
                        return BadRequest("User approver must have UserId and no RoleId");
                    break;

                default:
                    return BadRequest("Invalid ApproverType");
            }


            var entity = new WorkflowStageApprover
            {
                StageId = dto.StageId,
                ApproverType = dto.ApproverType,
                RoleId = dto.RoleId,
                UserId = dto.UserId,
                IsDynamic = dto.ApproverType == (int)ApproverTypeEnum.Dynamic,
                DynamicRule = dto.DynamicRule
            };

            _context.WorkflowStageApprovers.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(entity);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateWorkflowStageApproverDTO dto)
        {
            var entity = await _context.WorkflowStageApprovers.FindAsync(id);

            if (entity == null)
                return NotFound("Approver not found");

            if (!Enum.IsDefined(typeof(ApproverTypeEnum), dto.ApproverType))
                return BadRequest("Invalid ApproverType");

            switch (dto.ApproverType)
            {
                case (int)ApproverTypeEnum.Role:
                    if (dto.RoleId == null || dto.UserId != null)
                        return BadRequest("Role approver must have RoleId and no UserId");
                    break;

                case (int)ApproverTypeEnum.User:
                    if (dto.UserId == null || dto.RoleId != null)
                        return BadRequest("User approver must have UserId and no RoleId");
                    break;

                default:
                    return BadRequest("Invalid ApproverType");
            }


            entity.ApproverType = dto.ApproverType;
            entity.IsDynamic = dto.ApproverType == (int)ApproverTypeEnum.Dynamic;

            await _context.SaveChangesAsync();

            return Ok(entity);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.WorkflowStageApprovers.FindAsync(id);

            if (entity == null)
                return NotFound("Approver not found");

            _context.WorkflowStageApprovers.Remove(entity);
            await _context.SaveChangesAsync();

            return Ok("Deleted successfully");
        }
        public enum ApproverTypeEnum
        {
            Role = 1,
            User = 2,
            Dynamic = 3
        }

    }
}
