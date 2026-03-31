namespace HRManagement.Services.CurrentUsers
{
    public interface ICurrentUserService
    {
        int GetCurrentUserId();
        Task<int> GetCurrentEmployeeIdAsync();
    }
}
