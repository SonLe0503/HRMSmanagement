using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess
{
    public class EvaluationCriteriaRepository : IEvaluationCriteriaRepository
    {
        private readonly HrmsDbContext _context;

        public EvaluationCriteriaRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EvaluationCriterion>> GetByTemplateIdAsync(int templateId)
        {
            return await _context.EvaluationCriteria
                .Where(c => c.TemplateId == templateId)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<EvaluationCriterion?> GetByIdAsync(int criteriaId)
        {
            return await _context.EvaluationCriteria.FindAsync(criteriaId);
        }

        public async Task<EvaluationCriterion> AddAsync(EvaluationCriterion criterion)
        {
            await _context.EvaluationCriteria.AddAsync(criterion);
            await _context.SaveChangesAsync();
            return criterion;
        }

        public async Task<EvaluationCriterion> UpdateAsync(EvaluationCriterion criterion)
        {
            _context.EvaluationCriteria.Update(criterion);
            await _context.SaveChangesAsync();
            return criterion;
        }

        public async Task<bool> DeleteAsync(int criteriaId)
        {
            var criterion = await _context.EvaluationCriteria.FindAsync(criteriaId);
            if (criterion == null)
                return false;

            _context.EvaluationCriteria.Remove(criterion);
            return true;
        }

        public async Task<bool> ExistsAsync(int criteriaId)
        {
            return await _context.EvaluationCriteria.AnyAsync(c => c.CriteriaId == criteriaId);
        }

        public async Task<bool> CriteriaNameExistsInTemplateAsync(int templateId, string criteriaName, int? excludeCriteriaId = null)
        {
            var query = _context.EvaluationCriteria
                .Where(c => c.TemplateId == templateId && c.CriteriaName == criteriaName);

            if (excludeCriteriaId.HasValue)
            {
                query = query.Where(c => c.CriteriaId != excludeCriteriaId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetTotalWeightageAsync(int templateId, int? excludeCriteriaId = null)
        {
            var query = _context.EvaluationCriteria
                .Where(c => c.TemplateId == templateId);

            if (excludeCriteriaId.HasValue)
            {
                query = query.Where(c => c.CriteriaId != excludeCriteriaId.Value);
            }

            return await query.SumAsync(c => c.Weightage);
        }
    }
}
