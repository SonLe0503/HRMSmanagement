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

        [Required]
        [Range(1, 100)]
        public int DisplayOrder { get; set; }
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

        [Required]
        [Range(1, 100)]
        public int DisplayOrder { get; set; }
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
}
