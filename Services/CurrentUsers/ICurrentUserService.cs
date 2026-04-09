using System.Threading.Tasks;

namespace HRManagement.Services.CurrentUsers
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        int? EmployeeId { get; }
        string? RoleName { get; }

        int GetUserId();
        string GetRole();
        string GetFullName();
        int GetCurrentUserId();
        Task<int> GetCurrentEmployeeIdAsync();
    }
}
