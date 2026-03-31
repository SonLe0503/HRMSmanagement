namespace HRManagement.DTOs.LeaveBalance
{
    public class MyLeaveBalanceDTO
    {
        public int LeaveTypeId { get; set; }

        public string LeaveTypeName { get; set; } = string.Empty;

        public decimal TotalEntitlement { get; set; }

        public decimal UsedDays { get; set; }

        public decimal RemainingDays { get; set; }

        public decimal CarriedForward { get; set; }

        public int Year { get; set; }
    }
}
