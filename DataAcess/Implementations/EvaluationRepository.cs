using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess.Implementations
{
    public class EvaluationRepository : IEvaluationRepository
    {
        private readonly HrmsDbContext _context;

        public EvaluationRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Evaluation>> GetAllAsync()
        {
            return await _context.Evaluations
                .Include(e => e.Cycle)
                .Include(e => e.Employee)
                .Include(e => e.Template)
                .Include(e => e.PrimaryEvaluator)
                .Include(e => e.SecondaryEvaluator)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByCycleIdAsync(int cycleId)
        {
            return await _context.Evaluations
                .Include(e => e.Employee)
                    .ThenInclude(emp => emp.Department)
                .Include(e => e.Employee)
                    .ThenInclude(emp => emp.Position)
                .Include(e => e.Template)
                .Include(e => e.PrimaryEvaluator)
                .Include(e => e.SecondaryEvaluator)
                .Where(e => e.CycleId == cycleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.Evaluations
                .Include(e => e.Cycle)
                .Include(e => e.Template)
                .Include(e => e.PrimaryEvaluator)
                .Include(e => e.SecondaryEvaluator)
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.Cycle.EvaluationPeriodStart)
                .ToListAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetByEvaluatorIdAsync(int evaluatorId)
        {
            return await _context.Evaluations
                .Include(e => e.Cycle)
                .Include(e => e.Employee)
                    .ThenInclude(emp => emp.Department)
                .Include(e => e.Employee)
                    .ThenInclude(emp => emp.Position)
                .Include(e => e.Template)
                .Where(e => e.PrimaryEvaluatorId == evaluatorId || e.SecondaryEvaluatorId == evaluatorId)
                .ToListAsync();
        }

        public async Task<Evaluation?> GetByIdWithDetailsAsync(int evaluationId)
        {
            return await _context.Evaluations
                .Include(e => e.Cycle)
                .Include(e => e.Employee)
                    .ThenInclude(emp => emp.Department)
                .Include(e => e.Employee)
                    .ThenInclude(emp => emp.Position)
                .Include(e => e.Template)
                    .ThenInclude(t => t.EvaluationCriteria)
                .Include(e => e.PrimaryEvaluator)
                .Include(e => e.SecondaryEvaluator)
                .Include(e => e.EvaluationRatings)
                    .ThenInclude(r => r.Criteria)
                .FirstOrDefaultAsync(e => e.EvaluationId == evaluationId);
        }

        public async Task<Evaluation?> GetByIdAsync(int evaluationId)
        {
            return await _context.Evaluations.FindAsync(evaluationId);
        }

        public async Task<Evaluation> AddAsync(Evaluation evaluation)
        {
            await _context.Evaluations.AddAsync(evaluation);
            await _context.SaveChangesAsync();
            return evaluation;
        }

        public async Task<Evaluation> UpdateAsync(Evaluation evaluation)
        {
            _context.Evaluations.Update(evaluation);
            await _context.SaveChangesAsync();
            return evaluation;
        }

        public async Task<bool> ExistsAsync(int evaluationId)
        {
            return await _context.Evaluations.AnyAsync(e => e.EvaluationId == evaluationId);
        }

        public async Task<bool> EmployeeHasEvaluationInCycleAsync(int cycleId, int employeeId)
        {
            return await _context.Evaluations
                .AnyAsync(e => e.CycleId == cycleId && e.EmployeeId == employeeId);
        }

        public async Task<int> GetEvaluationCountByCycleAsync(int cycleId)
        {
            return await _context.Evaluations
                .Where(e => e.CycleId == cycleId)
                .CountAsync();
        }

        public async Task<int> GetAssignedEvaluatorCountAsync(int cycleId)
        {
            return await _context.Evaluations
                .Where(e => e.CycleId == cycleId && e.PrimaryEvaluatorId.HasValue)
                .Select(e => e.PrimaryEvaluatorId!.Value)
                .Distinct()
                .CountAsync();
        }

        public async Task<IEnumerable<Evaluation>> GetPendingEvaluationsByEvaluatorAsync(int evaluatorId)
        {
            return await _context.Evaluations
                .Include(e => e.Cycle)
                .Include(e => e.Employee)
                .Include(e => e.Template)
                .Where(e => (e.PrimaryEvaluatorId == evaluatorId || e.SecondaryEvaluatorId == evaluatorId) &&
                           (e.Status == "Not Started" ||
                            e.Status == "Self Evaluation" ||
                            e.Status == "Manager Evaluation"))
                .ToListAsync();
        }
    }
}
