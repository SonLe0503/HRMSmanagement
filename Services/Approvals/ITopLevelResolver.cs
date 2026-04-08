using System.Threading.Tasks;

namespace HRManagement.Services.Approvals
{
    public interface ITopLevelResolver
    {
        Task<bool> IsTopLevelEmployeeAsync(int employeeId);
        Task<int?> GetTopLevelFallbackUserIdAsync();
    }
}
