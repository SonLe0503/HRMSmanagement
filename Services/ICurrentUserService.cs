namespace HRManagement.Services
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        string? UserName { get; }
    }
}
