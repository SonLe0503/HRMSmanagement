namespace HRManagement.DTOs.CompetencyReport
{
    public class CompetencyReportFilterDTO
    {
        public int? CycleId { get; set; }
        public List<int>? CriteriaIds { get; set; }
        public string? CriteriaCategory { get; set; }
        public string Scope { get; set; } = "Individual"; // Individual | Team | Organization
        public int? EmployeeId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
