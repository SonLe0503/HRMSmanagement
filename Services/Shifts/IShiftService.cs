using HRManagement.DTOs.Shifts;

namespace HRManagement.Services.Shifts
{
    public interface IShiftService
    {
        Task<ShiftResponseDto> CreateShiftAsync(int currentUserId, CreateShiftDto dto);
        Task<List<ShiftResponseDto>> GetAllShiftsAsync(bool? isActive);
        Task<ShiftResponseDto?> GetShiftByIdAsync(int shiftId);
        Task<ShiftResponseDto> UpdateShiftAsync(int shiftId, UpdateShiftDto dto);
        Task DeactivateShiftAsync(int shiftId);
        Task ActivateShiftAsync(int shiftId);
    }
}
