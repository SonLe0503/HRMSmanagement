using HRManagement.DataAcess.Interfaces;
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.DataAcess.Implementations
{
    public class EvaluationTemplateRepository : IEvaluationTemplateRepository
    {
        private readonly HrmsDbContext _context;

        public EvaluationTemplateRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EvaluationTemplate>> GetAllAsync()
        {
            return await _context.EvaluationTemplates
                .Include(x => x.EvaluationCriteria)
                .OrderBy(x => x.TemplateName)
                .ToListAsync();
        }

        public async Task<IEnumerable<EvaluationTemplate>> GetActiveAsync()
        {
            return await _context.EvaluationTemplates
                .Include(x => x.EvaluationCriteria)
                .Where(x => x.IsActive)
                .OrderBy(x => x.TemplateName)
                .ToListAsync();
        }

        public async Task<EvaluationTemplate?> GetByIdAsync(int id)
        {
            return await _context.EvaluationTemplates.FindAsync(id);
        }

        public async Task<EvaluationTemplate?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.EvaluationTemplates
                .Include(x => x.EvaluationCriteria)
                .FirstOrDefaultAsync(x => x.TemplateId == id);
        }

        public async Task<EvaluationTemplate> AddAsync(EvaluationTemplate template)
        {
            await _context.EvaluationTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<EvaluationTemplate> UpdateAsync(EvaluationTemplate template)
        {
            _context.EvaluationTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.EvaluationTemplates.AnyAsync(x => x.TemplateId == id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.EvaluationTemplates
                .Where(x => x.TemplateName == name);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.TemplateId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
