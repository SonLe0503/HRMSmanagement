using HRManagement.DTOs;
using HRManagement.DTOs.Common;
using System.Security.Claims;

namespace HRManagement.Services
{
    public interface ILeaveBalanceService
    {
        Task<ApiResult<LeaveBalanceResponseDTO>> GetMyLeaveBalanceAsync(ClaimsPrincipal user);
    }
}
