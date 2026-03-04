using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface IApprovalService
    {
        Task<List<PendingApprovalDTO>> GetPendingRequestsAsync(int managerEmployeeId);
    }
}
