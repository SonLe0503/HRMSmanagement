using HRManagement.DTOs.CompetencyReport;

namespace HRManagement.Services.Analytics
{
    public interface ICompetencyReportService
    {
        Task<CompetencyReportResponseDTO> GenerateReportAsync(CompetencyReportFilterDTO filter);
        Task<CompetencyDrilldownResponseDTO> GetDrilldownAsync(CompetencyDrilldownRequestDTO request);
        Task<(byte[] FileContent, string FileName, string ContentType)> ExportReportAsync(ExportCompetencyReportRequestDTO request);
    }
}

