namespace HRManagement.DTOs
{
    public class LeaveBalanceItemDTO
    {
        public int LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;

        public int Year { get; set; }
        public decimal TotalEntitlement { get; set; }
        public decimal UsedDays { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal PendingDays { get; set; }
        public decimal CarriedForward { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
