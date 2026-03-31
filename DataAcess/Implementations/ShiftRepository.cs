using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess.Implementations
{
    public class ShiftRepository : IShiftRepository
    {
         private readonly HrmsDbContext _context;
        public ShiftRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<Shift>> GetAllShiftsAsync(bool? isActive)
        {
            var query = _context.Shifts.AsQueryable();

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            return await query
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<Shift?> GetShiftByIdAsync(int shiftId)
        {
            return await _context.Shifts.FirstOrDefaultAsync(x => x.ShiftId == shiftId);
        }

        public async Task<Shift?> GetShiftByCodeAsync(string shiftCode)
        {
            return await _context.Shifts.FirstOrDefaultAsync(x => x.ShiftCode == shiftCode);
        }

        public async System.Threading.Tasks.Task AddShiftAsync(Shift shift)
        {
            await _context.Shifts.AddAsync(shift);
        }

        public System.Threading.Tasks.Task UpdateShiftAsync(Shift shift)
        {
            _context.Shifts.Update(shift);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public async System.Threading.Tasks.Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
