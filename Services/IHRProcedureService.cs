using HRManagement.DTOs;

namespace HRManagement.Services
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
    }
}
