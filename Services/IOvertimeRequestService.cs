using HRManagement.DTOs.LeaveRequest;
using HRManagement.DTOs.OvertimeRequest;

namespace HRManagement.Services
{
    public interface IOvertimeRequestService
    {
        Task<ServiceResult<string>> CreateOvertimeRequestAsync(int userId, CreateOvertimeRequestDTO dto);
    }
}
