using HRManagement.DataAcess;
using HRManagement.DTOs;

namespace HRManagement.Services
{
    public class ViewEvaluationResultService : IViewEvaluationResultService
    {
        private readonly IEvaluationRepository _evaluationRepository;
        private readonly IEvaluationRatingRepository _ratingRepository;
        private readonly ICurrentUserService _currentUserService;

        private const string STATUS_COMPLETED = "Completed";
        private const string STATUS_ACKNOWLEDGED = "Acknowledged";
        private const string STATUS_UNDER_REVIEW = "Under Review";

        public ViewEvaluationResultService(
            IEvaluationRepository evaluationRepository,
            IEvaluationRatingRepository ratingRepository,
            ICurrentUserService currentUserService)
        {
            _evaluationRepository = evaluationRepository;
            _ratingRepository = ratingRepository;
            _currentUserService = currentUserService;
        }

        public async Task<IEnumerable<EvaluationResultListDto>> GetAvailableResultsForEmployeeAsync(int employeeId)
        {
            var evaluations = await _evaluationRepository.GetByEmployeeIdAsync(employeeId);

            var completedEvaluations = evaluations
                .Where(e => e.Status == STATUS_COMPLETED || e.Status == STATUS_ACKNOWLEDGED || e.Status == STATUS_UNDER_REVIEW)
                .OrderByDescending(e => e.SubmittedDate)
                .ToList();

            return completedEvaluations.Select(e => new EvaluationResultListDto
            {
                EvaluationId = e.EvaluationId,
                CycleName = e.Cycle?.CycleName ?? "N/A",
                EvaluationPeriodStart = e.Cycle?.EvaluationPeriodStart ?? DateOnly.MinValue,
                EvaluationPeriodEnd = e.Cycle?.EvaluationPeriodEnd ?? DateOnly.MinValue,
                CompletionDate = e.SubmittedDate,
                OverallRating = e.OverallRating,
                Status = e.AcknowledgedDate.HasValue ? "Acknowledged" : "New",
                IsNew = !e.AcknowledgedDate.HasValue
            }).ToList();
        }

        public async Task<EvaluationResultDto> GetEvaluationResultAsync(int evaluationId)
        {
            var evaluation = await _evaluationRepository.GetByIdWithDetailsAsync(evaluationId);
            if (evaluation == null)
            {
                throw new KeyNotFoundException("Evaluation not found.");
            }

            if (evaluation.Status != STATUS_COMPLETED &&
                evaluation.Status != STATUS_ACKNOWLEDGED &&
                evaluation.Status != STATUS_UNDER_REVIEW)
            {
                throw new InvalidOperationException(
                    $"Evaluation is not yet finalized. Current status: {evaluation.Status}. " +
                    $"Expected release date: {evaluation.Cycle?.ManagerEvaluationEnd}");
            }

            var ratings = await _ratingRepository.GetByEvaluationIdAsync(evaluationId);

            var employeeEvaluations = await _evaluationRepository.GetByEmployeeIdAsync(evaluation.EmployeeId);
            var previousEvaluation = employeeEvaluations
                .Where(e => e.EvaluationId != evaluationId &&
                           (e.Status == STATUS_COMPLETED || e.Status == STATUS_ACKNOWLEDGED))
                .OrderByDescending(e => e.SubmittedDate)
                .FirstOrDefault();

            ComparisonDataDto? comparison = null;
            if (previousEvaluation != null)
            {
                comparison = new ComparisonDataDto
                {
                    PreviousOverallRating = previousEvaluation.OverallRating,
                    RatingChange = evaluation.OverallRating.HasValue && previousEvaluation.OverallRating.HasValue
                        ? evaluation.OverallRating.Value - previousEvaluation.OverallRating.Value
                        : null,
                    PerformanceTrend = DeterminePerformanceTrend(
                        previousEvaluation.OverallRating,
                        evaluation.OverallRating)
                };

            }

            return new EvaluationResultDto
            {
                EvaluationId = evaluation.EvaluationId,
                CycleName = evaluation.Cycle?.CycleName ?? "N/A",
                EvaluationPeriodStart = evaluation.Cycle?.EvaluationPeriodStart ?? DateOnly.MinValue,
                EvaluationPeriodEnd = evaluation.Cycle?.EvaluationPeriodEnd ?? DateOnly.MinValue,
                CompletionDate = evaluation.SubmittedDate,
                OverallRating = evaluation.OverallRating,
                Status = evaluation.Status,
                IsNew = !evaluation.AcknowledgedDate.HasValue,
                IsAcknowledged = evaluation.AcknowledgedDate.HasValue,
                PrimaryEvaluatorName = evaluation.PrimaryEvaluator?.FullName,
                SecondaryEvaluatorName = evaluation.SecondaryEvaluator?.FullName,
                CriteriaResults = ratings.Select(r => new CriteriaResultDto
                {
                    CriteriaId = r.CriteriaId,
                    CriteriaName = r.Criteria?.CriteriaName ?? "N/A",
                    CriteriaCategory = r.Criteria?.CriteriaCategory,
                    Description = r.Criteria?.Description,
                    Weightage = r.Criteria?.Weightage ?? 0,
                    SelfRating = r.SelfRating,
                    SelfComments = r.SelfComments,
                    ManagerRating = r.ManagerRating,
                    ManagerComments = r.ManagerComments,
                    Evidence = r.Evidence,
                    Difference = r.ManagerRating.HasValue && r.SelfRating.HasValue
                        ? r.ManagerRating.Value - r.SelfRating.Value
                        : null
                }).ToList(),
                ManagerFeedback = new EvaluationFeedbackDto
                {
                    KeyStrengths = "Strong technical skills, excellent problem solver",
                    DevelopmentAreas = "Improve public speaking and stakeholder management",
                    TrainingRecommendations = "Presentation skills workshop, Leadership training",
                    CareerDevelopmentSuggestions = "Consider team lead role in next quarter",
                    GoalsForNextPeriod = "Lead 2 major projects, mentor junior developers"
                },
                Comparison = comparison,
                SupportingDocuments = new List<string>() 
            };
        }

        public async Task<EvaluationChartDataDto> GetEvaluationChartDataAsync(int evaluationId)
        {
            var evaluation = await _evaluationRepository.GetByIdWithDetailsAsync(evaluationId);
            if (evaluation == null)
            {
                throw new KeyNotFoundException("Evaluation not found.");
            }

            var ratings = await _ratingRepository.GetByEvaluationIdAsync(evaluationId);

            return new EvaluationChartDataDto
            {
                CriteriaLabels = ratings.Select(r => r.Criteria?.CriteriaName ?? "N/A").ToList(),
                SelfRatings = ratings.Select(r => r.SelfRating).ToList(),
                ManagerRatings = ratings.Select(r => r.ManagerRating).ToList(),
                Weightages = ratings.Select(r => r.Criteria?.Weightage ?? 0).ToList()
            };
        }

        public async Task<PerformanceSummaryDto> GetPerformanceSummaryAsync(int employeeId)
        {
            var evaluations = await _evaluationRepository.GetByEmployeeIdAsync(employeeId);

            var completedEvaluations = evaluations
                .Where(e => e.Status == STATUS_COMPLETED || e.Status == STATUS_ACKNOWLEDGED)
                .OrderByDescending(e => e.SubmittedDate)
                .ToList();

            if (!completedEvaluations.Any())
            {
                return new PerformanceSummaryDto
                {
                    TotalEvaluations = 0,
                    RecentEvaluations = new List<EvaluationResultListDto>()
                };
            }

            var current = completedEvaluations.FirstOrDefault();
            var previous = completedEvaluations.Skip(1).FirstOrDefault();

            var allRatings = completedEvaluations
                .Where(e => e.OverallRating.HasValue)
                .Select(e => e.OverallRating!.Value)
                .ToList();

            return new PerformanceSummaryDto
            {
                CurrentOverallRating = current?.OverallRating,
                PreviousOverallRating = previous?.OverallRating,
                Change = current?.OverallRating != null && previous?.OverallRating != null
                    ? current.OverallRating.Value - previous.OverallRating.Value
                    : null,
                TrendDirection = DetermineTrendDirection(previous?.OverallRating, current?.OverallRating),
                TotalEvaluations = completedEvaluations.Count,
                AverageRating = allRatings.Any() ? allRatings.Average() : null,
                HighestRating = allRatings.Any() ? allRatings.Max() : null,
                LowestRating = allRatings.Any() ? allRatings.Min() : null,
                RecentEvaluations = completedEvaluations.Take(5).Select(e => new EvaluationResultListDto
                {
                    EvaluationId = e.EvaluationId,
                    CycleName = e.Cycle?.CycleName ?? "N/A",
                    EvaluationPeriodStart = e.Cycle?.EvaluationPeriodStart ?? DateOnly.MinValue,
                    EvaluationPeriodEnd = e.Cycle?.EvaluationPeriodEnd ?? DateOnly.MinValue,
                    CompletionDate = e.SubmittedDate,
                    OverallRating = e.OverallRating,
                    Status = e.Status,
                    IsNew = !e.AcknowledgedDate.HasValue
                }).ToList()
            };
        }

        public async Task<EvaluationResultDto> AcknowledgeEvaluationAsync(AcknowledgeEvaluationDto dto)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(dto.EvaluationId);
            if (evaluation == null)
            {
                throw new KeyNotFoundException("Evaluation not found.");
            }
            int currentUserId = await _currentUserService.GetCurrentEmployeeIdAsync();
            var employeeEval = await _evaluationRepository.GetByIdWithDetailsAsync(dto.EvaluationId);

            if (employeeEval?.Employee?.EmployeeId != currentUserId)
            {
                throw new InvalidOperationException("You can only acknowledge your own evaluation.");
            }

            if (evaluation.Status != STATUS_COMPLETED && evaluation.Status != STATUS_UNDER_REVIEW)
            {
                throw new InvalidOperationException("Can only acknowledge completed evaluations.");
            }

            evaluation.AcknowledgedDate = DateTime.UtcNow;
            evaluation.AcknowledgementComments = dto.AcknowledgementComments;

            if (evaluation.Status != STATUS_UNDER_REVIEW)
            {
                evaluation.Status = STATUS_ACKNOWLEDGED;
            }

            await _evaluationRepository.UpdateAsync(evaluation);
            return await GetEvaluationResultAsync(dto.EvaluationId);
        }

        public async Task<bool> RequestReviewAsync(RequestReviewDto dto)
        {
            var evaluation = await _evaluationRepository.GetByIdAsync(dto.EvaluationId);
            if (evaluation == null)
            {
                throw new KeyNotFoundException("Evaluation not found.");
            }
            int currentUserId = await _currentUserService.GetCurrentEmployeeIdAsync();
            var employeeEval = await _evaluationRepository.GetByIdWithDetailsAsync(dto.EvaluationId);

            if (employeeEval?.Employee?.EmployeeId != currentUserId)
            {
                throw new InvalidOperationException("You can only appeal your own evaluation.");
            }

            if (evaluation.Status != STATUS_COMPLETED)
            {
                throw new InvalidOperationException("Can only appeal completed evaluations.");
            }
            evaluation.Status = STATUS_UNDER_REVIEW;
            evaluation.AcknowledgementComments =
                $"APPEAL REQUEST:\n{dto.DisagreementPoints}\n\nEVIDENCE:\n{dto.SupportingEvidence}\n\nEXPLANATION:\n{dto.DetailedExplanation}";

            await _evaluationRepository.UpdateAsync(evaluation);
            return true;
        }

        #region Helper Methods

        private string DeterminePerformanceTrend(decimal? previousRating, decimal? currentRating)
        {
            if (!previousRating.HasValue || !currentRating.HasValue)
                return "N/A";

            var difference = currentRating.Value - previousRating.Value;

            if (difference > 0.2m)
                return "Improving";
            else if (difference < -0.2m)
                return "Declining";
            else
                return "Stable";
        }

        private string DetermineTrendDirection(decimal? previousRating, decimal? currentRating)
        {
            if (!previousRating.HasValue || !currentRating.HasValue)
                return "N/A";

            var difference = currentRating.Value - previousRating.Value;

            if (difference > 0)
                return "Up";
            else if (difference < 0)
                return "Down";
            else
                return "Stable";
        }

        #endregion
    }
}
