using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRManagement.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HrmsDbContext _context;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, HrmsDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public int GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("Bạn chưa đăng nhập.");

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new UnauthorizedAccessException("Không tìm thấy UserId trong token.");

            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("UserId trong token không hợp lệ.");

            return userId;
        }

        public async Task<int> GetCurrentEmployeeIdAsync()
        {
            var userId = GetCurrentUserId();

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

            if (user == null)
                throw new KeyNotFoundException("Tài khoản không tồn tại hoặc đã bị vô hiệu hóa.");

            if (!user.EmployeeId.HasValue)
                throw new InvalidOperationException("Tài khoản chưa liên kết nhân viên.");

            return user.EmployeeId.Value;
        }
    }
}
