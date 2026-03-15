using AutoMapper;
using HRManagement.DTOs;
using HRManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task = HRManagement.Models.Task;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly HrmsDbContext _context;
        private readonly IMapper _mapper;
        public TaskController(HrmsDbContext context, IMapper mapper) 
        { 
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _context.Tasks
                .Include(t => t.AssignedToNavigation)
                .ToListAsync();
            var result = _mapper.Map<List<TaskDTO>>(tasks);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedToNavigation)
                .FirstOrDefaultAsync(t => t.TaskId == id);
            if (task == null)
                return NotFound("Task not found");
            return Ok(_mapper.Map<TaskDTO>(task));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTaskDTO dto)
        {
            var userId = GetCurrentUserId();
            var userExists = await _context.Users
               .AnyAsync(u => u.UserId == dto.AssignedTo);

            if (!userExists)
                return BadRequest("Assigned user does not exist");

            var task = _mapper.Map<Task>(dto);

            task.Status = "Pending";
            task.CreatedDate = DateTime.UtcNow;
            task.CreatedBy = userId;
            task.CompletedDate = null;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return Ok(_mapper.Map<TaskDTO>(task));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTaskDTO dto)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound("Task not found");

            if (dto.AssignedTo.HasValue)
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.UserId == dto.AssignedTo.Value);

                if (!userExists)
                    return BadRequest("Assigned user does not exist");
            }
            if (task.Status != "Pending")
                return BadRequest("Only pending tasks can be edited");

            _mapper.Map(dto, task);

            await _context.SaveChangesAsync();

            return Ok("Task updated successfully");
        }
        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(int id, ApproveTaskDTO dto)
        {
            var userId = GetCurrentUserId(); // bạn lấy từ JWT

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
                return NotFound("Task not found");

            // ✅ Kiểm tra task có được assign cho user này không
            if (task.AssignedTo != userId)
                return Forbid("You are not allowed to approve this task");

            // ✅ Kiểm tra trạng thái hợp lệ
            if (task.Status != "Pending" && task.Status != "InProgress")
                return BadRequest("Task is not in approvable state");

            // ✅ Update trạng thái
            task.Status = "Approved";
            task.CompletedDate = DateTime.UtcNow;
            task.CompletionNotes = dto.Comments;

            // TODO: thêm AuditLog ở đây

            await _context.SaveChangesAsync();

            return Ok("Task approved successfully");
        }

        [HttpPatch("{id}/reject")]
        public async Task<IActionResult> Reject(int id, RejectTaskDTO dto)
        {
            var userId = GetCurrentUserId();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
                return NotFound("Task not found");

            if (task.AssignedTo != userId)
                return Forbid("You are not allowed to reject this task");

            if (task.Status != "Pending" && task.Status != "InProgress")
                return BadRequest("Task is not in rejectable state");

            if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Length < 10)
                return BadRequest("Rejection reason must be at least 10 characters");

            task.Status = "Rejected";
            task.CompletedDate = DateTime.UtcNow;
            task.CompletionNotes = dto.Reason;

            await _context.SaveChangesAsync();

            return Ok("Task rejected successfully");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetCurrentUserId();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
                return NotFound("Task not found");

            if (task.Status == "Approved" || task.Status == "Rejected")
                return BadRequest("Processed tasks cannot be cancelled");

            if (task.AssignedTo != userId)
                return Forbid("You are not allowed to cancel this task");

            task.Status = "Cancelled";
            task.CompletedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Task cancelled successfully");
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new Exception("User ID not found in token");

            return int.Parse(userIdClaim.Value);
        }
    }
}
