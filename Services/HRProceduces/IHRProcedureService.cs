using HRManagement.DTOs;

namespace HRManagement.Services.HRProceduces
{
    public interface IHRProcedureService
    {
        Task<HRProcedureResponseDto> SubmitProcedureAsync(CreateHRProcedureDto createDto);
        Task<IEnumerable<HRProcedureListDto>> GetAllProceduresAsync();
        Task<HRProcedureResponseDto?> GetProcedureByIdAsync(int procedureId);
        Task<IEnumerable<HRProcedureListDto>> GetPendingProceduresAsync();
        Task<IEnumerable<HRProcedureListDto>> GetProceduresByEmployeeAsync(int employeeId);
        Task<IEnumerable<HRProcedureListDto>> GetProceduresByStatusAsync(string status);
        Task<HRProcedureResponseDto> UpdateProcedureAsync(int procedureId, UpdateHRProcedureDto updateDto);
        Task<HRProcedureResponseDto> ApproveProcedureAsync(int procedureId, ApproveHRProcedureDto approveDto);
        Task<HRProcedureResponseDto> RejectProcedureAsync(int procedureId, RejectHRProcedureDto rejectDto);
        Task<bool> DeleteProcedureAsync(int procedureId);
        /// <summary>Phase 2: manually apply an Approved procedure whose EffectiveDate is today or earlier</summary>
        Task<HRProcedureResponseDto> ApplyApprovedProcedureAsync(int procedureId, int? appliedBy = null);
    }
}
