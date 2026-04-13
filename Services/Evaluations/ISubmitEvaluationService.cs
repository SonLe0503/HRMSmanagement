using HRManagement.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRManagement.Services.Evaluations
{
    public interface ISubmitEvaluationService
    {
        Task<EvaluationDetailDto> SubmitSelfEvaluationAsync(SubmitSelfEvaluationDto dto);
        Task<EvaluationDetailDto> SubmitManagerEvaluationAsync(SubmitManagerEvaluationDto dto);
        Task<EvaluationDetailDto> SaveEvaluationDraftAsync(SaveEvaluationDraftDto dto);
        Task<IEnumerable<PendingEvaluationDto>> GetPendingEvaluationsForManagerAsync(int evaluatorId);
        Task<EvaluationDetailDto?> GetEvaluationDetailAsync(int evaluationId);
    }
}
