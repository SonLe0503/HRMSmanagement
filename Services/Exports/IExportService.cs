using HRManagement.DTOs;

namespace HRManagement.Services.Exports
{
    public interface IExportService
    {
        Task<ExportResponseDTO> ExportAsync(ExportRequestDTO request);
    }
}

