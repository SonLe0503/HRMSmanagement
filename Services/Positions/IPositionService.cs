using HRManagement.DTOs;

namespace HRManagement.Services.Positions
{
    public interface IPositionService
    {
        Task<PositionResponseDto> CreatePositionAsync(CreatePositionDto createDto);
        Task<IEnumerable<PositionListDto>> GetAllPositionsAsync();
        Task<IEnumerable<PositionListDto>> GetActivePositionsAsync();
        Task<PositionResponseDto?> GetPositionByIdAsync(int positionId);
        Task<PositionResponseDto> UpdatePositionAsync(int positionId, UpdatePositionDto updateDto);
        Task<bool> DeactivatePositionAsync(int positionId);
        Task<bool> ActivatePositionAsync(int positionId);
    }
}
