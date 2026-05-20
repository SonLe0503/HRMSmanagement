using HRManagement.Models;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllWithRolesAsync();
        Task<User?> GetByIdWithRolesAsync(int id);
        Task<User?> FindAsync(int id);
        Task<bool> ExistsByEmployeeIdAsync(int employeeId, int? excludeUserId = null);
        Task<bool> ExistsByEmailAsync(string email, int? excludeUserId = null);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<User> AddAsync(User user);
        Task AddUserRolesAsync(IEnumerable<UserRole> userRoles);
        Task ReplaceUserRolesAsync(IEnumerable<UserRole> oldRoles, IEnumerable<UserRole> newRoles);
        Task SaveChangesAsync();
    }
}
