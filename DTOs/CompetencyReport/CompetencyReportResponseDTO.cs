namespace HRManagement.DTOs.CompetencyReport
{
    public class CompetencyReportResponseDTO
    {
        public bool HasEnoughData { get; set; }
        public string? DisclaimerMessage { get; set; }
        public string Scope { get; set; } = string.Empty;
        public string? CycleName { get; set; }

        public List<CompetencyReportItemDTO> CompetencyProfiles { get; set; } = new();
        public List<CompetencyTrendDTO> Trends { get; set; } = new();
        public List<CompetencyReportItemDTO> Strengths { get; set; } = new();
        public List<CompetencyReportItemDTO> DevelopmentGaps { get; set; } = new();
        public List<EmployeeComparisonDTO> EmployeeComparisons { get; set; } = new();
        public List<DepartmentComparisonDTO> DepartmentComparisons { get; set; } = new();
        public List<HighLowPerformerDTO> HighLowPerformers { get; set; } = new();
    }
}
