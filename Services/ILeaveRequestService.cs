using HRManagement.DTOs.LeaveRequest;

namespace HRManagement.Services
{
    public interface ILeaveRequestService
    {
        Task<ServiceResult<LeaveRequestResponseDTO>> CreateLeaveRequestAsync(int userId, CreateLeaveRequestDTO dto);
        Task<ServiceResult<string>> ApproveLeaveRequestAsync(int managerUserId, int leaveRequestId, ApproveLeaveRequestDTO dto);
        Task<ServiceResult<List<MyLeaveRequestItemDTO>>> GetMyLeaveRequestsAsync(int userId);
        Task<ServiceResult<string>> RejectLeaveRequestAsync(int managerUserId, int leaveRequestId, RejectLeaveRequestDTO dto);
        Task<ServiceResult<string>> CancelLeaveRequestAsync(int userId, int leaveRequestId, CancelLeaveRequestDTO dto);
        Task<ServiceResult<List<PendingLeaveRequestDTO>>> GetPendingLeaveRequestsAsync(int managerUserId);
    }
}
