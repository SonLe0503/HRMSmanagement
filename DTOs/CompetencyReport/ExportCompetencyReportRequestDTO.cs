namespace HRManagement.DTOs.CompetencyReport
{
    public class ExportCompetencyReportRequestDTO
    {
        public CompetencyReportFilterDTO Filter { get; set; } = new();
        public string Format { get; set; } = "csv"; // csv | excel | pdf
    }
}
