namespace HRManagement.DTOs
{
    public class EmployeeApprovalAnalysisDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsTopLevel { get; set; }
        public int? TargetApproverId { get; set; }
        public string? TargetApproverName { get; set; }
        public string ApprovalRouteType { get; set; } = null!; // Direct, TopLevelFallback, DefaultFallback, None
        public bool IsValid { get; set; }
    }
}
