namespace HRManagement.DTOs.CompetencyReport
{
    public class CompetencyTrendDTO
    {
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = string.Empty;
        public List<CompetencyTrendPointDTO> Points { get; set; } = new();
    }

    public class CompetencyTrendPointDTO
    {
        public int CycleId { get; set; }
        public string CycleName { get; set; } = string.Empty;
        public decimal AverageManagerRating { get; set; }
        public decimal? AverageSelfRating { get; set; }
    }
}
