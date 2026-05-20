using HRManagement.DTOs;

namespace HRManagement.Services.Users
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO?> GetUserAsync(int id);
        Task<(bool Success, string? Error, string? Username)> CreateUserAsync(CreateUserDTO dto);
        Task<(bool Success, string? Error)> UpdateUserAsync(int id, UpdateUserDTO dto);
        Task<(bool Success, string? Error)> DeactivateUserAsync(int id);
        Task<(bool Success, string? Error)> ActivateUserAsync(int id);
    }
}
