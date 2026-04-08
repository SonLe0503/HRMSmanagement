namespace HRManagement.DTOs.WorkforceAnalytics
{
    public class WorkforceAnalyticsRequestDTO
    {
        public string TimePeriod { get; set; } = "monthly"; // monthly, quarterly, yearly, custom
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public string OrganizationLevel { get; set; } = "company"; // company, division, department, team
        public int? DepartmentId { get; set; }
        public int? ManagerEmployeeId { get; set; }

        public string EmployeeGroup { get; set; } = "all"; // all, full-time, part-time, contract
        public string ComparisonPeriod { get; set; } = "none";
    }
}
