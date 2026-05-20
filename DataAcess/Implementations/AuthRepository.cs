using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.DataAcess.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly HrmsDbContext _context;

        public AuthRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserForLoginAsync(string username)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Employee)
                    .ThenInclude(e => e!.Position)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetUserByEmailOrUsernameAsync(string emailOrUsername)
        {
            var input = emailOrUsername.Trim().ToLower();
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == input || u.Username.ToLower() == input);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
