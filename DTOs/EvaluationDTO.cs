using System.ComponentModel.DataAnnotations;

namespace HRManagement.DTOs
{
    public class CreateEvaluationTemplateDto
    {
        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateEvaluationTemplateDto
    {
        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class EvaluationTemplateResponseDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = null!;
        public string? Description { get; set; }
        public int CriteriaCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
    }

    public class EvaluationTemplateListDto
    {
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = null!;
        public bool IsActive { get; set; }
        public int CriteriaCount { get; set; }
    }

    public class CreateEvaluationCycleDto
    {
        [Required]
        [MaxLength(200)]
        public string CycleName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string CycleType { get; set; } = null!;

        [Required]
        public DateOnly EvaluationPeriodStart { get; set; }

        [Required]
        public DateOnly EvaluationPeriodEnd { get; set; }

        [Required]
        public DateOnly SelfEvaluationStart { get; set; }

        [Required]
        public DateOnly SelfEvaluationEnd { get; set; }

        [Required]
        public DateOnly ManagerEvaluationStart { get; set; }

        [Required]
        public DateOnly ManagerEvaluationEnd { get; set; }

        public DateOnly? ReviewMeetingStart { get; set; }

        public DateOnly? ReviewMeetingEnd { get; set; }
    }

    public class UpdateEvaluationCycleDto
    {
        [Required]
        [MaxLength(200)]
        public string CycleName { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string CycleType { get; set; } = null!;

        [Required]
        public DateOnly EvaluationPeriodStart { get; set; }

        [Required]
        public DateOnly EvaluationPeriodEnd { get; set; }

        [Required]
        public DateOnly SelfEvaluationStart { get; set; }

        [Required]
        public DateOnly SelfEvaluationEnd { get; set; }

        [Required]
        public DateOnly ManagerEvaluationStart { get; set; }

        [Required]
        public DateOnly ManagerEvaluationEnd { get; set; }

        public DateOnly? ReviewMeetingStart { get; set; }

        public DateOnly? ReviewMeetingEnd { get; set; }
    }

    public class EvaluationCycleSummaryDto
    {
        public int CycleId { get; set; }
        public string CycleName { get; set; } = null!;
        public string CycleType { get; set; } = null!;
        public DateOnly EvaluationPeriodStart { get; set; }
        public DateOnly EvaluationPeriodEnd { get; set; }
        public int EmployeeCount { get; set; }
        public int AssignedEvaluatorsCount { get; set; }
        public string Status { get; set; } = null!;
        public TimelineOverviewDto Timeline { get; set; } = null!;
    }

    public class TimelineOverviewDto
    {
        public DateOnly SelfEvaluationStart { get; set; }
        public DateOnly SelfEvaluationEnd { get; set; }
        public DateOnly ManagerEvaluationStart { get; set; }
        public DateOnly ManagerEvaluationEnd { get; set; }
        public DateOnly? ReviewMeetingStart { get; set; }
        public DateOnly? ReviewMeetingEnd { get; set; }
    }
    public class EvaluationCycleResponseDto
    {
        public int CycleId { get; set; }
        public string CycleName { get; set; } = null!;
        public string CycleType { get; set; } = null!;
        public DateOnly EvaluationPeriodStart { get; set; }
        public DateOnly EvaluationPeriodEnd { get; set; }
        public DateOnly SelfEvaluationStart { get; set; }
        public DateOnly SelfEvaluationEnd { get; set; }
        public DateOnly ManagerEvaluationStart { get; set; }
        public DateOnly ManagerEvaluationEnd { get; set; }
        public DateOnly? ReviewMeetingStart { get; set; }
        public DateOnly? ReviewMeetingEnd { get; set; }
        public string Status { get; set; } = null!;
        public int EmployeeCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
    }

    public class EvaluationCycleListDto
    {
        public int CycleId { get; set; }
        public string CycleName { get; set; } = null!;
        public string CycleType { get; set; } = null!;
        public DateOnly EvaluationPeriodStart { get; set; }
        public DateOnly EvaluationPeriodEnd { get; set; }
        public string Status { get; set; } = null!;
        public int EmployeeCount { get; set; }
    }

    public class ActivateCycleDto
    {
        public bool ConfirmActivation { get; set; } = true;
    }
    public class CloseCycleDto
    {
        [MaxLength(500)]
        public string? ClosureNotes { get; set; }
    }

    public class CreateEvaluationCriterionDto
    {
        [Required]
        [MaxLength(200)]
        public string CriteriaName { get; set; } = null!;

        [MaxLength(100)]
        public string? CriteriaCategory { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 100)]
        public int Weightage { get; set; }

        public int? DisplayOrder { get; set; }
    }
    public class UpdateEvaluationCriterionDto
    {
        [Required]
        [MaxLength(200)]
        public string CriteriaName { get; set; } = null!;

        [MaxLength(100)]
        public string? CriteriaCategory { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0, 100)]
        public int Weightage { get; set; }

        public int? DisplayOrder { get; set; }
    }
    public class EvaluationCriterionResponseDto
    {
        public int CriteriaId { get; set; }
        public int TemplateId { get; set; }
        public string CriteriaName { get; set; } = null!;
        public string? CriteriaCategory { get; set; }
        public string? Description { get; set; }
        public int Weightage { get; set; }
        public int DisplayOrder { get; set; }
    }
    public class EvaluationCriterionListDto
    {
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = null!;
        public string? CriteriaCategory { get; set; }
        public int Weightage { get; set; }
        public int DisplayOrder { get; set; }
    }
    public class BulkCreateCriteriaDto
    {
        [Required]
        public List<CreateEvaluationCriterionDto> Criteria { get; set; } = new();
    }
    public class EvaluatorAssignmentDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public int PrimaryEvaluatorId { get; set; }

        public int? SecondaryEvaluatorId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
    public class AssignEvaluatorsDto
    {
        [Required]
        public int CycleId { get; set; }

        [Required]
        [MinLength(1)]
        public List<EvaluatorAssignmentDto> Assignments { get; set; } = new();
    }
    public class AutoAssignEvaluatorsDto
    {
        [Required]
        public int CycleId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public bool IncludeSecondaryEvaluator { get; set; } = false;

        public int? DepartmentId { get; set; } 
    }

    public class BulkAssignByDepartmentDto
    {
        [Required]
        public int CycleId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public int PrimaryEvaluatorId { get; set; }

        public int? SecondaryEvaluatorId { get; set; }
    }
    public class EvaluationResponseDto
    {
        public int EvaluationId { get; set; }
        public int CycleId { get; set; }
        public string CycleName { get; set; } = null!;
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string EmployeeDepartment { get; set; } = null!;
        public string EmployeePosition { get; set; } = null!;
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = null!;
        public int? PrimaryEvaluatorId { get; set; }
        public string? PrimaryEvaluatorName { get; set; }
        public int? SecondaryEvaluatorId { get; set; }
        public string? SecondaryEvaluatorName { get; set; }
        public string Status { get; set; } = null!;
        public decimal? OverallRating { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
    }
    public class EvaluationListDto
    {
        public int EvaluationId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string EmployeeDepartment { get; set; } = null!;
        public string PrimaryEvaluatorName { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class AssignmentPreviewDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string Department { get; set; } = null!;
        public int? SuggestedPrimaryEvaluatorId { get; set; }
        public string? SuggestedPrimaryEvaluatorName { get; set; }
        public int? SuggestedSecondaryEvaluatorId { get; set; }
        public string? SuggestedSecondaryEvaluatorName { get; set; }
        public bool HasDirectManager { get; set; }
        public bool IsAssigned { get; set; }
        public string? Issue { get; set; }
    }
    public class AssignmentResultDto
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<EvaluationResponseDto> SuccessfulAssignments { get; set; } = new();
        public List<AssignmentErrorDto> Errors { get; set; } = new();
    }

    public class AssignmentErrorDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string ErrorMessage { get; set; } = null!;
    }

    public class ChangeEvaluatorDto
    {
        public int? PrimaryEvaluatorId { get; set; }
        public int? SecondaryEvaluatorId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class CriterionRatingDto
    {
        [Required]
        public int CriteriaId { get; set; }

        [Range(0, 5)]
        public decimal? SelfRating { get; set; }

        [MaxLength(1000)]
        public string? SelfComments { get; set; }

        [Range(0, 5)]
        public decimal? ManagerRating { get; set; }

        [MaxLength(1000)]
        public string? ManagerComments { get; set; }

        [MaxLength(2000)]
        public string? Evidence { get; set; }
    }

    public class SubmitSelfEvaluationDto
    {
        [Required]
        public int EvaluationId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CriterionRatingDto> Ratings { get; set; } = new();

        [MaxLength(2000)]
        public string? GeneralComments { get; set; }
    }
    public class SubmitManagerEvaluationDto
    {
        [Required]
        public int EvaluationId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CriterionRatingDto> Ratings { get; set; } = new();

        [MaxLength(2000)]
        public string? KeyStrengths { get; set; }

        [MaxLength(2000)]
        public string? DevelopmentAreas { get; set; }

        [MaxLength(2000)]
        public string? TrainingRecommendations { get; set; }

        [MaxLength(2000)]
        public string? CareerDevelopmentSuggestions { get; set; }

        [MaxLength(2000)]
        public string? GoalsForNextPeriod { get; set; }

        [Range(0, 5)]
        public decimal? OverallRating { get; set; }
    }

    public class EvaluationRatingResponseDto
    {
        public int RatingId { get; set; }
        public int EvaluationId { get; set; }
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = null!;
        public string? CriteriaCategory { get; set; }
        public int Weightage { get; set; }
        public decimal? SelfRating { get; set; }
        public string? SelfComments { get; set; }
        public decimal? ManagerRating { get; set; }
        public string? ManagerComments { get; set; }
        public string? Evidence { get; set; }
    }
    public class EvaluationDetailDto
    {
        public int EvaluationId { get; set; }
        public int CycleId { get; set; }
        public string CycleName { get; set; } = null!;
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string EmployeeName { get; set; } = null!;
        public string EmployeeDepartment { get; set; } = null!;
        public string EmployeePosition { get; set; } = null!;
        public int TemplateId { get; set; }
        public string TemplateName { get; set; } = null!;
        public int? PrimaryEvaluatorId { get; set; }
        public string? PrimaryEvaluatorName { get; set; }
        public int? SecondaryEvaluatorId { get; set; }
        public string? SecondaryEvaluatorName { get; set; }
        public string Status { get; set; } = null!;
        public decimal? OverallRating { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
        public string? AcknowledgementComments { get; set; }
        public List<EvaluationRatingResponseDto> Ratings { get; set; } = new();
        public EvaluationFeedbackDto? Feedback { get; set; }
        // Cycle timeline for time-gating
        public DateOnly SelfEvaluationStart { get; set; }
        public DateOnly SelfEvaluationEnd { get; set; }
        public DateOnly ManagerEvaluationStart { get; set; }
        public DateOnly ManagerEvaluationEnd { get; set; }
    }
    public class EvaluationFeedbackDto
    {
        public string? KeyStrengths { get; set; }
        public string? DevelopmentAreas { get; set; }
        public string? TrainingRecommendations { get; set; }
        public string? CareerDevelopmentSuggestions { get; set; }
        public string? GoalsForNextPeriod { get; set; }
    }
    public class SaveEvaluationDraftDto
    {
        [Required]
        public int EvaluationId { get; set; }

        public List<CriterionRatingDto> Ratings { get; set; } = new();
    }
    public class PendingEvaluationDto
    {
        public int EvaluationId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string EmployeeDepartment { get; set; } = null!;
        public string EmployeePosition { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateOnly Deadline { get; set; }
        public bool SelfEvaluationCompleted { get; set; }
        // Cycle timeline for time-gating
        public DateOnly SelfEvaluationStart { get; set; }
        public DateOnly SelfEvaluationEnd { get; set; }
        public DateOnly ManagerEvaluationStart { get; set; }
        public DateOnly ManagerEvaluationEnd { get; set; }
    }

    public class EvaluationResultDto
    {
        public int EvaluationId { get; set; }
        public int EmployeeId { get; set; }
        public string CycleName { get; set; } = null!;
        public DateOnly EvaluationPeriodStart { get; set; }
        public DateOnly EvaluationPeriodEnd { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal? OverallRating { get; set; }
        public string Status { get; set; } = null!;
        public bool IsNew { get; set; }
        public bool IsAcknowledged { get; set; }

        public string? PrimaryEvaluatorName { get; set; }
        public string? SecondaryEvaluatorName { get; set; }

        public List<CriteriaResultDto> CriteriaResults { get; set; } = new();

        public EvaluationFeedbackDto? ManagerFeedback { get; set; }

        public ComparisonDataDto? Comparison { get; set; }

        public List<string> SupportingDocuments { get; set; } = new();
    }
    public class CriteriaResultDto
    {
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = null!;
        public string? CriteriaCategory { get; set; }
        public string? Description { get; set; }
        public int Weightage { get; set; }
        public decimal? SelfRating { get; set; }
        public string? SelfComments { get; set; }
        public decimal? ManagerRating { get; set; }
        public string? ManagerComments { get; set; }
        public string? Evidence { get; set; }
        public decimal? Difference { get; set; } 
    }
    public class ComparisonDataDto
    {
        public decimal? PreviousOverallRating { get; set; }
        public decimal? RatingChange { get; set; }
        public decimal? TeamAverageRating { get; set; }
        public string? PerformanceTrend { get; set; } 
    }
    public class EvaluationResultListDto
    {
        public int EvaluationId { get; set; }
        public string CycleName { get; set; } = null!;
        public DateOnly EvaluationPeriodStart { get; set; }
        public DateOnly EvaluationPeriodEnd { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal? OverallRating { get; set; }
        public string Status { get; set; } = null!;
        public bool IsNew { get; set; }
    }
    public class AcknowledgeEvaluationDto
    {
        [Required]
        public int EvaluationId { get; set; }

        [MaxLength(2000)]
        public string? AcknowledgementComments { get; set; }
    }
    public class RequestReviewDto
    {
        [Required]
        public int EvaluationId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string DisagreementPoints { get; set; } = null!;

        [MaxLength(3000)]
        public string? SupportingEvidence { get; set; }

        [Required]
        [MaxLength(2000)]
        public string DetailedExplanation { get; set; } = null!;
    }
    public class EvaluationChartDataDto
    {
        public List<string> CriteriaLabels { get; set; } = new();
        public List<decimal?> SelfRatings { get; set; } = new();
        public List<decimal?> ManagerRatings { get; set; } = new();
        public List<int> Weightages { get; set; } = new();
    }
    public class PerformanceSummaryDto
    {
        public decimal? CurrentOverallRating { get; set; }
        public decimal? PreviousOverallRating { get; set; }
        public decimal? Change { get; set; }
        public string? TrendDirection { get; set; } 
        public int TotalEvaluations { get; set; }
        public decimal? AverageRating { get; set; }
        public decimal? HighestRating { get; set; }
        public decimal? LowestRating { get; set; }
        public List<EvaluationResultListDto> RecentEvaluations { get; set; } = new();
    }
}
