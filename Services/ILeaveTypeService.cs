using HRManagement.DTOs.LeaveRequest;

namespace HRManagement.Services
{
    public interface ILeaveTypeService
    {
        Task<IEnumerable<LeaveTypeDTO>> GetActiveLeaveTypesAsync();
    }
}