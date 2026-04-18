namespace HRManagement.DTOs.OvertimeRequest
{
    public class PendingOvertimeRequestDTO
    {
        public int OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly OvertimeDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal TotalHours { get; set; }
        public string? OTType { get; set; }
        public string? OTMode { get; set; }
        public string? Reason { get; set; }
        public string? TaskDescription { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public bool IsTopLevel { get; set; }
    }
}
