namespace HRManagement.DTOs.CompetencyReport
{
    public class CompetencyReportItemDTO
    {
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = string.Empty;
        public string? CriteriaCategory { get; set; }
        public decimal AverageManagerRating { get; set; }
        public decimal? AverageSelfRating { get; set; }
        public decimal Gap { get; set; }
    }
}
