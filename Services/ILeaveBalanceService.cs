using HRManagement.DTOs.LeaveBalance;
using HRManagement.DTOs.LeaveRequest;

namespace HRManagement.Services
{
    public interface ILeaveBalanceService
    {
        Task<ServiceResult<List<MyLeaveBalanceDTO>>> GetMyLeaveBalanceAsync(int userId);
        Task<ServiceResult<string>> AdjustLeaveBalanceAsync(int hrUserId, AdjustLeaveBalanceDTO dto);
    }
}
