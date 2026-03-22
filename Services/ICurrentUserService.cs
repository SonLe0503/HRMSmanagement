namespace HRManagement.Services
{
    public interface ICurrentUserService
    {
        int GetUserId();
        string GetRole();
        string GetFullName();
        int GetCurrentUserId();
        Task<int> GetCurrentEmployeeIdAsync();
    }
}
