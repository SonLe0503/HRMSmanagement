namespace HRManagement.DTOs.CompetencyReport
{
    public class HighLowPerformerDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public decimal AverageRating { get; set; }
        public string Group { get; set; } = string.Empty;
    }
}
