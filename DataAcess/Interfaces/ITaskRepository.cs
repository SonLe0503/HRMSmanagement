using HRManagement.Models;
using Task = HRManagement.Models.Task;

namespace HRManagement.DataAcess.Interfaces
{
    public interface ITaskRepository
    {
        System.Threading.Tasks.Task<IEnumerable<Task>> GetAllAsync();
        System.Threading.Tasks.Task<Task?> GetByIdAsync(int id);
        System.Threading.Tasks.Task<Task?> FindAsync(int id);
        System.Threading.Tasks.Task<bool> UserExistsAsync(int userId);
        System.Threading.Tasks.Task<Task> AddAsync(Task task);
        System.Threading.Tasks.Task SaveChangesAsync();
    }
}
