using HRManagement.DTOs.LeaveBalance;
using HRManagement.DTOs.LeaveRequest;

namespace HRManagement.Services.Leaves
{
    public interface ILeaveBalanceService
    {
        Task<ServiceResult<List<MyLeaveBalanceDTO>>> GetMyLeaveBalanceAsync(int userId);
        Task<ServiceResult<List<LeaveBalanceListDTO>>> GetAllLeaveBalancesAsync();

        Task<ServiceResult<List<LeaveBalanceListDTO>>> GetLeaveBalancesByEmployeeAsync(int employeeId);

        Task<ServiceResult<string>> CreateLeaveBalanceAsync(int hrUserId, CreateLeaveBalanceDTO dto);
        Task<ServiceResult<string>> AdjustLeaveBalanceAsync(int hrUserId, AdjustLeaveBalanceDTO dto);
        Task<ServiceResult<GenerateBalanceResultDTO>> GenerateBalancesForYearAsync(int hrUserId, int year, bool carryForward);
    }
}
