using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface ILeaveBalanceService
    {
        Task<List<LeaveBalanceDTO>> GetLeaveBalanceAsync(int employeeId);
        Task AdjustLeaveBalanceAsync(AdjustLeaveBalanceDTO dto, int hrUserId);
    }
}
