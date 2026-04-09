namespace HRManagement.Services.CurrentUsers
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        int? EmployeeId { get; }
        string? RoleName { get; }

        int GetCurrentUserId();
        Task<int> GetCurrentEmployeeIdAsync();
    }
}
