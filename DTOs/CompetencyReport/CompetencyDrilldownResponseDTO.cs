namespace HRManagement.DTOs.CompetencyReport
{
    public class CompetencyDrilldownResponseDTO
    {
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = string.Empty;
        public string? CriteriaCategory { get; set; }
        public List<CompetencyDrilldownItemDTO> Details { get; set; } = new();
    }

    public class CompetencyDrilldownItemDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public decimal? SelfRating { get; set; }
        public decimal? ManagerRating { get; set; }
        public string? SelfComments { get; set; }
        public string? ManagerComments { get; set; }
    }
}
