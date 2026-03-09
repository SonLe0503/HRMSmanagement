namespace HRManagement.DTOs
{
    public class UpcomingLeaveItemDTO
    {
        public int LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal NumberOfDays { get; set; }
    }
}
