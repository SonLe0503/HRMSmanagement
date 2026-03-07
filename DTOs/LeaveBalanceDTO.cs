namespace HRManagement.DTOs
{
    public class LeaveBalanceDTO
    {
        public string LeaveType { get; set; } = null!;

        public decimal TotalEntitlement { get; set; }

        public decimal UsedDays { get; set; }

        public decimal RemainingDays { get; set; }

        public decimal CarriedForward { get; set; }

        public int Year { get; set; }
    }
}
