using HRManagement.DTOs.LeaveRequest;

namespace HRManagement.Services
{
    public interface ILeaveRequestService
    {
        Task<ServiceResult<LeaveRequestResponseDTO>> CreateLeaveRequestAsync(int userId, CreateLeaveRequestDTO dto);
    }
}
