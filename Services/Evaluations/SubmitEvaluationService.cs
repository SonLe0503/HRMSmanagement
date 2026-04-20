using HRManagement.DataAcess;
using HRManagement.Services.CurrentUsers;
using HRManagement.DTOs;
using HRManagement.Models;

namespace HRManagement.Services.Evaluations
{
    public class SubmitEvaluationService : ISubmitEvaluationService
    {
        private readonly IEvaluationRepository _evaluationRepository;
        private readonly IEvaluationRatingRepository _ratingRepository;
        private readonly IEvaluationCycleRepository _cycleRepository;
        private readonly ICurrentUserService _currentUserService;

        private const string STATUS_NOT_STARTED = "Not Started";
        private const string STATUS_SELF_EVALUATION = "Self Evaluation";
        private const string STATUS_MANAGER_EVALUATION = "Manager Evaluation";
        private const string STATUS_UNDER_REVIEW = "Under Review";
        private const string STATUS_COMPLETED = "Completed";
        private const string STATUS_ACKNOWLEDGED = "Acknowledged";

        public SubmitEvaluationService(
            IEvaluationRepository evaluationRepository,
            IEvaluationRatingRepository ratingRepository,
            IEvaluationCycleRepository cycleRepository,
            ICurrentUserService currentUserService)
        {
            _evaluationRepository = evaluationRepository;
            _ratingRepository = ratingRepository;
            _cycleRepository = cycleRepository;
            _currentUserService = currentUserService;
        }

        public async Task<EvaluationDetailDto> SubmitSelfEvaluationAsync(SubmitSelfEvaluationDto dto)
        {
            var evaluation = await _evaluationRepository.GetByIdWithDetailsAsync(dto.EvaluationId);
            if (evaluation == null)
            {
                throw new KeyNotFoundException("Evaluation not found.");
            }

            int currentUserId = await _currentUserService.GetCurrentEmployeeIdAsync();
            if (evaluation.EmployeeId != currentUserId)
            {
                throw new InvalidOperationException("You can only submit self-evaluation for your own evaluation. (Current ID: " + currentUserId + ", Target ID: " + evaluation.EmployeeId + ")");
            }

            var cycle = evaluation.Cycle;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (today < cycle.SelfEvaluationStart || today > cycle.SelfEvaluationEnd)
            {
                throw new InvalidOperationException(
                    $"Self-evaluation period is from {cycle.SelfEvaluationStart} to {cycle.SelfEvaluationEnd}.");
            }

            ValidateAllCriteriaRated(dto.Ratings, evaluation.Template.EvaluationCriteria.ToList());

            foreach (var ratingDto in dto.Ratings)
            {
                var existingRating = await _ratingRepository.GetByEvaluationAndCriteriaAsync(
                    dto.EvaluationId,
                    ratingDto.CriteriaId);

                if (existingRating != null)
                {
                    existingRating.SelfRating = ratingDto.SelfRating;
                    existingRating.SelfComments = ratingDto.SelfComments;
                    existingRating.Evidence = ratingDto.Evidence;
                    await _ratingRepository.UpdateAsync(existingRating);
                }
                else
                {
                    // Create new
                    var newRating = new EvaluationRating
                    {
                        EvaluationId = dto.EvaluationId,
                        CriteriaId = ratingDto.CriteriaId,
                        SelfRating = ratingDto.SelfRating,
                        SelfComments = ratingDto.SelfComments,
                        Evidence = ratingDto.Evidence
                    };
                    await _ratingRepository.AddAsync(newRating);
                }
            }

            await _ratingRepository.SaveChangesAsync();

            evaluation.Status = STATUS_SELF_EVALUATION;
            await _evaluationRepository.UpdateAsync(evaluation);
            return await GetEvaluationDetailAsync(dto.EvaluationId)
                   ?? throw new InvalidOperationException("Failed to retrieve evaluation detail.");
        }

        public async Task<EvaluationDetailDto> SubmitManagerEvaluationAsync(SubmitManagerEvaluationDto dto)
        {
            var evaluation = await _evaluationRepository.GetByIdWithDetailsAsync(dto.EvaluationId);
            if (evaluation == null)
            {
                throw new KeyNotFoundException("Evaluation not found.");
            }

            int currentUserId = await _currentUserService.GetCurrentEmployeeIdAsync();
            var currentEmployee = evaluation.Employee;

            if (evaluation.PrimaryEvaluatorId != currentUserId &&
                evaluation.SecondaryEvaluatorId != currentUserId)
            {
                throw new InvalidOperationException("You are not assigned as an evaluator for this evaluation. (Current ID: " + currentUserId + ", Primary: " + evaluation.PrimaryEvaluatorId + ")");
            }
            var cycle = evaluation.Cycle;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (today < cycle.ManagerEvaluationStart || today > cycle.ManagerEvaluationEnd)
            {
                throw new InvalidOperationException(
                    $"Manager evaluation period is from {cycle.ManagerEvaluationStart} to {cycle.ManagerEvaluationEnd}.");
            }

            ValidateManagerEvaluationCompleteness(dto, evaluation.Template.EvaluationCriteria.ToList());

            // Calculate expected overall rating from manager ratings and weightages
            decimal calculatedOverallRating = CalculateOverallRating(dto.Ratings, evaluation.Template.EvaluationCriteria.ToList());

            // Validate that frontend calculation matches backend calculation (within tolerance)
            if (Math.Abs((double)(dto.OverallRating.Value - calculatedOverallRating)) > 0.1)
            {
                throw new InvalidOperationException(
                    $"Overall rating calculation mismatch. Expected: {calculatedOverallRating:F1}, Received: {dto.OverallRating:F1}. " +
                    $"Please ensure the calculation is correct: Σ(managerRating × weightage/100)");
            }

            foreach (var ratingDto in dto.Ratings)
            {
                var existingRating = await _ratingRepository.GetByEvaluationAndCriteriaAsync(
                    dto.EvaluationId,
                    ratingDto.CriteriaId);

                if (existingRating != null)
                {
                    existingRating.ManagerRating = ratingDto.ManagerRating;
                    existingRating.ManagerComments = ratingDto.ManagerComments;

                    if (!string.IsNullOrWhiteSpace(ratingDto.Evidence))
                    {
                        existingRating.Evidence = ratingDto.Evidence;
                    }

                    await _ratingRepository.UpdateAsync(existingRating);
                }
                else
                {
                    var newRating = new EvaluationRating
                    {
                        EvaluationId = dto.EvaluationId,
                        CriteriaId = ratingDto.CriteriaId,
                        ManagerRating = ratingDto.ManagerRating,
                        ManagerComments = ratingDto.ManagerComments,
                        Evidence = ratingDto.Evidence
                    };
                    await _ratingRepository.AddAsync(newRating);
                }
            }

            await _ratingRepository.SaveChangesAsync();

            // Step 15: System saves the evaluation
            evaluation.Status = STATUS_COMPLETED;
            evaluation.OverallRating = dto.OverallRating;
            evaluation.SubmittedDate = DateTime.UtcNow;

            await _evaluationRepository.UpdateAsync(evaluation);

            return await GetEvaluationDetailAsync(dto.EvaluationId)
                   ?? throw new InvalidOperationException("Failed to retrieve evaluation detail.");
        }

        public async Task<EvaluationDetailDto> SaveEvaluationDraftAsync(SaveEvaluationDraftDto dto)
        {
            var evaluation = await _evaluationRepository.GetByIdWithDetailsAsync(dto.EvaluationId);
            if (evaluation == null)
            {
                throw new KeyNotFoundException("Evaluation not found.");
            }

            int currentUserId = await _currentUserService.GetCurrentEmployeeIdAsync();

            if (evaluation.PrimaryEvaluatorId != currentUserId &&
                evaluation.SecondaryEvaluatorId != currentUserId)
            {
                throw new InvalidOperationException("You are not assigned as an evaluator for this evaluation. (Current ID: " + currentUserId + ", Primary: " + evaluation.PrimaryEvaluatorId + ")");
            }

            foreach (var ratingDto in dto.Ratings)
            {
                var existingRating = await _ratingRepository.GetByEvaluationAndCriteriaAsync(
                    dto.EvaluationId,
                    ratingDto.CriteriaId);

                if (existingRating != null)
                {
                    existingRating.ManagerRating = ratingDto.ManagerRating;
                    existingRating.ManagerComments = ratingDto.ManagerComments;
                    existingRating.Evidence = ratingDto.Evidence;
                    await _ratingRepository.UpdateAsync(existingRating);
                }
                else
                {
                    var newRating = new EvaluationRating
                    {
                        EvaluationId = dto.EvaluationId,
                        CriteriaId = ratingDto.CriteriaId,
                        ManagerRating = ratingDto.ManagerRating,
                        ManagerComments = ratingDto.ManagerComments,
                        Evidence = ratingDto.Evidence
                    };
                    await _ratingRepository.AddAsync(newRating);
                }
            }

            await _ratingRepository.SaveChangesAsync();

            if (evaluation.Status == STATUS_NOT_STARTED || evaluation.Status == STATUS_SELF_EVALUATION)
            {
                evaluation.Status = STATUS_MANAGER_EVALUATION;
                await _evaluationRepository.UpdateAsync(evaluation);
            }

            return await GetEvaluationDetailAsync(dto.EvaluationId)
                   ?? throw new InvalidOperationException("Failed to retrieve evaluation detail.");
        }

        public async Task<IEnumerable<PendingEvaluationDto>> GetPendingEvaluationsForManagerAsync(int evaluatorId)
        {
            var evaluations = await _evaluationRepository.GetByEvaluatorIdAsync(evaluatorId);

            var pendingEvaluations = evaluations
                .Where(e => e.Status != STATUS_COMPLETED && e.Status != STATUS_ACKNOWLEDGED)
                .Select(e => new PendingEvaluationDto
                {
                    EvaluationId = e.EvaluationId,
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.Employee?.FullName ?? "N/A",
                    EmployeeDepartment = e.Employee?.Department?.DepartmentName ?? "N/A",
                    EmployeePosition = e.Employee?.Position?.PositionName ?? "N/A",
                    Status = e.Status,
                    Deadline = e.Cycle.ManagerEvaluationEnd,
                    SelfEvaluationCompleted = e.Status == STATUS_SELF_EVALUATION ||
                                             e.Status == STATUS_MANAGER_EVALUATION ||
                                             e.Status == STATUS_COMPLETED,
                    SelfEvaluationStart = e.Cycle.SelfEvaluationStart,
                    SelfEvaluationEnd = e.Cycle.SelfEvaluationEnd,
                    ManagerEvaluationStart = e.Cycle.ManagerEvaluationStart,
                    ManagerEvaluationEnd = e.Cycle.ManagerEvaluationEnd
                })
                .ToList();

            return pendingEvaluations;
        }

        public async Task<EvaluationDetailDto?> GetEvaluationDetailAsync(int evaluationId)
        {
            var evaluation = await _evaluationRepository.GetByIdWithDetailsAsync(evaluationId);
            if (evaluation == null)
                return null;

            var criteriaList = evaluation.Template?.EvaluationCriteria ?? new List<EvaluationCriterion>();
            var ratings = await _ratingRepository.GetByEvaluationIdAsync(evaluationId);

            var ratingsDtoList = criteriaList.Select(c => {
                var r = ratings.FirstOrDefault(x => x.CriteriaId == c.CriteriaId);
                return new EvaluationRatingResponseDto
                {
                    RatingId = r?.RatingId ?? 0,
                    EvaluationId = evaluationId,
                    CriteriaId = c.CriteriaId,
                    CriteriaName = c.CriteriaName,
                    CriteriaCategory = c.CriteriaCategory,
                    Weightage = c.Weightage,
                    SelfRating = r?.SelfRating,
                    SelfComments = r?.SelfComments,
                    ManagerRating = r?.ManagerRating,
                    ManagerComments = r?.ManagerComments,
                    Evidence = r?.Evidence
                };
            }).ToList();

            return new EvaluationDetailDto
            {
                EvaluationId = evaluation.EvaluationId,
                CycleId = evaluation.CycleId,
                CycleName = evaluation.Cycle?.CycleName ?? "N/A",
                EmployeeId = evaluation.EmployeeId,
                EmployeeCode = evaluation.Employee?.EmployeeCode ?? "N/A",
                EmployeeName = evaluation.Employee?.FullName ?? "N/A",
                EmployeeDepartment = evaluation.Employee?.Department?.DepartmentName ?? "N/A",
                EmployeePosition = evaluation.Employee?.Position?.PositionName ?? "N/A",
                TemplateId = evaluation.TemplateId,
                TemplateName = evaluation.Template?.TemplateName ?? "N/A",
                PrimaryEvaluatorId = evaluation.PrimaryEvaluatorId,
                PrimaryEvaluatorName = evaluation.PrimaryEvaluator?.FullName,
                SecondaryEvaluatorId = evaluation.SecondaryEvaluatorId,
                SecondaryEvaluatorName = evaluation.SecondaryEvaluator?.FullName,
                Status = evaluation.Status,
                OverallRating = evaluation.OverallRating,
                SubmittedDate = evaluation.SubmittedDate,
                AcknowledgedDate = evaluation.AcknowledgedDate,
                AcknowledgementComments = evaluation.AcknowledgementComments,
                Ratings = ratingsDtoList,
                SelfEvaluationStart = evaluation.Cycle?.SelfEvaluationStart ?? default,
                SelfEvaluationEnd = evaluation.Cycle?.SelfEvaluationEnd ?? default,
                ManagerEvaluationStart = evaluation.Cycle?.ManagerEvaluationStart ?? default,
                ManagerEvaluationEnd = evaluation.Cycle?.ManagerEvaluationEnd ?? default
            };
        }

        #region Validation

        private void ValidateAllCriteriaRated(List<CriterionRatingDto> ratings, List<EvaluationCriterion> criteria)
        {
            var ratedCriteriaIds = ratings.Select(r => r.CriteriaId).ToList();
            var allCriteriaIds = criteria.Select(c => c.CriteriaId).ToList();

            var missingCriteria = allCriteriaIds.Except(ratedCriteriaIds).ToList();

            if (missingCriteria.Any())
            {
                throw new InvalidOperationException(
                    $"All criteria must be rated. Missing ratings for {missingCriteria.Count} criteria.");
            }

            foreach (var rating in ratings)
            {
                if (!rating.SelfRating.HasValue)
                {
                    var criteriaName = criteria.FirstOrDefault(c => c.CriteriaId == rating.CriteriaId)?.CriteriaName;
                    throw new InvalidOperationException($"Rating is required for criteria: {criteriaName}");
                }
            }
        }

        private void ValidateManagerEvaluationCompleteness(SubmitManagerEvaluationDto dto, List<EvaluationCriterion> criteria)
        {
            var ratedCriteriaIds = dto.Ratings.Select(r => r.CriteriaId).ToList();
            var allCriteriaIds = criteria.Select(c => c.CriteriaId).ToList();

            var missingCriteria = allCriteriaIds.Except(ratedCriteriaIds).ToList();

            if (missingCriteria.Any())
            {
                throw new InvalidOperationException(
                    $"All criteria must be rated. Missing ratings for {missingCriteria.Count} criteria.");
            }

            foreach (var rating in dto.Ratings)
            {
                if (!rating.ManagerRating.HasValue)
                {
                    var criteriaName = criteria.FirstOrDefault(c => c.CriteriaId == rating.CriteriaId)?.CriteriaName;
                    throw new InvalidOperationException($"Manager rating is required for criteria: {criteriaName}");
                }
            }

            if (!dto.OverallRating.HasValue)
            {
                throw new InvalidOperationException("Overall rating is required.");
            }
        }

        private decimal CalculateOverallRating(List<CriterionRatingDto> ratings, List<EvaluationCriterion> criteria)
        {
            decimal totalScore = 0;

            foreach (var rating in ratings)
            {
                var criterion = criteria.FirstOrDefault(c => c.CriteriaId == rating.CriteriaId);
                if (criterion == null || !rating.ManagerRating.HasValue)
                {
                    continue; // Skip if criterion not found or no manager rating
                }

                decimal weightage = criterion.Weightage;
                decimal managerRating = rating.ManagerRating.Value;

                totalScore += managerRating * (weightage / 100m);
            }

            return Math.Round(totalScore, 1);
        }

        #endregion
    }
}
