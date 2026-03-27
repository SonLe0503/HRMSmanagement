namespace HRManagement.DTOs.LeaveRequest
{
    public class LeaveBalanceDTO
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int Year { get; set; }

        public decimal TotalEntitlement { get; set; }
        public decimal UsedDays { get; set; }
        public decimal CarriedForward { get; set; }

        public decimal RemainingDays { get; set; }
    }
}
