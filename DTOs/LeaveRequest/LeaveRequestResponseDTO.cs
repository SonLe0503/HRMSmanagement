namespace HRManagement.DTOs.LeaveRequest
{
    public class LeaveRequestResponseDTO
    {
        public int LeaveRequestID { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal NumberOfDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string MessageCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public decimal? CurrentBalance { get; set; }
        public decimal? RemainingAfterRequest { get; set; }
    }
}
