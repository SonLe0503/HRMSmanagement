using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowStageController : Controller
    {
        private readonly HrmsDbContext _context;
        public WorkflowStageController(HrmsDbContext context)
        {
            _context = context;
        }

        [HttpGet("workflow/{workflowId}")]
        public async Task<IActionResult> GetByWorkflow(int workflowId)
        {
            var stages = await _context.WorkflowStages
                .Where(x => x.WorkflowId == workflowId)
                .Include(x => x.WorkflowStageApprovers)
                .OrderBy(x => x.StageOrder)
                .ToListAsync();

            return Ok(stages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var stage = await _context.WorkflowStages
                .Include(x => x.WorkflowStageApprovers)
                .FirstOrDefaultAsync(x => x.StageId == id);

            if (stage == null)
                return NotFound("Stage not found");

            return Ok(stage);
        }  

        [HttpPost]
        public async Task<IActionResult> Create(CreateWorkflowStageDTO dto)
        {
            var workflowExists = await _context.Workflows
            .AnyAsync(x => x.WorkflowId == dto.WorkflowId && x.IsActive);

            if (!workflowExists)
                return BadRequest("Workflow does not exist or is inactive");

            var stageOrderExists = await _context.WorkflowStages
                .AnyAsync(x => x.WorkflowId == dto.WorkflowId
                            && x.StageOrder == dto.StageOrder);

            if (stageOrderExists)
                return BadRequest("Stage order already exists in this workflow");

            var stage = new WorkflowStage
            {
                WorkflowId = dto.WorkflowId,
                StageOrder = dto.StageOrder,
                StageName = dto.StageName,
                ApprovalType = dto.ApprovalType,
                TimeoutHours = dto.TimeoutHours,
                IsAutoApprove = dto.IsAutoApprove,
                CreatedDate = DateTime.Now
            };

            _context.WorkflowStages.Add(stage);
            await _context.SaveChangesAsync();

            return Ok(stage);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateWorkflowStageDTO dto)
        {
            var stage = await _context.WorkflowStages.FindAsync(id);

            if (stage == null)
                return NotFound("Stage not found");

            var stageOrderExists = await _context.WorkflowStages
                .AnyAsync(x => x.WorkflowId == stage.WorkflowId
                            && x.StageOrder == dto.StageOrder
                            && x.StageId != id);

            if (stageOrderExists)
                return BadRequest("Stage order already exists in this workflow");

            stage.StageOrder = dto.StageOrder;
            stage.StageName = dto.StageName;
            stage.ApprovalType = dto.ApprovalType;
            stage.TimeoutHours = dto.TimeoutHours;
            stage.IsAutoApprove = dto.IsAutoApprove;

            await _context.SaveChangesAsync();

            return Ok(stage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var stage = await _context.WorkflowStages
                .Include(x => x.WorkflowStageApprovers)
                .FirstOrDefaultAsync(x => x.StageId == id);

            if (stage == null)
                return NotFound("Stage not found");

            if (stage.WorkflowStageApprovers.Any())
                return BadRequest("Cannot delete stage with existing approvers");

            _context.WorkflowStages.Remove(stage);
            await _context.SaveChangesAsync();

            return Ok("Stage deleted successfully");
        }
    }
}
