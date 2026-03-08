namespace HRManagement.DTOs
{
    public class LeaveRequestResponseDTO
    {
        public int LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal NumberOfDays { get; set; }

        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime SubmittedDate { get; set; }

        public DateTime? ReviewedDate { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewerComments { get; set; }

        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedBy { get; set; }

        public string? MessageCode { get; set; }
        public string? Message { get; set; }
    }
}
