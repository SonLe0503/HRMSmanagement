namespace HRManagement.Services
{
    public interface ICurrentUserService
    {
        int GetCurrentUserId();
        Task<int> GetCurrentEmployeeIdAsync();
    }
}
