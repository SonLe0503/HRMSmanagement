namespace HRManagement.DTOs
{
    public class OvertimeRequestResponseDTO
    {
        public int OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = null!;
        public DateOnly OvertimeDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal TotalHours { get; set; }
        public string Status { get; set; } = null!;
        public string? Reason { get; set; }
        public string? TaskDescription { get; set; }
        public DateTime SubmittedDate { get; set; }
    }
}
