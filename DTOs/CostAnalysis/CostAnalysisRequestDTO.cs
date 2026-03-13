namespace HRManagement.DTOs.CostAnalysis
{
    public class CostAnalysisRequestDTO
    {
        public string TimePeriod { get; set; } = "monthly"; // monthly, quarterly, yearly, custom
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public string BreakdownLevel { get; set; } = "company"; // company, division, department, project
        public int? DepartmentId { get; set; }

        public List<string> CostCategories { get; set; } = new(); // salaries, benefits, training, recruitment
        public string ComparisonPeriod { get; set; } = "none"; // none, yoy, mom, custom
    }
}