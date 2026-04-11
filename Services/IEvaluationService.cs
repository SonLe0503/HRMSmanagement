using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface IEvaluationService
    {
        Task<AssignmentResultDto> AssignEvaluatorsAsync(AssignEvaluatorsDto dto);
        Task<AssignmentResultDto> AutoAssignEvaluatorsAsync(AutoAssignEvaluatorsDto dto);
        Task<List<AssignmentPreviewDto>> GetAssignmentPreviewAsync(int cycleId);
        Task<IEnumerable<EvaluationListDto>> GetEvaluationsByCycleAsync(int cycleId);
        Task<IEnumerable<EvaluationListDto>> GetEvaluationsByEmployeeAsync(int employeeId);
        Task<IEnumerable<EvaluationListDto>> GetEvaluationsByEvaluatorAsync(int evaluatorId);
        Task<EvaluationResponseDto?> GetEvaluationByIdAsync(int evaluationId);
        Task<EvaluationResponseDto> ChangeEvaluatorAsync(int evaluationId, ChangeEvaluatorDto dto);
    }
}
