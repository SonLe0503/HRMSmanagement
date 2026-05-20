using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.DataAcess.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly HrmsDbContext _context;

        public UserRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllWithRolesAsync()
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<User?> GetByIdWithRolesAsync(int id)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> FindAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> ExistsByEmployeeIdAsync(int employeeId, int? excludeUserId = null)
        {
            return await _context.Users
                .AnyAsync(u => u.EmployeeId == employeeId
                    && (excludeUserId == null || u.UserId != excludeUserId));
        }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeUserId = null)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email
                    && (excludeUserId == null || u.UserId != excludeUserId));
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task AddUserRolesAsync(IEnumerable<UserRole> userRoles)
        {
            _context.UserRoles.AddRange(userRoles);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceUserRolesAsync(IEnumerable<UserRole> oldRoles, IEnumerable<UserRole> newRoles)
        {
            _context.UserRoles.RemoveRange(oldRoles);
            _context.UserRoles.AddRange(newRoles);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
