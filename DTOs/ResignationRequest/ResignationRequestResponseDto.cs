namespace HRManagement.DTOs.ResignationRequest
{
    public class ResignationRequestResponseDto
    {
        public int ResignationRequestId { get; set; }
        public string RequestNumber { get; set; } = null!;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public string EmployeeCode { get; set; } = null!;
        public DateOnly ExpectedLastWorkingDate { get; set; }
        public string? Reason { get; set; }
        public string? HandoverNote { get; set; }
        public int? HandoverToEmployeeId { get; set; }
        public string? HandoverToEmployeeName { get; set; }
        public string Status { get; set; } = null!;
        public string? RejectionReason { get; set; }
        public string? ReviewerComments { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime SubmittedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public int IncompleteTaskCount { get; set; }
    }
}
