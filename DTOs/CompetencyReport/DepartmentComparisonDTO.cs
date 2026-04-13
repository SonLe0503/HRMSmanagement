namespace HRManagement.DTOs.CompetencyReport
{
    public class DepartmentComparisonDTO
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
    }
}
