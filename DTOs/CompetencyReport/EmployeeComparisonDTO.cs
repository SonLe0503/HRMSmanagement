namespace HRManagement.DTOs.CompetencyReport
{
    public class EmployeeComparisonDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public decimal EmployeeAverageRating { get; set; }
        public decimal TeamAverageRating { get; set; }
    }
}
