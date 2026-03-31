using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess.Implementations
{
    public class PositionRepository : IPositionRepository
    {
        private readonly HrmsDbContext _context;
        public PositionRepository(HrmsDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Position>> GetAllAsync()
        {
            return await _context.Positions
                .Include(p => p.Employees)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.PositionCode)
                .ToListAsync();
        }
        public async Task<IEnumerable<Position>> GetActiveAsync()
        {
            return await _context.Positions
                .Include(p => p.Employees)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Level)
                .ThenBy(p => p.PositionCode)
                .ToListAsync();
        }
        public async Task<Position?> GetByIdWithDetailsAsync(int positionId)
        {
            return await _context.Positions
                .Include(p => p.Employees)
                .FirstOrDefaultAsync(p => p.PositionId == positionId);
        }
        public async Task<Position?> GetByIdAsync(int positionId)
        {
            return await _context.Positions.FindAsync(positionId);
        }
        public async Task<Position> AddAsync(Position position)
        {
            await _context.Positions.AddAsync(position);
            _context.SaveChanges();
            return position;
        }
        public async Task<Position> UpdateAsync(Position position)
        {
            _context.Positions.Update(position);
            await _context.SaveChangesAsync();
            return position;
        }
        public async Task<bool> ExistsAsync(int positionId)
        {
            return await _context.Positions.AnyAsync(p => p.PositionId == positionId);
        }
        public async Task<bool> PositionCodeExistsAsync(string positionCode, int? excludePositionId = null)
        {
            var query = _context.Positions.Where(p => p.PositionCode == positionCode);

            if (excludePositionId.HasValue)
            {
                query = query.Where(p => p.PositionId != excludePositionId.Value);
            }

            return await query.AnyAsync();
        }
        public async Task<bool> HasEmployeesAsync(int positionId)
        {
            return await _context.Employees
                .AnyAsync(e => e.PositionId == positionId);
        }
    }
}
