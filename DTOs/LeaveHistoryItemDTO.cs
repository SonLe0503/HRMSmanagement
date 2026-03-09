namespace HRManagement.DTOs
{
    public class LeaveHistoryItemDTO
    {
        public int LeaveRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal NumberOfDays { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
