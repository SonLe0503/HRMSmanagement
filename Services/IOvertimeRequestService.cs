using HRManagement.DTOs.LeaveRequest;
using HRManagement.DTOs.OvertimeRequest;

namespace HRManagement.Services
{
    public interface IOvertimeRequestService
    {
        Task<ServiceResult<string>> CreateOvertimeRequestAsync(int userId, CreateOvertimeRequestDTO dto);
        Task<ServiceResult<string>> ApproveOvertimeRequestAsync(int managerUserId, int requestId, ApproveOvertimeRequestDTO dto);

        Task<ServiceResult<string>> RejectOvertimeRequestAsync(int managerUserId, int requestId, RejectOvertimeRequestDTO dto);
    }
}
