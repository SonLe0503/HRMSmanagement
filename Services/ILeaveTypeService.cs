using HRManagement.DTOs.LeaveTypes;

namespace HRManagement.Services
{
    public interface ILeaveTypeService
    {
        Task<IEnumerable<LeaveTypeDTO>> GetActiveLeaveTypesAsync();
        Task<IEnumerable<LeaveTypeDTO>> GetAllLeaveTypesAsync();
        Task<LeaveTypeDTO?> GetLeaveTypeByIdAsync(int id);
        Task<LeaveTypeDTO> CreateLeaveTypeAsync(CreateLeaveTypeDTO dto);
        Task<LeaveTypeDTO?> UpdateLeaveTypeAsync(int id, UpdateLeaveTypeDTO dto);
        Task<bool> SoftDeleteLeaveTypeAsync(int id);

    }
}