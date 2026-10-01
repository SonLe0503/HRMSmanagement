using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Services.Companies
{
    /// <summary>
    /// Tạo tài khoản SuperAdmin đầu tiên từ cấu hình SuperAdmin:Username / Email / Password
    /// (user-secrets hoặc biến môi trường), để mật khẩu không nằm trong code hay migration.
    /// Chỉ tạo khi username chưa tồn tại; đổi mật khẩu sau đó qua chức năng đổi mật khẩu.
    /// </summary>
    public static class SuperAdminSeeder
    {
        public static async System.Threading.Tasks.Task EnsureAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
        {
            var username = configuration["SuperAdmin:Username"];
            var email = configuration["SuperAdmin:Email"];
            var password = configuration["SuperAdmin:Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            try
            {
                using var scope = services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();

                if (await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == username))
                    return;

                context.Users.Add(new User
                {
                    CompanyId = null,
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                });
                await context.SaveChangesAsync();

                logger.LogInformation("Created SuperAdmin account {Username}.", username);
            }
            catch (Exception ex)
            {
                // Không chặn app khởi động (ví dụ khi DB chưa chạy migration)
                logger.LogError(ex, "Could not create SuperAdmin account {Username}.", username);
            }
        }
    }
}
