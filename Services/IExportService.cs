using HRManagement.DTOs;

namespace HRManagement.Services
{
    public interface IExportService
    {
        Task<ExportResponseDTO> ExportAsync(ExportRequestDTO request);
    }
}
