using HRManagement.DTOs.LeaveRequest;
using HRManagement.DTOs.ResignationRequest;

namespace HRManagement.Services.Resignations
{
    public interface IResignationRequestService
    {
        Task<ServiceResult<ResignationRequestResponseDto>> CreateAsync(int userId, CreateResignationRequestDto dto);
        Task<ServiceResult<List<ResignationRequestResponseDto>>> GetMyRequestsAsync(int userId);
        Task<ServiceResult<string>> CancelAsync(int userId, int requestId);
        Task<ServiceResult<List<ResignationRequestResponseDto>>> GetPendingForManagerAsync(int userId);
        Task<ServiceResult<string>> ApproveAsync(int userId, int requestId, ApproveResignationRequestDto dto);
        Task<ServiceResult<string>> RejectAsync(int userId, int requestId, RejectResignationRequestDto dto);
    }
}
