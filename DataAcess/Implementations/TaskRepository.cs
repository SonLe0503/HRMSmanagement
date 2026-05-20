using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using Task = HRManagement.Models.Task;

namespace HRManagement.DataAcess.Implementations
{
    public class TaskRepository : ITaskRepository
    {
        private readonly HrmsDbContext _context;

        public TaskRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async System.Threading.Tasks.Task<IEnumerable<Task>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedToNavigation)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task<Task?> GetByIdAsync(int id)
        {
            return await _context.Tasks
                .Include(t => t.AssignedToNavigation)
                .FirstOrDefaultAsync(t => t.TaskId == id);
        }

        public async System.Threading.Tasks.Task<Task?> FindAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async System.Threading.Tasks.Task<bool> UserExistsAsync(int userId)
        {
            return await _context.Users.AnyAsync(u => u.UserId == userId);
        }

        public async System.Threading.Tasks.Task<Task> AddAsync(Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async System.Threading.Tasks.Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
