using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HRManagement.DataAcess
{
    public class EvaluationCycleRepository : IEvaluationCycleRepository
    {
        private readonly HrmsDbContext _context;
        public EvaluationCycleRepository(HrmsDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<EvaluationCycle>> GetAllAsync()
        {
            return await _context.EvaluationCycles
                .Include(c => c.Evaluations)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvaluationCycle>> GetActiveAsync()
        {
            return await _context.EvaluationCycles
                .Include(c => c.Evaluations)
                .Where(c => c.Status == "Active")
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<EvaluationCycle?> GetByIdWithDetailsAsync(int cycleId)
        {
            return await _context.EvaluationCycles
                .Include(c => c.Evaluations)
                .FirstOrDefaultAsync(c => c.CycleId == cycleId);
        }

        public async Task<EvaluationCycle?> GetByIdAsync(int cycleId)
        {
            return await _context.EvaluationCycles.FindAsync(cycleId);
        }

        public async Task<EvaluationCycle> AddAsync(EvaluationCycle cycle)
        {
            await _context.EvaluationCycles.AddAsync(cycle);
            await _context.SaveChangesAsync();
            return cycle;
        }

        public async Task<EvaluationCycle> UpdateAsync(EvaluationCycle cycle)
        {
            _context.EvaluationCycles.Update(cycle);
            await _context.SaveChangesAsync();
            return cycle;
        }

        public async Task<bool> ExistsAsync(int cycleId)
        {
            return await _context.EvaluationCycles.AnyAsync(c => c.CycleId == cycleId);
        }

        public async Task<bool> HasOverlappingCycleAsync(
            DateOnly periodStart,
            DateOnly periodEnd,
            int? excludeCycleId = null)
        {
            var query = _context.EvaluationCycles
                .Where(c => c.Status == "Active" || c.Status == "Draft");

            if (excludeCycleId.HasValue)
            {
                query = query.Where(c => c.CycleId != excludeCycleId.Value);
            }

            var overlappingCycles = await query
                .Where(c =>
                    (c.EvaluationPeriodStart <= periodEnd && c.EvaluationPeriodEnd >= periodStart))
                .ToListAsync();

            if (!overlappingCycles.Any())
                return false;

            foreach (var cycle in overlappingCycles)
            {
                if (string.IsNullOrEmpty(cycle.ApplicableDepartments) || cycle.ApplicableDepartments == "All")
                {
                    return true;
                }
            }

            return false;
        }

    }
}
