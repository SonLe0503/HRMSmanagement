using HRManagement.Models;

namespace HRManagement.DataAcess
{
    public interface IEvaluationRatingRepository  
    {
        Task<IEnumerable<EvaluationRating>> GetByEvaluationIdAsync(int evaluationId);
        Task<EvaluationRating?> GetByIdAsync(int ratingId);
        Task<EvaluationRating?> GetByEvaluationAndCriteriaAsync(int evaluationId, int criteriaId);
        Task<EvaluationRating> AddAsync(EvaluationRating rating);
        Task<EvaluationRating> UpdateAsync(EvaluationRating rating);
        Task<bool> DeleteAsync(int ratingId);
        Task<int> SaveChangesAsync();
    }
}
