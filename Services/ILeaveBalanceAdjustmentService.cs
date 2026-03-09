using HRManagement.DTOs;
using HRManagement.DTOs.Common;
using System.Security.Claims;

namespace HRManagement.Services
{
    public interface ILeaveBalanceAdjustmentService
    {
        Task<ApiResult<AdjustLeaveBalanceResponseDTO>> AdjustAsync(AdjustLeaveBalanceDTO dto, ClaimsPrincipal user);
    }
}
