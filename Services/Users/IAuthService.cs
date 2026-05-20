using HRManagement.DTOs;
using HRManagement.DTOs.Auths;

namespace HRManagement.Services.Users
{
    public interface IAuthService
    {
        Task<(bool Success, string? Error, string? Token)> LoginAsync(LoginRequestDTO dto);
        Task<(bool Success, string? Error, bool NotFound)> ChangePasswordAsync(int userId, ChangePasswordDTO dto);
        Task<(bool Success, string? Error)> ForgotPasswordAsync(ForgotPasswordRequestDTO dto);
        Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}
