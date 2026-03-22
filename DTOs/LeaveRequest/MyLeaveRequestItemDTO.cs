namespace HRManagement.DTOs.LeaveRequest
{
    public class MyLeaveRequestItemDTO
    {
        public int LeaveRequestID { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal NumberOfDays { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
    }
}
