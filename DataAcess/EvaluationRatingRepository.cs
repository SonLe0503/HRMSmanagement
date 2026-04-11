
using HRManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRManagement.DataAcess
{
    public class EvaluationRatingRepository : IEvaluationRatingRepository
    {
        private readonly HrmsDbContext _context;

        public EvaluationRatingRepository(HrmsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EvaluationRating>> GetByEvaluationIdAsync(int evaluationId)
        {
            return await _context.EvaluationRatings
                .Include(r => r.Criteria)
                .Where(r => r.EvaluationId == evaluationId)
                .OrderBy(r => r.Criteria.DisplayOrder)
                .ToListAsync();
        }

        public async Task<EvaluationRating?> GetByIdAsync(int ratingId)
        {
            return await _context.EvaluationRatings
                .Include(r => r.Criteria)
                .FirstOrDefaultAsync(r => r.RatingId == ratingId);
        }

        public async Task<EvaluationRating?> GetByEvaluationAndCriteriaAsync(int evaluationId, int criteriaId)
        {
            return await _context.EvaluationRatings
                .Include(r => r.Criteria)
                .FirstOrDefaultAsync(r => r.EvaluationId == evaluationId && r.CriteriaId == criteriaId);
        }

        public async Task<EvaluationRating> AddAsync(EvaluationRating rating)
        {
            await _context.EvaluationRatings.AddAsync(rating);
            return rating;
        }

        public async Task<EvaluationRating> UpdateAsync(EvaluationRating rating)
        {
            _context.EvaluationRatings.Update(rating);
            return rating;
        }

        public async Task<bool> DeleteAsync(int ratingId)
        {
            var rating = await _context.EvaluationRatings.FindAsync(ratingId);
            if (rating == null)
                return false;

            _context.EvaluationRatings.Remove(rating);
            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}