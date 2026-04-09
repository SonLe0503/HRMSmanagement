namespace HRManagement.DTOs.CompetencyReport
{
    public class CompetencyDrilldownRequestDTO
    {
        public int CriteriaId { get; set; }
        public int? CycleId { get; set; }
        public int? DepartmentId { get; set; }
        public int? EmployeeId { get; set; }
    }
}
