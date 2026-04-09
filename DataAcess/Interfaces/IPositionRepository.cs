using HRManagement.Models;

namespace HRManagement.DataAcess.Interfaces
{
    public interface IPositionRepository
    {
        Task<IEnumerable<Position>> GetAllAsync();
        Task<IEnumerable<Position>> GetActiveAsync();
        Task<Position?> GetByIdWithDetailsAsync(int positionId);
        Task<Position?> GetByIdAsync(int positionId);
        Task<Position> AddAsync(Position position);
        Task<Position> UpdateAsync(Position position);
        Task<bool> ExistsAsync(int positionId);
        Task<bool> PositionCodeExistsAsync(string positionCode, int? excludePositionId = null);
        Task<bool> HasEmployeesAsync(int positionId);
    }
}
