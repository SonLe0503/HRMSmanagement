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
            // Chưa đăng nhập nên chưa có công ty: bỏ filter, username là duy nhất toàn hệ thống
            return await _context.Users.IgnoreQueryFilters()
                .Include(u => u.Company)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.Employee)
                    .ThenInclude(e => e!.Position)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            // Id comes from the caller's own validated token; bypassing the filter lets the
            // SuperAdmin (no company) change its own password too
            return await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetUserByEmailOrUsernameAsync(string emailOrUsername)
        {
            var input = emailOrUsername.Trim().ToLower();
            // Quên mật khẩu chạy khi chưa đăng nhập: bỏ filter công ty
            return await _context.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == input || u.Username.ToLower() == input);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
