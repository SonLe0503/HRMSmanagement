using HRManagement.Models;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserForLoginAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailOrUsernameAsync(string emailOrUsername);
        Task SaveChangesAsync();
    }
}
