namespace HRManagement.DTOs
{
    public class LeaveBalanceResponseDTO
    {
        public int EmployeeId { get; set; }
        public int Year { get; set; }

        public List<LeaveBalanceItemDTO> Balances { get; set; } = new();
        public List<LeaveHistoryItemDTO> LeaveHistory { get; set; } = new();
        public List<UpcomingLeaveItemDTO> UpcomingLeaves { get; set; } = new();

        public string? MessageCode { get; set; }
        public string? Message { get; set; }
    }
}
