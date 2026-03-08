using HRManagement.Common;
using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface IOvertimeRequestService
    {
        Task<ApiResponse<object>> CreateAsync(CreateOvertimeRequestDTO dto, int employeeId);
        Task<ApiResponse<object>> CancelAsync(int requestId, int employeeId);

    }
}
